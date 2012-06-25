using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mergedServices
{
   public class ObjectsRelationManager
    {
        //is it the end of the results yet?
        private bool isEndOfResults=false;
        public bool IsEndOfResults
        {
            get {return isEndOfResults;}      
                    
        }

        //private variables
        private List<SPARQLQueryBuilder.InnerQuery> generatedQueriesList;


        //The two objects to compare between
        private string obj1;
        private string obj2;


        private static bool isConnectionStarted = false;

        /// <summary>
        /// starts the connection to the server
        /// </summary>
        public  void startConnection()
        {
            //Initiating the manager(to be added to constructor?)
            if (!isConnectionStarted)
            {
                QueryProcessor.startConnection();
                isConnectionStarted = true;
            }

        }

        /// <summary>
        /// closes the connection of the server
        /// </summary>
        public  void closeConnection()
        {
            if (isConnectionStarted)
            {
                QueryProcessor.closeConnection();
                isConnectionStarted = false;
            }

        }


        /// <summary>
        /// Builds and returns a set of queries to find relations between two object1 and object2.
        /// </summary>
        /// <param name="object1">object1</param>
        /// <param name="object2">object2</param>
        /// <param name="maxDistance">MaxiumDistance between the two objects</param>
        /// <param name="limit">Limit of results</param>
        /// <param name="ignoredObjects">List of strings of names of objects be ignored in the Queries</param>
        /// <param name="ignoredProperties">List of strings of names of properties to be ignored in the Queries</param>
        /// <param name="avoidCycles">Integer value which indicates whether we want to suppress cycles , 0 = no cycle avoidance ,  1 = no intermediate object can be object1 or object2 ,   2 = like 1 + an object can not occur more than once in a connection</param>
        /// <returns>false means an error happened, true means it's ok</returns>
        public bool generateQueries(string object1, string object2, int maxDistance=3, int limit=50, List<string> ignoredObjects = null, List<string> ignoredProperties = null, int avoidCycles = 1)
        {
            //here we reset everything because this method is only used upon new comparison
            //resetting the bool
            isEndOfResults = false;
            //resetting the list of queries
            generatedQueriesList = new List<SPARQLQueryBuilder.InnerQuery>();
            //resetting the resultlist
            curResultSet2 = new List<string>();
            //resetting the uniqueList of JsonObjects
            uniqueJsonObjects = new List<string>();

            //to make other methods see the two objects
            obj1 = object1;
            obj2 = object2;

                       
            SPARQLQueryBuilder builder = new SPARQLQueryBuilder();
            
            //hardcoded objects to ignore (For testing purposes)
            ignoredObjects = new List<string>();
            ignoredObjects.Add("http://dbpedia.org/ontology/wikiPageWikiLink");
            ignoredObjects.Add("http://dbpedia.org/ontology/wikiPageRedirects");
            ignoredObjects.Add("http://www.w3.org/2002/07/owl#Thing");
            ignoredObjects.Add("http://www.opengis.net/gml/_Feature");


            generatedQueriesList = builder.buildQueries(object1, object2, maxDistance, limit, ignoredObjects, ignoredObjects, avoidCycles);
            //generatedQueriesList=builder.buildQueries(object1, object2, maxDistance, limit, ignoredObjects, ignoredProperties, avoidCycles);
            
            //if an error happened
            if (generatedQueriesList.Count < 1)
                return false;
            
            return true;
        }
        private string temp = "";
        private  List<string> curResultSet2 = new List<string>();

                
        //list of unique json objects
        private static List<string> uniqueJsonObjects = new List<string>();
        
        /// <summary>
        /// Query the next result 
        /// </summary>
        /// <returns>the JsonObject of the next result</returns>
        public string getNextResult()
        {
            //this one is tricky, it gets the next jsob object to draw, 
            //if we're querying with a result set 
            //string temp="";
            if (curResultSet2.Count > 0)
            {
                //temp = curResultSet2[0];
                //curResultSet2.RemoveAt(0);
                //return temp;


                temp = curResultSet2[0];
                curResultSet2.RemoveAt(0);

                //making sure it's unique
                foreach (string t in uniqueJsonObjects)
                {
                    if (temp.Equals(t))
                        return "";
                }
                //adding it to the list if it's unique
                uniqueJsonObjects.Add(temp);
                //returning it
                return temp;
            }
                //else we need to get a new resultset
            else
            {
                //inner results to execute queries to
                List<ResSetToJSON.innerResult> results = new List<ResSetToJSON.innerResult>();

                //the query gets processed and the query is removed from the query list till it's empty
                if (generatedQueriesList != null && generatedQueriesList.Count > 0)
                {
                    //making the nextQuery
                    startConnection();
                    results = QueryProcessor.ExecuteQueryWithInnerQuery(generatedQueriesList[0], obj1, obj2);
                    closeConnection();
                    
                    //removing the query done from the list
                    generatedQueriesList.RemoveAt(0);

                    //generating the JsonObj
                    curResultSet2 = ResSetToJSON.ToListOfJsonObj(results);


                    getNextResult();
                    
                }

                else
                {
                    //termenating
                    isEndOfResults = true;
                    return "";
                }
 
            }
            return temp;
        }
        


        /// <summary>
        /// Query the next result 
        /// </summary>
        /// <returns>the JsonObject of the next result</returns>
        //public string getNextResult()
        //{
        //    //inner results to execute queries to
        //    List<ResSetToJSON.innerResult> results = new List<ResSetToJSON.innerResult>();

        //    //the query gets processed and the query is removed from the query list till it's empty
        //    if (generatedQueriesList != null && generatedQueriesList.Count > 0)
        //    {
        //        //making the nextQuery
        //        results = QueryProcessor.ExecuteQueryWithInnerQuery(generatedQueriesList[0], obj1, obj2);

        //        //removing the query done from the list
        //        generatedQueriesList.RemoveAt(0);

        //        //generating the JsonObj
        //        string res=ResSetToJSON.ToJsonObj(results);
               
        //        return res;
        //    }

        //    else
        //    {
        //        //termenating
        //        isEndOfResults = true;
        //        return "";
        //    }

        //}


    }
}
