using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nest.Entities
{
    /// <summary>
    /// Nest device object
    /// </summary>
    public abstract class NestObject
    {
        public NestObjectType Type { get; set; }
        public Regex Regex { get; set; }
        public string SystemId { get; set; }
        public NestChannel Channel { get; set; }

        public string NestId { get; set; }

        public dynamic Properties { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type"> Object type </param>
        /// <param name="regex"> Regex matching element indicator </param>
        /// <param name="channel"> Channel coming from</param>
        /// <param name="basepath"> path base indicator</param>
        /// <param name="systemId"> system id for message push title</param>
        public NestObject(NestObjectType type, string regex, string systemId, NestChannel channel = NestChannel.devices) 
        {
            if (string.IsNullOrEmpty(regex))
                throw new ArgumentNullException("regex");

            if (string.IsNullOrEmpty(systemId))
                throw new ArgumentNullException("systemId");

            this.Type = type;
            this.Regex = new Regex(regex, RegexOptions.Compiled);
            this.Channel = channel;
            this.SystemId = systemId;
            this.Properties = new Dictionary<string, Object>();
        }
    }
}
