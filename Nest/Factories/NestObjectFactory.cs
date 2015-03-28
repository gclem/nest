using Nest.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Factories
{
    public static class NestObjectFactory
    {
        public static NestObject Create(NestObjectType type)
        {
            switch (type)
            {
                case NestObjectType.STRUCTURE:
                    return new Structure();
                case NestObjectType.THERMOSTATS:
                    return new Thermostat();
                case NestObjectType.SMOKE_CO_ALARM:
                    return new SmokeCoAlarm();
                default:
                    throw new InvalidOperationException("Type not handled. Please manage it on factory.");
            }
        }
    }
}
