using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using System.IO;
using VDS.RDF.Storage;
using VDS.RDF.Parsing;
using System.Text.RegularExpressions;

/*The communication happens between the methods inside this class thorugh the properties of the class itself.
 * the public lists
 */
namespace mergedServices
{
    public static class QueryHandler
    {
        public static VirtuosoManager manager;
        private static bool isConnectionStarted = false;
        
        /// <summary>
        /// starts connection with the server
        /// </summary>
        public static void startConnection()
        {
            //Initiating the manager(to be added to constructor?)
            if (!isConnectionStarted)
            {
                //manager = new VirtuosoManager("Server=localhost;Uid=dba;pwd=dba;Connection Timeout=500");
                
                manager = new VirtuosoManager("localhost", 1111, "DB", "dba", "dba");
                isConnectionStarted = true;
            }

        }

        /// <summary>
        /// closes connection to the server
        /// </summary>
        public static void closeConnection()
        {
            if (isConnectionStarted)
            {
                //manager.Close(false);
                //isConnectionStarted = false;
            }

        }


        /// <summary>
        /// converts the localhost to dbpedia, not recommended
        /// </summary>
        /// <param name="input">the localhost input</param>
        /// <returns>the dbpeida link</returns>
        public static string convertToDbpedia(string input)
        {
            //testing purposes only
            if(input.Contains("localhost"))
            {
            int count = 0;
            string toreplace = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (count == 2)
                    toreplace += input[i];
                if (input[i] == '/')
                    count++;

            }
            input = input.Replace(toreplace, "dbpedia.org/");

            }
            return input;

        }


        /// <summary>
        /// converts any domain name to the localhost:8890
        /// </summary>
        /// <param name="input">url with any domain name</param>
        /// <returns>converted url</returns>
        public static string convertToLocalhost(string input)
        {
            int count = 0;
            string toreplace = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (count == 2)
                    toreplace += input[i];
                if (input[i] == '/')
                    count++;

            }
            input = input.Replace(toreplace, "localhost:8890/");
            return input;

        }


        /// <summary>
        /// Gets label of given node.
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>String of the node label</returns>
        public static string getLabelIfAny(string node)
        {
            if (node.Contains("http://"))
            {
                //QueryHandler.startConnection();
                //SparqlResultSet results = QueryHandler.ExecuteQueryWithString("select ?x where {<" + node + "> <http://www.w3.org/2000/01/rdf-schema#label> ?x}");// FILTER (langMatches(lang(?x), \"EN\"))}");
                //QueryHandler.closeConnection();


                SparqlResultSet results = Request.RequestWithHTTP("select ?x where {<" + node + "> <http://www.w3.org/2000/01/rdf-schema#label> ?x}");

                if (results.Count != 0)
                    return results[0].Value("x").ToString();
                else
                    return node.ToString();
            }
            else
                return node.ToString();
        }

        /// <summary>
        /// gets the graph from the input Uri(changes the uri to localhost)
        /// </summary>
        /// <param name="input">the input string uri</param>
        /// <returns>the output graph</returns>
        public static Graph getGraphFromURIWithlocalHost(string input)
        {            
            Graph g = new Graph(); 
           
            //replacing the domainname with localhost
            int count = 0;
            string toreplace = "";
            for (int i=0; i < input.Length;i++ )
            {
                if (count == 2)
                    toreplace += input[i];
                if (input[i] == '/')
                    count++;               

            }
            input = input.Replace(toreplace, "localhost:8890/");

            //getting the rdf
            try
            {
                UriLoader.Load(g, new Uri(input));
            }
            catch{}
            return g;
            
        }


        /// <summary>
        /// overload of Execute query
        /// </summary>
        /// <param name="input">the query text as string</param>
        /// <returns></returns>
        public static SparqlResultSet ExecuteQueryWithString(string input)
        {
            //list to hold the results
            SparqlResultSet resultSet = new SparqlResultSet();
            try
            {
                //just in cas someone didn't open the connection
                if (!isConnectionStarted)
                    startConnection();
                
                //making the query
                Object result = manager.Query(input);
                //Object result = manager.ExecuteQuery(input);
                resultSet = (SparqlResultSet)result;                
               
            }
            catch { 
            
            }
            return resultSet;

        }
    }
}
