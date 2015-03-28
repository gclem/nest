using Constellation.Host;
using FirebaseSharp.Portable;
using Nest.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nest
{
    public class Program : PackageBase
    {
        private const string NEST_ROOT_URI = "https://developer-api.nest.com";

        private NestObjectCollection nestRegister = null;

        static void Main(string[] args)
        {
            PackageHost.Start<Program>(args);
        }

        public override void OnStart()
        {
            //// Init
            this.InitDataObjects();

            PackageHost.WriteInfo("Connecting to NEST streaming API ... Waiting 3s...");

            // Subscribe to streaming for all kinds of objects
            this.SubscribeAll();

            // Push initial SO
            System.Threading.Thread.Sleep(3000);
            PackageHost.WriteInfo("Pushing inital state objects");
            foreach (var item in nestRegister)
            {
                foreach (var device in item.Value)
                {
                    PackageHost.PushStateObject(device.Value.name, device.Value, item.Key.SystemId);
                }
            }   
   
            // Started !
            PackageHost.WriteInfo("Connected to Nest streaming API");
        }

        /// <summary>
        /// Sets the away mode.
        /// </summary>
        /// <param name="isAway">if set to <c>true</c> if is away.</param>
        [MessageCallback]
        public void SetAwayMode(bool isAway, string id = "")
        {
            var structureStore = this.nestRegister.GetObjectStore(NestObjectType.STRUCTURE);

            if(string.IsNullOrEmpty(id) && structureStore.Count == 0)
            {
                PackageHost.WriteWarn("No Structure available to set away.");
                return;
            }

            if(!structureStore.ContainsKey(id))
            {
                PackageHost.WriteWarn("No Structure with this id is available to set away.");
                return;
            }

            this.SetProperty("structures/" + id == "" ? structureStore.First().Key : id, "away", isAway ? "away" : "home");
        }

        /// <summary>
        /// Sets the target temperature for the first thermostat.
        /// </summary>
        /// <param name="temperature">The temperature.</param>
        [MessageCallback]
        public void SetTargetTemperature(double temperature, string id = "")
        {
            var store = this.nestRegister.GetObjectStore(NestObjectType.THERMOSTATS);

            if (string.IsNullOrEmpty(id) && store.Count == 0)
            {
                PackageHost.WriteWarn("No thermostat available to set away.");
                return;
            }

            if (!store.ContainsKey(id))
            {
                PackageHost.WriteWarn("No thermostat with this id is available to set away.");
                return;
            }

            this.SetProperty("devices/thermostats/" + store.First().Key, "target_temperature_c", temperature);
        }

        /// <summary>
        ///  Init available data objects to subscribe
        /// </summary>
        private void InitDataObjects()
        {
            this.nestRegister = new NestObjectCollection();

            this.nestRegister.Add(new Thermostat());
            this.nestRegister.Add(new SmokeCoAlarm());
            this.nestRegister.Add(new Structure());
        }

        /// <summary>
        /// Sets the property for a specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        [MessageCallback]
        private void SetProperty(string path, string propertyName, object value)
        {
            this.Set(path, JsonConvert.SerializeObject(new Dictionary<string, object>() { { propertyName, value } }));
        }

        private void Set(string path, string payload)
        {
            var response = this.CreateFirebaseClient().Put(path, payload);
            PackageHost.WriteInfo("Put '{1}' to '{0}'", path, response);
        }

        private Firebase CreateFirebaseClient()
        {
            return new Firebase(NEST_ROOT_URI, PackageHost.GetSettingValue<string>("AccessToken"));
        }

        private void Subscribe(NestObjectConfiguration item)
        {
            //// Dictionary<string, dynamic> source, Regex pathRegex, string path, string soTypeName
            PackageHost.WriteInfo("Subscribing to {0} ({1})", item.BasePath, item.SystemId);
            Firebase fb = this.CreateFirebaseClient();

            var streaming = fb.GetStreaming(item.BasePath,
                added: (s, e) =>
                {
                    this.ProcessObject(item, e.Path, e.Data);
                },
                changed: (s, e) =>
                {
                    if (e.OldData != e.Data)
                    {
                        var obj = this.ProcessObject(item, e.Path, e.Data);
                        PackageHost.PushStateObject(obj.name, obj, item.SystemId);
                    }
                });
            streaming.Canceled += (s, e) =>
            {
                PackageHost.WriteWarn("Streaming of {0} ({1}) is cancelled", item.BasePath, item.SystemId);
                this.Subscribe(item);
            };
        }

        private void SubscribeAll()
        {
            foreach (var item in this.nestRegister.Keys)
            {
                this.Subscribe(item);
            }
        }

        private dynamic ProcessObject(NestObjectConfiguration config, string path, string value)
        {
            var m = config.Regex.Match(path);
            if (m.Success)
            {
                var id = m.Groups[1].Value;
                var configStore = this.nestRegister.GetObjectStore(config);
                if (!configStore.ContainsKey(id))
                {
                    configStore.Add(id, new ExpandoObject());
                }
                var objectContent = configStore[id] as IDictionary<string, Object>;
                var propertyName = m.Groups[2].Value;
                if (!objectContent.ContainsKey(propertyName))
                {
                    PackageHost.WriteInfo("[{2}] Added property of {3} :  {0} = {1}", propertyName, value, config.SystemId, id);
                    objectContent.Add(propertyName, value);
                }
                else
                {
                    PackageHost.WriteInfo("[{2}] Updated property of {3} : {0} = {1}", propertyName, value, config.SystemId, id);
                    objectContent[propertyName] = value;
                }
                return objectContent;
            }
            else
            {
                return null;
            }
        }
    }
}
