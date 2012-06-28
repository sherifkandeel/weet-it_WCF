using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{

    [DataContract]
    [KnownType(typeof(MicroProfile))]
    [KnownType(typeof(LiteralProfile))]
    public class Profile
    {        

    }
}
