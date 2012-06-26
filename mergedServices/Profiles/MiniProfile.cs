using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    [KnownType(typeof(FullProfile))]
    public class MiniProfile: MicroProfile
    {
        List<KeyValuePair<String, List<Entity>>> details;        
        public MiniProfile()
        {
           
            details = new List<KeyValuePair<String, List<Entity>>>();
        }    
        [DataMember]
        public List<KeyValuePair<String, List<Entity>>> Details
        {
            get { return details; }
            set { details = value; }
        }
    }
}