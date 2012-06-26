using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    public class RelationEntity
    {
        [DataMember]
        public string uri;
        [DataMember]
        public string label;
        [DataMember]
        public RelationEntity next;

        public RelationEntity()
        {
            next = new RelationEntity();
        }
        public RelationEntity(string uri, string label, RelationEntity next)
        {
            this.uri = uri;
            this.label = label;
            this.next = next;
        }


    }
}

