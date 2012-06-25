using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using VDS.RDF.Query;
using VDS.RDF;

namespace mergedServices
{
    //[ServiceBehavior()]
    partial class MergedService : CompareWithOnePredicateInterface
    {
        public List<List<String>> CompareWithRespect(List<String> subjectsNames, String predicateURI,int limit=50)
        {
            String subjects = findURIs(subjectsNames);

            List<List<String>> objects = getObjects(subjects.Split(',').ToList<String>(), predicateURI,limit);
            return objects;
        }

        private int computeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        private String findURIs(List<String> subjectsNames)
        {
            //SparqlRemoteEndpoint remoteEndPoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql"));
            SparqlResultSet result = new SparqlResultSet();
            string query = null;
            List<int> scores = new List<int>();
            List<string> uris = new List<string>();
            string comma_sep_uris;

            for (int i = 0; i < subjectsNames.Count; i++)
            {
                //query = "select distinct * where{" +
                //    "<http://dbpedia.org/resource/Inception> ?x ?y}";

                //here a for loop to query the similar subjectsNames and add found uris to uris List<string>
                //if a URI is not found a "not found" string is put instead of the URI


                query = "select distinct  ?subject ?literal ?redirects where{" +
                        "?subject <http://www.w3.org/2000/01/rdf-schema#label> ?literal." +
                        "optional {   ?subject <http://dbpedia.org/ontology/wikiPageRedirects> ?redirects}." +
                        "optional {?subject <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?type}." +
                        "Filter (!bound(?type) || !(?type=<http://www.w3.org/2004/02/skos/core#Concept>))." +
                        "?literal bif:contains '\"" + subjectsNames[i] + "\"'.}" + "limit 100";

                //query = "select distinct * where  {?t1 <http://www.w3.org/2000/01/rdf-schema#label> \"The Dark Knight\"   @en }";

                //result = remoteEndPoint.QueryWithResultSet(query);
                result = Request.RequestWithHTTP(query);


                //QueryProcessor.closeConnection();
                if (result.Count == 0)
                {
                    //a panic mode to be added to generate a more generic query with more results
                    uris.Add("");
                    continue;
                }
                else if (result.Count == 1)
                {
                    if ((result[0].Value("redirects") == null))
                        uris.Add(result[0].Value("subject").ToString());
                    else
                        uris.Add(result[0].Value("redirects").ToString());
                    continue;
                }
                else
                {

                    int new_value;
                    int min_value = 1000;
                    int max_index = 0;
                    for (int j = 0; j < result.Count; j++)
                    {

                        new_value = (computeLevenshteinDistance(subjectsNames[i], result[j].Value("literal").ToString()));
                        scores.Add(new_value);
                        if (new_value < min_value)
                        {
                            max_index = j;
                            min_value = new_value;
                        }
                        else if (new_value == min_value)
                        {
                            if (result[j].Value("redirects") == null)
                            {
                                max_index = j;
                                min_value = new_value;

                            }
                            else
                            {
                                min_value = new_value;
                            }

                        }
                    }
                    if ((result[max_index].Value("redirects") == null))
                        uris.Add(result[max_index].Value("subject").ToString());
                    else
                        uris.Add(result[max_index].Value("redirects").ToString());

                    min_value = 0;
                }
            }
            comma_sep_uris = string.Join(",", uris.ToArray());
            return comma_sep_uris;
        }
       

        private List<List<String>> getObjects(List<String> SubjectsURIs, String predicateURI, int limit)
        {
            //SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql"));
            List<List<String>> objectsLabels = new List<List<String>>();
            foreach (String i in SubjectsURIs)
            {
                string query = "select * where {<" + i + "> <" + predicateURI + "> ?obj} limit "+limit;
                List<String> buff = new List<String>();
                //SparqlResultSet results = endpoint.QueryWithResultSet(query);
                SparqlResultSet results = Request.RequestWithHTTP(query);

                foreach (SparqlResult result in results)
                {
                    if (((INode)result[0]).NodeType == NodeType.Uri)
                        buff.Add(util.getLabel(result.Value("obj").ToString()));
                    else
                    {
                        string s = "";
                        s += ((LiteralNode)result.Value("obj")).Value +" "+ util.getLabel(((LiteralNode)result.Value("obj")).DataType.ToString());
                        buff.Add(s);
                    }
                }
                objectsLabels.Add(buff);
            }
            return objectsLabels;
        }
    }
}