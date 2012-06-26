using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace mergedServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    partial class MergedService : RelationGeneratorServiceInterface
    {
        public List<List<string>> simpleGetRelations(List<string> uri, int Distance, int Limit = 50)
        {
            return RelFinder.getRelations(uri, Distance, Limit);
        }

        public List<List<KeyValuePair<string, string>>> simpleGetRelationWithLabels(List<string> uri, int Distance, int Limit = 50)
        {
            return RelFinder.getRelationWithLabels(uri, Distance, Limit);
        }

        public List<Relation> getRelations(List<string> uri, int Distance, int Limit = 50)
        {
            List<List<string>> results = RelFinder.getRelations(uri, Distance, Limit);
            List<Relation> relations = new List<Relation>();
            foreach (List<string> s in  results)
            {
                relations.Add(new Relation(s));
            }
            return relations;
        }

        public List<Relation> getRelationWithLabels(List<string> uri, int Distance, int Limit = 50)
        {
            List<List<KeyValuePair<string,string>>> results =  RelFinder.getRelationWithLabels(uri, Distance, Limit);
            List<Relation> relations = new List<Relation>();
            foreach (List<KeyValuePair<string,string>> s in results)
            {
                relations.Add(new Relation(s));
            }

            return relations;
        }


    }
}
