using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    public class entity
    {
        [DataMember]
        public string uri;
        [DataMember]
        public string label;
        [DataMember]
        public entity next;

        public entity()
        {
            next = new entity();
        }
        public entity(string uri, string label, entity next)
        {
            this.uri = uri;
            this.label = label;
            this.next = next;
        }


    }
}

