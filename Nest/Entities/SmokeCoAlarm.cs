using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Entities
{
    public class SmokeCoAlarm : NestObject
    {
        protected const string REGEX = @"\/devices\/smoke_co_alarms\/(.*)\/(.*)";
        protected const string SYSTEM_ID = "Nest.SmokeCoAlarm";

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

        public SmokeCoAlarm()
            : base(NestObjectType.SMOKE_CO_ALARM, SYSTEM_ID, REGEX)
        {

        }
    }
}
