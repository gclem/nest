using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Entities
{
    public class Thermostat : NestObject
    {
        protected const string REGEX = @"\/devices\/thermostats\/(.*)\/(.*)";
        protected const string SYSTEM_ID = "Nest.Thermostat";

        public override string NestId
        {
            get
            {
                return this.Properties["device_id"];
            }
            set
            {
                this.Properties["device_id"] = value;
            }
        }

        public Thermostat() : base(NestObjectType.THERMOSTATS, SYSTEM_ID, REGEX)
        {

        }
    }
}
