using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mergedServices55
{
    
    public class ResourceInformations
    {

        //it's own id
        public KeyValuePair<string, string> ID;
        //predicates information
        //URI, Label
        public List<KeyValuePair<string, string>> predicates_resourceIsSubj;//= new List<KeyValuePair<string, string>>(); 

        //resources information
        //URI,label
        public List<KeyValuePair<string, string>> resources_resourceIsSubj;//= new  List<KeyValuePair<string, string>>();

        //predicates information
        //URI, Label
        public List<KeyValuePair<string, string>> predicates_resourceIsObj;//=new List<KeyValuePair<string, string>>();

        //resources information
        //URI,label
        public List<KeyValuePair<string, string>> resources_resourceIsObj;//=new  List<KeyValuePair<string, string>>();

        //resources put in a form of Pred -> List<it's resources>
        public List<KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>> rawComparisonObject;

        //common resources between this resource and others in the same component
        public List<KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>> FinalComparisonObject;

        /// <summary>
        /// gets the id of the resource
        /// </summary>
        /// <returns>uri, label of the id</returns>
        public KeyValuePair<string, string> getID()
        {
            return this.ID;
        }

        /// <summary>
        /// gets all predicates responds to the query < resource> ?pred ?obj
        /// </summary>
        /// <returns>list<predURI,predLabel> </returns>
        public List<KeyValuePair<string, string>> getPredicates_ResourceIsSubj()
        {
            return this.predicates_resourceIsSubj.Distinct().ToList();
        }


        /// <summary>
        /// gets all predicates responds to the query ?subj ?pred < resource>
        /// </summary>
        /// <returns>list<predURI,predLabel> </returns>           
        public List<KeyValuePair<string, string>> getPredicates_ResourceIsObj()
        {
            return this.predicates_resourceIsObj.Distinct().ToList();
        }

        /// <summary>
        /// Common Predicates between all the resources we're comparing
        /// </summary>
        /// <returns>list<predURI,predLabel></returns>
        public List<KeyValuePair<string, string>> getCommonPredicates()
        {
            List<KeyValuePair<string, string>> toReturn = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>> item in this.FinalComparisonObject)
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
        public List<KeyValuePair<string, string>> getResourcesOfPredicate(KeyValuePair<string, string> pred)
        {
            foreach (KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>> item in rawComparisonObject)
            {
                if (item.Key.Equals(pred))
                    return item.Value;
            }
            //if not foud
            return null;
        }



    }
}
