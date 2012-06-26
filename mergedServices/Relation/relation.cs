using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    public class Relation
    {
        [DataMember]
        public RelationEntity source;
        [DataMember]
        public RelationEntity destination;
        [DataMember]
        public List<RelationEntity> entities;

        public Relation()
        {
            this.source = new RelationEntity();
            this.destination = new RelationEntity();
            this.entities = new List<RelationEntity>();
        }

        public Relation(List<KeyValuePair<string,string>> relationsWithLabels)
        {
            entities = new List<RelationEntity>();
            foreach (KeyValuePair<string,string> s in relationsWithLabels)
            {
                entities.Add(new RelationEntity( s.Key , s.Value, null));
            }
            for (int i = 0; i < relationsWithLabels.Count ; i++)
            {
                if (i < entities.Count - 1)
                    this.entities[i].next = this.entities[i + 1];
            }
            source = entities[0];
            destination = entities[entities.Count - 1];
        }

        public Relation(List<string> relations)
        {
            entities = new List<RelationEntity>();

            foreach (string s in relations)
            {
                entities.Add(new RelationEntity(s,null,null));
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
