using Nest.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nest.Entities
{
    public class NestObjectCollection : List<NestObject>
    {
        public NestObjectCollection()
        {

        }

        public bool TypeExists(NestObjectType type) {
            return (this.Any(x => x.Type == type));
        }

        public NestObject Add(NestObjectType nestObjectType)
        {
            NestObject e = NestObjectFactory.Create(nestObjectType);

            //// Adding
            this.Add(e);

            //// Getting back
            return e;
        }

        public NestObject Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            return this.FirstOrDefault(x => x.NestId == id);
        }

        public List<NestObject> Get(NestObjectType nestObjectType)
        {
            return this.Where(x => x.Type == nestObjectType).ToList();
        }
    }
}
