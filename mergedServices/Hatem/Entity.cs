using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    public class Entity
    {
        
        String uri, label, pic;
        
        [DataMember]
        public String URI
        {
            get { return uri; }
            set { uri = value; }
        }
        
        [DataMember]
        public String Label
        {
            get { return label; }
            set { label = value; }
        }
        
        [DataMember]
        public String Picture
        {
            get { return pic; }
            set { pic = value; }
        }
    }
}
