using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace mergedServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    partial class MergedService : ComparisonServiceInterface
    {

        /// <summary>
        /// This should return the list of resource information objects returning as the comparison result
        /// </summary>
        /// <param name="URIs">the uris to compare between </param>
        /// <returns>the resource information objects resulting form the comparison</returns>
        public List<ResourceInformation> Compare(List<string> URIs)
        {
            Comparison c;
            c = new Comparison(URIs);
            List<ResourceInformation> ri = c.getComparisonOutput();
            return ri;

            //ResourceInformation r = new ResourceInformation();
            //r.ID = new KeyValuePair<string, string>("ali", "alis");
            //List<ResourceInformation> rs = new List<ResourceInformation>();
            //rs.Add(r);
            //return rs;

        }

       

        /// <summary>
        /// gets the id of the resource
        /// </summary>
        /// <returns>uri, label of the id</returns>
        public KeyValuePair<string, string> getID(ResourceInformation ri)
        {
            return ri.ID;
        }

        /// <summary>
        /// gets all predicates responds to the query < resource> ?pred ?obj
        /// </summary>
        /// <returns>list<predURI,predLabel> </returns>
        public List<KeyValuePair<string, string>> getPredicates_ResourceIsSubj(ResourceInformation ri)
        {
            return ri.predicates_resourceIsSubj.Distinct().ToList();
        }

        /// <summary>
        /// gets all predicates responds to the query ?subj ?pred < resource>
        /// </summary>
        /// <returns>list<predURI,predLabel> </returns>           
        public List<KeyValuePair<string, string>> getPredicates_ResourceIsObj(ResourceInformation ri)
        {
            return ri.predicates_resourceIsObj.Distinct().ToList();
        }

        /// <summary>
        /// Common Predicates between all the resources we're comparing
        /// </summary>
        /// <returns>list<predURI,predLabel></returns>
        public List<KeyValuePair<string, string>> getCommonPredicates(ResourceInformation ri)
        {
            List<KeyValuePair<string, string>> toReturn = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>> item in ri.FinalComparisonObject)
            {
                toReturn.Add(item.Key);
            }
            return toReturn;
        }

        /// <summary>
        /// gets List of resources of a certain predicate
        /// </summary>
        /// <param name="pred">the predicate as a keyValuePair</param>
        /// <returns>the resources attached as a list of keyValuePairs</returns>
        public List<KeyValuePair<string, string>> getResourcesOfPredicate(KeyValuePair<string, string> pred, ResourceInformation ri)
        {
            foreach (KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>> item in ri.rawComparisonObject)
            {
                if (item.Key.Equals(pred))
                    return item.Value;
            }
            //if not foud
            return null;
        }

        
    }
}
