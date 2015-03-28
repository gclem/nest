using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Entities
{
    public class Structure : NestObject
    {
        protected const string REGEX = @"\/structures\/(.*)\/(.*)";
        protected const string SYSTEM_ID = "Nest.Structure";

        public override string NestId
        {
            get
            {
                return this.Properties["structure_id"];
            }
            set
            {
                this.Properties["structure_id"] = value;
            }
        }

        public Structure() : base(NestObjectType.STRUCTURE, SYSTEM_ID, REGEX, "structures")
        {

        }
    }
}
