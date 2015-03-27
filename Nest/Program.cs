using Constellation.Host;
using FirebaseSharp.Portable;
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

        private readonly Regex thermostatPathRegex = new Regex(@"\/devices\/thermostats\/(.*)\/(.*)", RegexOptions.Compiled);
        private readonly Regex structurePathRegex = new Regex(@"\/structures\/(.*)\/(.*)", RegexOptions.Compiled);

        private Dictionary<string, dynamic> structures = new Dictionary<string, dynamic>();
        private Dictionary<string, dynamic> thermostats = new Dictionary<string, dynamic>();

        static void Main(string[] args)
        {
            PackageHost.Start<Program>(args);
        }

        public override void OnStart()
        {
            PackageHost.WriteInfo("Connecting to NEST streaming API ...");

            // Subscribe to streaming
            this.Subscribe(thermostats, thermostatPathRegex, "devices", "Nest.Thermostat");
            this.Subscribe(structures, structurePathRegex, "structures", "Nest.Structure");

            // Push initial SO
            System.Threading.Thread.Sleep(2000);
            PackageHost.WriteInfo("Pushing inital state objects");
            foreach (var item in thermostats)
            {
                PackageHost.PushStateObject(item.Value.name, item.Value, "Nest.Thermostat");
            }
            foreach (var item in structures)
            {
                PackageHost.PushStateObject(item.Value.name, item.Value, "Nest.Structure");
            }         
   
            // Started !
            PackageHost.WriteInfo("Connected to Nest streaming API");
        }

        [MessageCallback]
        public void SetAwayMode(bool isAway)
        {
            this.SetProperty("structures/" + structures.First().Key, "away", isAway ? "away" : "home");
        }

        [MessageCallback]
        public void SetTargetTemperature(double temperature)
        {
            this.SetProperty("devices/thermostats/" + thermostats.First().Key, "target_temperature_c", temperature);        
        }

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

        private void Subscribe(Dictionary<string, dynamic> source, Regex pathRegex, string path, string soTypeName)
        {
            PackageHost.WriteInfo("Subscribing to {0} ({1})", path, soTypeName);
            Firebase fb = this.CreateFirebaseClient();
            var streaming = fb.GetStreaming(path,
                added: (s, e) =>
                {
                    this.ProcessObject(source, pathRegex, e.Path, e.Data);
                },
                changed: (s, e) =>
                {
                    if (e.OldData != e.Data)
                    {
                        var obj = this.ProcessObject(source, pathRegex, e.Path, e.Data);
                        PackageHost.PushStateObject(obj.name, obj, soTypeName);
                    }
                });
            streaming.Canceled += (s, e) =>
            {
                PackageHost.WriteWarn("Streaming of {0} ({1}) is cancelled", path, soTypeName);
                this.Subscribe(source, pathRegex, path, soTypeName);
            };
        }

        private dynamic ProcessObject(Dictionary<string, dynamic> source, Regex pathRegex, string path, string value)
        {
            var m = pathRegex.Match(path);
            if (m.Success)
            {
                var id = m.Groups[1].Value;
                if (!source.ContainsKey(id))
                {
                    source.Add(id, new ExpandoObject());
                }
                var objectContent = source[id] as IDictionary<string, Object>;
                var propertyName = m.Groups[2].Value;
                if (!objectContent.ContainsKey(propertyName))
                {
                    PackageHost.WriteInfo("[{2}] Added property {0} = {1}", propertyName, value, path.StartsWith("/structures") ? "Structure" : "Thermostat");
                    objectContent.Add(propertyName, value);
                }
                else
                {
                    PackageHost.WriteInfo("[{2}] Updated property {0} = {1}", propertyName, value, path.StartsWith("/structures") ? "Structure" : "Thermostat");
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
