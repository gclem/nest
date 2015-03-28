using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Entities
{
    /// <summary>
    /// Nest API object types
    /// </summary>
    public enum NestObjectType
    {
        [Description("structures")]
        STRUCTURE,
        [Description("thermostats")]
        THERMOSTATS,
        //[Description("$company")]
        //COMPANY,
        [Description("smoke_co_alarms")]
        SMOKE_CO_ALARM
    }
}
