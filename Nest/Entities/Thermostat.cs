using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Entities
{
    public class Thermostat : NestObject
    {
        public const string REGEX = @"\/devices\/thermostats\/(.*)\/(.*)";
        protected const string SYSTEM_ID = "Nest.Thermostat";

        public Thermostat()
            : base(NestObjectType.THERMOSTATS, REGEX, SYSTEM_ID)
        {

        }
    }
}
