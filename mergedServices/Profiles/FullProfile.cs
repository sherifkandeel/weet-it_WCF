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
        Location location;
        
        [DataMember]
        public List<Entity> Related
        {
            get { return related; }
            set { related = value; }
        }

        [DataMember]
        public Location Location
        {
            get { return location; }
            set { location = value; }
        }
    }

    [DataContract]
    public class Location
    {
        String latitude, longitude;

        [DataMember]
        public String Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        [DataMember]
        public String Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }
    }
}
