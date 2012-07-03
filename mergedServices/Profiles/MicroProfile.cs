using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    [KnownType(typeof(MiniProfile))]
    public class MicroProfile :Profile
    {        
        Entity MyEntity;
        public MicroProfile()
        {
            MyEntity = new Entity();
        }

        [DataMember]
        public String URI
        {
            get { return MyEntity.URI; }
            set { MyEntity.URI = value; }
        }
        [DataMember]
        public String Label
        {
            get { return MyEntity.Label; }
            set { MyEntity.Label = value; }
        }
        [DataMember]
        public String Picture
        {
            get { return MyEntity.Picture; }
            set { MyEntity.Picture = value; }
        }
        
        String ABSTRACT;
        [DataMember]
        public String Abstract
        {
            get { return ABSTRACT; }
            set { ABSTRACT = value; }
        }

        bool isShortAbstract;
        [DataMember]
        public bool IsShortAbstract
        {
            get { return isShortAbstract; }
            set { isShortAbstract = value; }
        }
    }
}
