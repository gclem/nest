using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Entities
{
    public class SmokeCoAlarm : NestObject
    {
        public const string REGEX = @"\/devices\/smoke_co_alarms\/(.*)\/(.*)";
        protected const string SYSTEM_ID = "Nest.SmokeCoAlarm";

        public SmokeCoAlarm()
            : base(NestObjectType.SMOKE_CO_ALARM, REGEX, SYSTEM_ID)
        {

        }
    }
}
