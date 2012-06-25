using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using VDS.RDF;
using VDS.RDF.Query;

namespace mergedServices
{
    public static class ResSetToJSON
    {
        #region variables
        //private static string s;
        static List<List<string>> z = new List<List<string>>();
        
        public struct innerResult
        {
            public SparqlResultSet resultSets;
            public int connectState;
            public string firstObj, lastObj;
        }
        #endregion


        /// <summary>
        /// gets the list of the json objects to be drawn for each query
        /// </summary>
        /// <param name="b">the result list of each query</param>
        /// <returns>the output list of json objects</returns>
        public static List<string> ToListOfJsonObj(List<innerResult> b)
        {
            List<string> toReturn = new List<string>();

            //if the results are empty
            if (b.Count == 0)
                return toReturn;

            z = innerResultToURIs(b);

            string s = "";

            //this will set the always changing first and last objects            
            foreach (List<string> m in z)
            {
                m.Insert(0, b[0].firstObj);
                m.Insert(m.Count, b[0].lastObj);

            }
            for (int x = 0; x < z.Count; x++)
            {
                //this will set the results to the structure
                z[x] = reArrangeRelationVariables(z[x]);
                s = "{" + toJSONNode(z[x]) + "," + toJSONEdge(z[x]) + "}";
                toReturn.Add(s);
            }

            //foreach (List<string> x in z)
            //{
            //    s = "{" + toJSONNode(x) + "," + toJSONEdge(x) + "}";
            //    toReturn.Add(s);
            //    //textBox3.Text += toJSONEdge(x) + "\n";
            //}

            return toReturn;
        }


        

        //this one is not used, we're keeping it as private just in case
        private static string ToJsonObj(List<innerResult> b)
        {
            if (b.Count == 0)
                return "";
            z = innerResultToURIs(b);
            string s = "";

            //this is last object
            foreach (List<string> m in z)
            {
                m.Insert(0, b[0].firstObj);
                m.Insert(m.Count, b[0].lastObj);
            }

            for (int x = 0; x < z.Count; x++)
            {
                z[x] = reArrangeRelationVariables(z[x]);
                s += "{" + toJSONNode(z[x]) + "," + toJSONEdge(z[x]) + "},";
            }

            //foreach (List<string> x in z)
            //{
                
            //    //textBox3.Text += toJSONEdge(x) + "\n";
            //}
            s = s.Remove((s.Length) - 1, 1);
            s += "";
            return s;


        }


        /// <summary>
        /// renames the string;
        /// </summary>
        /// <param name="URI"></param>
        /// <returns></returns>
        public static string namer(string URI)
        {
            //string inp = "select ?x where{<" + URI + "> <http://www.w3.org/2000/01/rdf-schema#label> ?x}";
            //QueryProcessor.startConnection();
            //SparqlResultSet result= QueryProcessor.ExecuteQueryWithString(inp);
            //QueryProcessor.closeConnection();
            //string resString = "";

            ////if the query got a result
            //if (result.Count > 0)
            //{
            //    SparqlResult res = result[0];
            //    resString = (res.Value("x")).ToString();
            //    //resString = res.ToString();
            //}

            ////if something bad happened
            //else
            //{
            //    int x = URI.LastIndexOf('/');
            //    resString= (URI.Substring(x + 1));

            //}
            //int index = resString.IndexOf("@");
            //if (index != -1)
            //    resString = resString.Remove(resString.IndexOf("@"));

            //resString = resString.Replace("_", " ");
            //return resString;


            return util.getLabel(URI);
            
        }    // get the labels of URIs

        //this variable holds the results structure of each query
        private static List<string> variableName;

        /// <summary>
        /// we get the values of each result from here
        /// </summary>
        /// <param name="sq">input sparqlResult</param>
        /// <returns>list of string of all the urls according to the query structure ex:?pf ?middle ?os ?ps </returns>
        private static List<string> valuesOfResults(SparqlResult sq)
        {
            //I need this one to know the variables names
            variableName = new List<string>();

            //List<string> variableName = new List<string>();
            variableName = sq.Variables.ToList();
            List<string> output = new List<string>();
            foreach (string var in variableName)
            {
                output.Add(sq.Value(var).ToString());
            }
            return output;
        }
        
        
        /// <summary>
        /// get the value of each variable
        /// </summary>
        /// <param name="inner">input innerResult list</param>
        /// <returns>list of list of urls for each result</returns>
        private static List<List<string>> innerResultToURIs(List<innerResult> inner)
        {
            List<List<string>> a = new List<List<string>>();
            foreach (innerResult q in inner)
            {
                foreach (SparqlResult w in q.resultSets)
                {
                    a.Add(valuesOfResults(w));
                }
            }
            return a;
        }

        //this is the array used to order the variable names
        private static string[] sortedVars;

        /// <summary>
        /// This function sorts the results and the variables names to be different from each side
        /// Egypt->Type->Country<-Type<-Syria
        /// The first Type should be drawn separately from the second one
        /// </summary>
        /// <param name="query">the query resutls list</param>
        /// <returns>The sorted query resutls list</returns>
        private static List<string> reArrangeRelationVariables(List<string> query)
        {
            List<int> pf = new List<int>();
            List<int> of = new List<int>();
            int middle=-1;
            List<int> os = new List<int>();
            List<int> ps = new List<int>();

            sortedVars = new string[variableName.Count];

            //this for loop stores the indicies of the categorized vars
            for (int i = 0; i < variableName.Count; i++)
            {
                if(variableName[i].Contains("pf"))
                    pf.Add(i);
                
                if(variableName[i].Contains("of"))
                    of.Add(i);
                
                if (variableName[i].Contains("middle"))
                    middle = i;

                if(variableName[i].Contains("os"))
                    os.Add(i);
                
                if(variableName[i].Contains("ps"))
                    ps.Add(i);
            }

            //this string array holds the right order of the variables
            string[] orderedVars = new string[query.Count];
            
            //initiating the two original objects
            orderedVars[0] = query[0];
            orderedVars[query.Count - 1] = query[query.Count - 1];
            
            //The following variables is used to correctly reference each url to it's place
            //Also references each variable name in it's right place to be able to draw the json edge
            int oddAdder = 1;
            int evenAdder = 2;
            for (int i = 0; i < pf.Count; i++)
            {
                orderedVars[oddAdder] = query[pf[i]+1];
                
                //sorting the variableName array
                sortedVars[oddAdder - 1] = variableName[pf[i]];

                oddAdder += 2;
            }            
            
            for (int i = 0; i < of.Count; i++)
            {
                orderedVars[evenAdder] = query[of[i]+1];     


                //sorting the variableName array
                sortedVars[evenAdder - 1] = variableName[of[i]];

                evenAdder += 2;
            }
                       


            int reverseOddAdder = query.Count - 2;
            int reverseEvenAdder = query.Count - 3;

            for (int i = 0; i < ps.Count; i++)
            {               
            
                orderedVars[reverseOddAdder] = query[ps[i]+1];

                //sorting the variableName array
                sortedVars[reverseOddAdder - 1] = variableName[ps[i]];

                reverseOddAdder -= 2;
            }
            for (int i = 0; i < os.Count; i++)
            {
                orderedVars[reverseEvenAdder] = query[os[i]+1];

                //sorting the variableName array
                sortedVars[reverseEvenAdder - 1] = variableName[os[i]];

                reverseEvenAdder -= 2;
            }


            if (middle != -1)
            {
                for (int i = 0; i < orderedVars.Length; i++)
                {
                    if (orderedVars[i] == null)
                        orderedVars[i] = query[middle + 1];
                }
                for (int i = 0; i < variableName.Count; i++)
                {
                    if (sortedVars[i] == null)
                        sortedVars[i] = variableName[middle];
                }
            }

            return orderedVars.ToList();

        }

        /// <summary>
        /// get all the results of each variable
        /// </summary>
        /// <param name="query">result list of the query</param>
        /// <returns>JsonNode of the result</returns>
        private static string toJSONNode(List<string> query)
        {
            //query = reArrangeRelationVariables(query);

            string a = "nodes:{";
           
            //a += "\"" + namer(query[0])  + "\":{\"uri\":\"" + query[0] + "\",\"shape\":\"rectangle\",\"color\":\"#956BC9\",\"label\":\"" + namer(query[0]) + "\"},";
            a += "\"" + namer(query[0]) + "\":{\"uri\":\"" + query[0] + "\",\"shape\":\"rectangle\",\"color\":\"#956BC9\",\"label\":\"" + namer(query[0]) + "\"},";
            for (int i = 1; i < query.Count-1; i++)
            {
                //we added different naming so that they don't merge
                if (sortedVars[i - 1].Contains("p"))
                    a += "\"" + namer(query[i]) + sortedVars[i - 1] + "\":{\"uri\":\"" + query[i] + "\",\"shape\":\"rectangle\",\"color\":\"#9E9E9E\",\"label\":\"" + namer(query[i]) + "\"},";
                    
                else
                    a += "\"" + namer(query[i]) + sortedVars[i - 1] + "\":{\"uri\":\"" + query[i] + "\",\"shape\":\"rectangle\",\"color\":\"#FF8800\",\"label\":\"" + namer(query[i]) + "\"},";
                    
            }
            a += "\"" + namer(query[query.Count-1]) + "\":{\"uri\":\"" + query[query.Count-1] + "\",\"shape\":\"rectangle\",\"color\":\"#956BC9\",\"label\":\"" + namer(query[query.Count-1]) + "\"},";
            
            a = a.Remove((a.Length) - 1, 1);
            a += "}";
            return a;
        } 
        
        /// <summary>
        /// get the nodes of each query
        /// </summary>
        /// <param name="query">Result List of the query</param>
        /// <returns>JSonEdge object</returns> 
        private static string toJSONEdge(List<string> query)
        {
            //query = reArrangeRelationVariables(query);
            string a = "edges:{";           
            for (int i = 0; i < query.Count - 1; i++)
            {
                
                if (i == 0)
                    a += "\"" + namer(query[0]) + "\":{\"" + namer(query[1]) + sortedVars[0] + "\":{}},";

                else if (i == query.Count - 2)
                    a += "\"" + namer(query[query.Count - 2]) + sortedVars[query.Count - 3] + "\":{\"" + namer(query[query.Count - 1]) + "\":{}},";

                else
                    a += "\"" + namer(query[i]) + sortedVars[i - 1] + "\":{\"" + namer(query[i + 1]) + sortedVars[i] + "\":{}},";
            }
            a = a.Remove((a.Length) - 1, 1);
            a += "}";
            return a;
        }


    }
}