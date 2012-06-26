using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    public class FullProfile : MiniProfile
    {
        
        List<Entity> related = new List<Entity>();
        
        [DataMember]
        public List<Entity> Related
        {
            get { return related; }
            set { related = value; }
        }
    }
}
