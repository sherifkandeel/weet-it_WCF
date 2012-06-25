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
   
    /// <summary>
    /// 
    /// </summary>
    public static class QueryProcessor
    {
        public static VirtuosoManager manager;
        private static bool isConnectionStarted = false;
        
        /// <summary>
        /// starts connection with the server
        /// </summary>
        public static void startConnection()
        {
            //Read from the config file.
            StreamReader sr = new StreamReader("VirtuosoConnectionParameters.txt");
            
            //Initiating the manager(to be added to constructor?)
            if (!isConnectionStarted)
            {
                //manager = new VirtuosoManager("Server=localhost;Uid=dba;pwd=dba;Connection Timeout=500");                
                //manager = new VirtuosoManager("localhost", 1111, "DB", "dba", "dba");
                manager = new VirtuosoManager(sr.ReadLine(), int.Parse(sr.ReadLine()), sr.ReadLine(), sr.ReadLine(), sr.ReadLine());
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
        /// Exectues a certain List of InnerQuery objects
        /// </summary>
        /// <param name="input">the list of innerquery to be queried</param>
        /// <returns>a list of resultSet one for each innerquery.queryText</returns>
        public static List<ResSetToJSON.innerResult> ExecuteQueryWithInnerQuery(SPARQLQueryBuilder.InnerQuery input,string obj1,string obj2)
        {
            //list to hold the results
            List<ResSetToJSON.innerResult> resultsList = new List<ResSetToJSON.innerResult>();

            try
            {
                //temp result holder
                ResSetToJSON.innerResult temp;
                //fetching results and passing to the list

                temp = new ResSetToJSON.innerResult();

                //

                //temp.firstObj = obj1;
                //temp.lastObj = obj2;

                temp = setOriginalObjects(temp,input.queryText);
                
                temp.connectState = (int)input.connectState;
                temp.resultSets = ExecuteQueryWithString(input.queryText);
                
                //if there's any results add it
                if (temp.resultSets.Count > 0)
                    resultsList.Add(temp);


            }
            catch { }            
            return resultsList;
            
        }


        /// <summary>
        /// helper function to manage the changes of the queries first and last objects
        /// </summary>
        /// <param name="temp">the innerResult object ot set it's first and last object</param>
        /// <param name="input">the query string</param>
        /// <returns></returns>
        private static ResSetToJSON.innerResult setOriginalObjects(ResSetToJSON.innerResult temp, string input)
        {
            //List<string> urls = new List<string>();
            string one="";
            string two="";
            MatchCollection mc = Regex.Matches(input, @"\<(.*?)\>", RegexOptions.IgnoreCase);
            one = mc[0].Value.Replace("<", "");
            temp.firstObj = one.Replace(">", "");

            two = mc[1].Value.Replace("<", "");
            temp.lastObj = two.Replace(">", "");

            return temp;
        }



        /// <summary>
        /// overload of Execute query
        /// </summary>
        /// <param name="input">the query text as string</param>
        /// <returns></returns>
        public static SparqlResultSet ExecuteQueryWithString(string input)
        {
            //list to hold the results
            //SparqlResultSet resultSet = new SparqlResultSet();
            //try
            //{
            //    //just in cas someone didn't open the connection
            //    if (!isConnectionStarted)
            //        startConnection();
                
            //    //making the query
            //    Object result = manager.Query(input);
            //    //Object result = manager.ExecuteQuery(input);
            //    resultSet = (SparqlResultSet)result;                
               
            //}
            //catch { }
            //return resultSet;.


            return Request.RequestWithHTTP(input);

        }
    }
}
