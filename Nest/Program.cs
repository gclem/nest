using Constellation.Host;
using FirebaseSharp.Portable;
using Nest.Entities;
using Nest.Factories;
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
            foreach (var item in nestRegister.OrderBy(x => x.Type))
            {
                PackageHost.PushStateObject(item.Properties["name"], item.Properties, item.SystemId);
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
            var structureStore = this.nestRegister.Get(NestObjectType.STRUCTURE);

            if(string.IsNullOrEmpty(id) && structureStore.Count == 0)
            {
                PackageHost.WriteWarn("No Structure available to set away.");
                return;
            }

            if(!structureStore.Any(x => x.NestId == id))
            {
                PackageHost.WriteWarn("No Structure with this id is available to set away.");
                return;
            }

            this.SetProperty("structures/" + id == "" ? structureStore.First().NestId : id, "away", isAway ? "away" : "home");
        }

        /// <summary>
        /// Sets the target temperature for the first thermostat.
        /// </summary>
        /// <param name="temperature">The temperature.</param>
        [MessageCallback]
        public void SetTargetTemperature(double temperature, string id = "")
        {
            var store = this.nestRegister.Get(NestObjectType.THERMOSTATS);

            if (string.IsNullOrEmpty(id) && store.Count == 0)
            {
                PackageHost.WriteWarn("No thermostat available to set temperature.");
                return;
            }

            if (!store.Any(x => x.NestId == id))
            {
                PackageHost.WriteWarn("No thermostat with this id is available to set temperature.");
                return;
            }

            this.SetProperty("devices/thermostats/" + store.First().NestId, "target_temperature_c", temperature);
        }

        /// <summary>
        ///  Init available data objects to subscribe
        /// </summary>
        private void InitDataObjects()
        {
            this.nestRegister = new NestObjectCollection();
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

        private void Subscribe(NestChannel channel)
        {
            PackageHost.WriteInfo("Subscribing to {0}", channel.ToString());
            Firebase fb = this.CreateFirebaseClient();
            NestObject nestObjectReturned = null;

            var streaming = fb.GetStreaming(channel.ToString(),
                added: (s, e) =>
                {
                    this.ProcessObject(channel, e.Path, e.Data, out nestObjectReturned);
                },
                changed: (s, e) =>
                {
                    if (e.OldData != e.Data)
                    {
                        var obj = this.ProcessObject(channel, e.Path, e.Data, out nestObjectReturned);

                        if(obj != null)
                            PackageHost.PushStateObject(obj.name, obj, nestObjectReturned.SystemId);
                    }
                });
            streaming.Canceled += (s, e) =>
            {
                PackageHost.WriteWarn("Streaming of {0} ({1}) is cancelled", null,null);
                this.Subscribe(channel);
            };
        }

        private void SubscribeAll()
        {
            this.Subscribe(NestChannel.structures);
            this.Subscribe(NestChannel.devices);
        }

        private dynamic Parse(NestChannel channel, string path)
        {
            dynamic result = new ExpandoObject();
            Match m = null;

            switch (channel)
            {
                case NestChannel.structures:
                    //// Matching structure regex
                    m = Regex.Match(path, Structure.REGEX, RegexOptions.Compiled);
                    result.ObjectType = NestObjectType.STRUCTURE;
                    break;
                case NestChannel.devices:
                    //// Matching thermostat regex
                    m = Regex.Match(path, Thermostat.REGEX, RegexOptions.Compiled);
                    result.ObjectType = NestObjectType.THERMOSTATS;

                    //// Matching smoke co alarm regex
                    if (!m.Success)
                    {
                        m = Regex.Match(path, SmokeCoAlarm.REGEX, RegexOptions.Compiled);
                        result.ObjectType = NestObjectType.SMOKE_CO_ALARM;
                    }
                    break;
                default:
                    break;
            }

            if (m.Success)
            {
                result.Id = m.Groups[1].Value;
                result.PropertyName = m.Groups[2].Value;

                return result;
            }

            return null;
        }

        private dynamic ProcessObject(NestChannel channel, string path, string value, out NestObject storeObject)
        {
            var m = this.Parse(channel, path);

            if (m != null)
            {
                storeObject = this.nestRegister.Get(m.Id);
                if (storeObject == null)
                {
                    storeObject = this.nestRegister.Add(m.ObjectType);
                    storeObject.NestId = m.Id;
                }

                var objectContent = storeObject.Properties as IDictionary<string, Object>;
                
                if (!objectContent.ContainsKey(m.PropertyName))
                {
                    PackageHost.WriteInfo("[{2}] Added property of {3} :  {0} = {1}", m.PropertyName, value, storeObject.SystemId, m.Id);
                    objectContent.Add(m.PropertyName, value);
                }
                else
                {
                    PackageHost.WriteInfo("[{2}] Updated property of {3} : {0} = {1}", m.PropertyName, value, storeObject.SystemId, m.Id);
                    objectContent[m.PropertyName] = value;
                }
                return objectContent;
            }
            else
            {
                storeObject = null;
                return null;
            }
        }
    }
}
