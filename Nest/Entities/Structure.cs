using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Entities
{
    public class Structure : NestObject
    {
        public const string REGEX = @"\/structures\/(.*)\/(.*)";
        protected const string SYSTEM_ID = "Nest.Structure";

        public Structure() : base(NestObjectType.STRUCTURE, REGEX, SYSTEM_ID, NestChannel.structures)
        {

        }
    }
}
