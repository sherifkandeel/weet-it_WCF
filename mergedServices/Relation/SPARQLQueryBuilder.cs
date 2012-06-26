/*Done by Sherif Kandeel, Hady Elsahar on february 2012
 * This class is intended to create the queries necessary to get the relation between two objects
 * The relation can be direct or indirect relationships throuhg another objects
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace mergedServices
{
    public class SPARQLQueryBuilder
    {

        #region variables

        public const String db = "http://dbpedia.org/resource/";
        public const String rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        public const String skos = "http://www.w3.org/2004/02/skos/core#";

        public enum connectionType { connectedDirectly = 0, connectedDirectlyInverted = 1, connectedViaMiddle = 2, connectedViaMiddleInverted = 3 } ; 

        //public const int connectedDirectly = 0;
        //public const int connectedDirectlyInverted = 1;
        //public const int connectedViaMiddle = 2;
        //public const int connectedViaMiddleInverted = 3;

        private Dictionary<String, String> prefixes = new Dictionary<String, String>();

        /// <summary>
        /// The main structure object that returns from this class
        /// </summary> 
        public struct InnerQuery
        {
            public string object1;
            public string object2;
            public connectionType connectState;
            public string queryText;
        };

        #endregion

        /// <summary>
        /// The constructor of the class, initiates the vars
        /// </summary>
        public SPARQLQueryBuilder()
        {
            prefixes["db"] = db;
            prefixes["rdf"] = rdf;
            prefixes["skos"] = skos;
        }
        /// <summary>
        /// Builds and returns a set of queries to find relations between two object1 and object2.
        /// </summary>
        /// <param name="object1">object1</param>
        /// <param name="object2">object2</param>
        /// <param name="maxDistance">MaxiumDistance between the two objects</param>
        /// <param name="limit">Limit of results</param>
        /// <param name="ignoredObjects">List of strings of names of objects be ignored in the Queries for example : http://dbpedia.org/resource/thing </param>
        /// <param name="ignoredProperties">List of strings of names of properties to be ignored in the Queries</param>
        /// <param name="avoidCycles">Integer value which indicates whether we want to suppress cycles , 0 = no cycle avoidance ,  1 = no intermediate object can be object1 or object2 ,   2 = like 1 + an object can not occur more than once in a connection </param>
        /// <returns>List of Queries to be queried</returns>
        public List<InnerQuery> buildQueries(string object1, string object2, int maxDistance, int limit, List<string> ignoredObjects = null, List<string> ignoredProperties = null, int avoidCycles = 0)
        {
            List<InnerQuery> Queries = new List<InnerQuery>();
            Dictionary<int, List<InnerQuery>> Queries2 = getQueries(object1, object2, maxDistance, limit, ignoredObjects, ignoredProperties, avoidCycles);


            // Queries2 object  Dictionary<int,list<innerQuery>>  , interation over all the Queries  
            foreach (int key in Queries2.Keys)
            {
                List<InnerQuery> arr = Queries2[key];

                //adding all Queries into a list of InnerQuery 
                if (arr.Count > 0)
                {
                    for (int i = 0; i < arr.Count; i++)
                    {
                        Queries.Add(arr[i]);
                    }
                }
            }
            return Queries;
        }

        /// <summary>
        /// Returns a query for getting a direct connection from $object1 to $object2. 
        /// </summary>
        /// <param name="object1">the first object</param>
        /// <param name="object2">the second object</param>
        /// <param name="distance">distance between obj1 and obj2</param>
        /// <param name="options">Options parameters object1,object2,contains,ignoredObjects,ignoredProperties,avoidCycles </param>
        /// <returns>String contains the Generated Queries</returns>
        private string direct(string object1, string object2, int distance, Dictionary<string, List<string>> options)
        {
            Dictionary<string, List<string>> vars = new Dictionary<string, List<string>>();
            vars["obj"] = new List<string>();
            vars["pred"] = new List<string>();


            // if the distance between the two objects is 1 , Query Generated will be in the form of  ....where{obj1 ?p1 obj2}
            if (distance == 1)
            {
                string retval = uri(object1) + "?pf1" + uri(object2);
                vars["pred"].Add("?pf1");
                return completeQuery(retval, options, vars);

            }
            // if the distance between the two objects is 1 , Query Generated will be in the form of  ....where{obj1 ?pf1 ?of1 ?pf2 ... obj2}
            // the where part of theQueries is then passed to thefunction CompleteQuery Method to Generate the complete Query 
            else
            {
                string query = uri(object1) + "?pf1 ?of1" + ".\n";
                vars["pred"].Add("?pf1");
                vars["obj"].Add("?of1");

                for (int i = 1; i < distance - 1; i++)
                {
                    query += "?of" + i + "?pf" + (i + 1) + "?of" + (i + 1) + ".\n";
                    vars["pred"].Add("?pf" + (i + 1));
                    vars["obj"].Add("?of" + (i + 1));
                }

                query += "?of" + (distance - 1) + "?pf" + distance + ' ' + uri(object2);
                vars["pred"].Add("?pf" + distance);
                return completeQuery(query, options, vars);
            }
        }


        /// <summary>
        /// is thefunction that takes the Sparql Query from Direct and ConnectViamiddleObject functions and complete it 
        /// it takes the part after where {....  , and completes it depending on the options 
        /// </summary>
        /// <param name="retval">is the string after where </param>
        /// <param name="options">Options parameters object1,object2,contains,ignoredObjects,ignoredProperties,avoidCycles</param>
        /// <param name="vars">Vars containing the all the variables in the predicates and the objects to be used in the filter function </param>
        /// <returns>returns the total complete Query with the Filter and Limit options </returns>
        private string completeQuery(string coreQuery, Dictionary<string, List<string>> options, Dictionary<string, List<string>> vars)
        {
            string completeQuery = "";
            //foreach (string key in prefixes.Keys)
            //{
            //    completeQuery += "PREFIX" + key + ": <" + prefixes[key] + ">\n"; 
            //}

            // TODO: we have to ask for an abstract, an imageURL and a link to wikipedia for each information too!

            completeQuery += "Select * WHERE {" + "\n";
            completeQuery += coreQuery + "\n";
            completeQuery += generateFilter(options, vars) + "\n";
            string limit = "";

            if (options.Keys.Contains("limit"))
            {
                limit = "LIMIT " + listToString(options["limit"]);
            }
            completeQuery += "}" + limit;
            completeQuery = completeQuery.Replace('\n', ' ');
            completeQuery = completeQuery.Replace("  ", " ");
            return completeQuery;
        }

        /// <summary>
        ///  Return a set of queries to find relations between two objects.
        /// </summary>
        /// <param name="object1">First object</param>
        /// <param name="object2">Second object</param>
        /// <param name="distance"> maxDistance The maximum distance up to which we want to search</param>
        /// <param name="limit">limit The maximum number of results per SPARQL query (=LIMIT).</param>
        /// <param name="ignoredObjects">Objects which should not be part of the returned connections between the first and second object</param>
        /// <param name="ignoredProperties">Properties which should not be part of the returned connections between the first and second object</param>
        /// <param name="avoidCycles">Integer value which indicates whether we want to suppress cycles , 0 = no cycle avoidance ,  1 = no intermediate object can be object1 or object2 ,   2 = like 1 + an object can not occur more than once in a connection </param>
        /// <returns>A two dimensional array of the form $array[$distance][$queries]</returns>
        private Dictionary<int, List<InnerQuery>> getQueries(string object1, string object2, int distance, int limit, List<string> ignoredObjects = null, List<string> ignoredProperties = null, int avoidCycles = 0)
        {
            Dictionary<int, List<InnerQuery>> queries = new Dictionary<int, List<InnerQuery>>();
            Dictionary<string, List<string>> options = new Dictionary<string, List<string>>();


            //May generate null 
            options["object1"] = stringToLoist(object1);
            options["object2"] = stringToLoist(object2);
            options["limit"] = stringToLoist(limit.ToString());
            options["ignoredObjects"] = ignoredObjects;
            options["ignoredProperties"] = ignoredProperties;
            options["avoidCycles"] = stringToLoist(avoidCycles.ToString());

            // get direct connection in both directions
            //queries[distance] = new ArrayCollection();
            //(queries[distance] as ArrayCollection).addItem(new Array(direct(object1, object2, distance, options), connectedDirectly));
            //(queries[distance] as ArrayCollection).addItem(new Array(direct(object2, object1, distance, options), connectedDirectlyInverted)); 
            //we substitiued ArrayCollection with a list and passed the list to the dictionary
            var tempList = new List<InnerQuery>();
            var tempInnerQuery = new InnerQuery();
            tempInnerQuery.queryText = direct(object1, object2, distance, options);
            tempInnerQuery.connectState = connectionType.connectedDirectly;
            tempInnerQuery.object1 = object1;
            tempInnerQuery.object2 = object2; 
            tempList.Add(tempInnerQuery);

            tempInnerQuery = new InnerQuery();
            tempInnerQuery.queryText = direct(object2, object1, distance, options);
            tempInnerQuery.connectState = connectionType.connectedDirectlyInverted;
            tempInnerQuery.object1 = object2;
            tempInnerQuery.object2 = object1; 
            tempList.Add(tempInnerQuery);

            //queries.Add(distance, tempList);
            //ended substitution

            for (int a = 1; a <= distance; a++)
            {
                for (int b = 1; b <= distance; b++)
                {
                    if ((a + b) == distance)
                    {

                        //(queries[distance] as ArrayCollection).addItem(new Array(connectedViaAMiddleObject(object1, object2, a, b, true,  options), connectedViaMiddle));
                        //tempList = new List<InnerQuery>();
                        tempInnerQuery = new InnerQuery();
                        tempInnerQuery.queryText = connectedViaAMiddleObject(object1, object2, a, b, true, options);
                        tempInnerQuery.connectState = connectionType.connectedViaMiddle;
                        tempInnerQuery.object1 = object1;
                        tempInnerQuery.object2 = object2; 
                        tempList.Add(tempInnerQuery);

                        tempInnerQuery = new InnerQuery();
                        tempInnerQuery.queryText = connectedViaAMiddleObject(object1, object2, a, b, false, options);
                        tempInnerQuery.connectState = connectionType.connectedViaMiddleInverted;
                        tempInnerQuery.object1 = object1;
                        tempInnerQuery.object2 = object2; 
                        tempList.Add(tempInnerQuery);


                        //(queries[distance] as ArrayCollection).addItem(new Array(connectedViaAMiddleObject(object1, object2, a, b, false, options), connectedViaMiddleInverted));
                    }
                }
            }
            queries.Add(distance, tempList);
            //substitution of the above code:



            return queries;
        }

        /// <summary>
        /// Return a set of queries to find relations between two objects
        /// which are connected via a middle objects $dist1 and $dist2 
        /// give the distance between the first and second object to 
        /// the middle they have ti be greater that 1
        /// </summary>
        /// <param name="first">1stobject</param>
        /// <param name="second">2ndobject</param>
        /// <param name="dist1">distance between 1obj and the middle object</param>
        /// <param name="dist2">distance between 2obj and the middle object</param>
        /// <param name="toObject">if $toObject is true then:
        ///          PATTERN                                         DIST1   DIST2
        ///          first-->?middle<--second                        1         1
        ///          first-->?of1-->?middle<--second                 2         1
        ///          first-->?middle<--?os1<--second                 1         2
        ///          first-->?of1-->middle<--?os1<--second           2         2
        ///          first-->?of1-->?of2-->middle<--second           3         1
        ///          
        ///          if $toObject is false then (reverse arrows)
        ///          first<--?middle-->second </param>
        /// <param name="options">options All options like ignoredProperties, etc. are passed via this array (needed for filters)</param>
        /// <returns>the SPARQL Query as a String</returns>
        private string connectedViaAMiddleObject(string first, string second, int dist1, int dist2, bool toObject, Dictionary<string, List<string>> options)
        {
            Dictionary<string, List<string>> vars = new Dictionary<string, List<string>>();
            vars["pred"] = new List<string>();
            vars["obj"] = new List<string>();
            vars["obj"].Add("?middle");

            string fs = "f";
            int tmpdist = dist1;
            int twice = 0;
            string coreQuery = "";
            string oobject = first;

            while (twice < 2)
            {

                if (tmpdist == 1)
                {
                    coreQuery += toPattern(uri(oobject), "?p" + fs + "1", "?middle", toObject);
                    vars["pred"].Add("?p" + fs + "1");
                }
                else
                {
                    coreQuery += toPattern(uri(oobject), "?p" + fs + "1", "?o" + fs + "1", toObject);
                    vars["pred"].Add("?p" + fs + "1");

                    for (int x = 1; x < tmpdist; x++)
                    {
                        string s = "?o" + fs + "" + x;
                        string p = "?p" + fs + "" + (x + 1);
                        vars["obj"].Add(s);
                        vars["pred"].Add(p);
                        if ((x + 1) == tmpdist)
                        {
                            coreQuery += toPattern(s, p, "?middle", toObject);
                        }
                        else
                        {
                            coreQuery += toPattern(s, p, "?o" + fs + "" + (x + 1), toObject);
                        }
                    }
                }
                twice++;
                fs = "s";
                tmpdist = dist2;
                oobject = second;

            }//end while

            return completeQuery(coreQuery, options, vars);
        }

        /// <summary>
        /// generates the necessary Filters
        /// </summary>
        /// <param name="options">The options Dictionary</param>
        /// <param name="vars">The vars Dictionary</param>
        /// <returns>returns the Filter part of the query</returns>
        private string generateFilter(Dictionary<string, List<string>> options, Dictionary<string, List<string>> vars)
        {
            //var filterterms:ArrayCollection = new ArrayCollection();
            var filterterms = new List<string>();
            foreach (string pred in vars["pred"])
            {
                // ignore properties

                if ((options != null) && (options.Keys.Contains("ignoredProperties")) && options["ignoredProperties"] != null && (options["ignoredProperties"] != null) && (options["ignoredProperties"].Count > 0))
                {
                    foreach (string ignored in (options["ignoredProperties"]))
                    {
                        filterterms.Add(pred + " != " + uri(ignored) + " ");
                    }
                }

            }
            foreach (string obj in (vars["obj"]))
            {
                // ignore literals
                filterterms.Add("!isLiteral(" + obj + ")");
                // ignore objects
                if ((options != null) && (options.Keys.Contains("ignoredObjects")) && options["ignoredObjects"] != null && (options["ignoredObjects"] != null) && (options["ignoredObjects"].Count > 0))
                {
                    foreach (string ignored2 in (options["ignoredObjects"]))
                    {
                        filterterms.Add(obj + " != " + uri(ignored2) + " ");
                    }
                }

                if ((options != null) && options.Keys.Contains("avoidCycles") && (options["avoidCycles"] != null))
                {
                    // object variables should not be the same as object1 or object2
                    if (options["avoidCycles"].Count > 0)
                    {
                        filterterms.Add(obj + " != " + uri(listToString(options["object1"])) + " ");
                        filterterms.Add(obj + " != " + uri(listToString(options["object2"])) + " ");
                    }
                    // object variables should not be the same as any other object variables
                    if (options["avoidCycles"].Count > 1)
                    {
                        foreach (string otherObj in (vars["obj"]))
                        {
                            if (obj != otherObj)
                            {
                                filterterms.Add(obj + " != " + otherObj + " ");
                            }
                        }
                    }
                }
            }

            if (filterterms.Count == 0)
            {
                return "";
            }

            return "FILTER " + expandTerms(filterterms, "&&") + ". ";
        }

        #region Helper_Functions

        /// <summary>
        /// takes a list and converts it to a string(list must be of count 1)
        /// </summary>
        /// <param name="inputList">takes the input list</param>
        /// <returns>first string of the list</returns>
        private string listToString(List<string> inputList)
        {
            return inputList[0];
        }

        /// <summary>
        /// takes a string and converts it to a list of strings of count1
        /// </summary>
        /// <param name="input">the intput string</param>
        /// <returns>list of count 1</returns>
        private List<string> stringToLoist(string input)
        {
            var list = new List<string>();
            list.Add(input);
            return list;
        }

        /// <summary>
        /// takes the object name and returns the URI link to easily use in Sparql Query 
        /// </summary>
        /// <param name="x">string the object name</param>
        /// <returns>string the URI</returns>
        private string uri(string x)
        {
            return "<" + x + ">";
        }

        /// <summary>
        /// this function puts the query terms subject, predicate, object
        /// </summary>
        /// <param name="s">the subject</param>
        /// <param name="p">the predicate</param>
        /// <param name="o">the object</param>
        /// <param name="toObject"></param>
        /// <returns></returns>
        private string toPattern(string s, string p, string o, bool toObject)
        {
            if (toObject)
            {
                return s + ' ' + p + ' ' + o + " . \n";
            }
            else
            {
                return o + ' ' + p + ' ' + s + " . \n";
            }

        }



        /// <summary>
        /// puts bracket around the (filterterms) and concatenates them with &&
        /// </summary>
        /// <param name="terms"> list of filterterms</param>
        /// <param name="ooperator"> separator operator default &&</param>
        /// <returns>string</returns>
        private string expandTerms(List<string> terms, string ooperator = "&&")
        {
            string result = "";

            for (int x = 0; x < terms.Count; x++)
            {
                result += "(" + terms[x] + ")";
                result += (x + 1 == terms.Count) ? "" : " " + ooperator + " ";
                result += "\n";
            }
            return "(" + result + ")";
        }

        #endregion

    }
}
