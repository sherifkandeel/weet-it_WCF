using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    public class relation
    {
        [DataMember]
        public entity source;
        [DataMember]
        public entity destination;
        [DataMember]
        public List<entity> entities;

        public relation()
        {
            this.source = new entity();
            this.destination = new entity();
            this.entities = new List<entity>();
        }

        public relation(List<KeyValuePair<string,string>> relationsWithLabels)
        {
            entities = new List<entity>();
            foreach (KeyValuePair<string,string> s in relationsWithLabels)
            {
                entities.Add(new entity( s.Key , s.Value, null));
            }
            for (int i = 0; i < relationsWithLabels.Count ; i++)
            {
                if (i < entities.Count - 1)
                    this.entities[i].next = this.entities[i + 1];
            }
            source = entities[0];
            destination = entities[entities.Count - 1];
        }

        public relation(List<string> relations)
        {
            entities = new List<entity>();

            foreach (string s in relations)
            {
                entities.Add(new entity(s,null,null));
            }
            for (int i = 0; i < relations.Count; i++)
            {
                if(i < entities.Count-1 )
                this.entities[i].next = this.entities[i+1];
            }
            source = entities[0];
            destination = entities[entities.Count - 1];
        }

    }


}
