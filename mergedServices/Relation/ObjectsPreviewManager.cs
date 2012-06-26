using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VDS.RDF;
using VDS.RDF.Query;

namespace mergedServices
{
    public class ObjectsPreviewManager
    {
        //this one holds the generated queries for a list of URIs
        private List<string> generatedQueries = new List<string>();


        //it's a list because if you want to retrieve and preview more than one
        public List<string> run(string URI)
        {
            generateQueries(URI);

            List<List<string>> resultStrings=new List<List<string>>();
            resultStrings = getResultsAsStrings(generatedQueries, URI);
            return getListofPreviewJsonObjs(resultStrings);            
 
        }



        private void generateQueries(string URI)
        {
            //resetting the generated queries list
            generatedQueries = new List<string>();
            
                //adding the results of the query
                generatedQueries.Add(
                    "select distinct * where{?s ?p <" + URI +
                    ">. filter (?p != <http://dbpedia.org/ontology/wikiPageWikiLink>)}");
                generatedQueries.Add("select distinct * where{"+
                    "<" + URI + "> ?p ?o. filter (?p != <http://dbpedia.org/ontology/wikiPageWikiLink>)}");  

        }

        private List<List<string>> getResultsAsStrings(List<string> queries, string obj)
        {
            
            //QueryProcessor.startConnection();
            List<SparqlResultSet> resultSet = new List<SparqlResultSet>();
            foreach (string item in queries)
            {
                //resultSet.Add(QueryProcessor.ExecuteQueryWithString(item));
                resultSet.Add(Request.RequestWithHTTP(item));
            }
            
            return convertResultSetListToStrings(resultSet, obj);
        }


        private List<string> getListofPreviewJsonObjs(List<List<string>> input)
        {
            List<string> toReturn = new List<string>();
            foreach (List<string> ls in input)
            {
                
                //string ob1 = ResSetToJSON.namer(ls[0]);
                //string pred = ResSetToJSON.namer(ls[1]);
                //string ob2 = ResSetToJSON.namer(ls[2]);

                string ob1 = util.getLabel(ls[0]);
                string pred = util.getLabel(ls[1]);
                string ob2 = util.getLabel(ls[2]);

                
                    string a = "{nodes:{";
                    a += "\"" + ob1 + "\":{\"uri\":\"" + ls[0] + "\",\"shape\":\"rectangle\",\"color\":\"#956BC9\",\"label\":\"" + ob1 + "\"},";
                    a += "\"" + pred + "P" + "\":{\"uri\":\"" + ls[1] + "\",\"shape\":\"rectangle\",\"color\":\"#9E9E9E\",\"label\":\"" + pred + "\"},";
                    a += "\"" + ob2 + "o" + "\":{\"uri\":\"" + ls[2] + "\",\"shape\":\"rectangle\",\"color\":\"#FF8800\",\"label\":\"" + ob2 + "\"}";
                    a += "}";

                    a += ",edges:{";
                    a += "\"" + ob1 + "\":{\"" + pred + "P" + "\":{}}," + pred + "P" + ":{\"" + ob2 + "o" + "\":{}}}";
                    a += "}";

                    toReturn.Add(a);
            }
            return toReturn;
        }
        





        /// <summary>
        /// converts a list of sparqlResultSet to a list of list of strings
        /// </summary>
        /// <param name="resultSet">the input list of sparql resultset</param>
        /// <param name="obj">the main object to reference</param>
        /// <returns>the list of list of results as strings</returns>
        private List<List<string>> convertResultSetListToStrings( List<SparqlResultSet> resultSet,string obj)
        {
            List<List<string>> toReturn = new List<List<string>>();
            List<string> eachResult = new List<string>();

            //we're iterating through each result by itself
            int counter = 1;
            foreach (SparqlResultSet res in resultSet)
            {
                //add the first resultset to a separate list<string>
                if (counter == 1)
                {
                    
                    foreach (SparqlResult r in res)
                    {
                        eachResult = new List<string>();
                        eachResult.Add(obj);
                        eachResult.Add((r.Value("p")).ToString());
                        eachResult.Add((r.Value("s")).ToString());
                        toReturn.Add(eachResult);
                    }
                    
                    counter++;
                }
                 
                    //add the second resultset to a separate list<string>
                else
                {
                    
                    foreach (SparqlResult r in res)
                    {
                        eachResult = new List<string>();
                        eachResult.Add(obj);
                        eachResult.Add((r.Value("p")).ToString());
                        eachResult.Add((r.Value("o")).ToString());
                        toReturn.Add(eachResult);
                    }
                    

                }

            }

            return toReturn;
        }

        //public List<string> getResultsInJSONArray()
        //{ 

        //}


    }
}