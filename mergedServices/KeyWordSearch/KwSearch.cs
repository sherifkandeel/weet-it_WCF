using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web;

namespace mergedServices
{
    //[DataContract]
    public static class KwSearch
    {
        static string[] versus_delimeter = new string[] { " vs ", "VS", "Vs", "vS" };
        static char[] space_delimeter = new char[] { ' ' };
        //static SparqlRemoteEndpoint remoteEndPoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql"));

        
        
        /// <summary>
        /// generates the sparql bifcontains syntax for multiple word keywords 
        /// </summary>
        /// <param name="keyword">keyword to operate on</param>
        /// <returns></returns>
        private static string bifcont_generator(string keyword)
        {
            List<string> kw_words;
            kw_words = keyword.Split(space_delimeter,StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            for (int i = 0; i < kw_words.Count; i++)
            {
                kw_words[i] = kw_words[i].Trim();
                kw_words[i] = "\"" + kw_words[i] + "\"";
            }
            return string.Join("and", kw_words);
        }
        private static string bifcont_generator1(string keyword)
        {
            List<string> kw_words;
            kw_words = keyword.Split(space_delimeter, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            for (int i = 0; i < kw_words.Count; i++)
            {
                kw_words[i] = kw_words[i].Trim();
                kw_words[i] = "\"" + kw_words[i] + "\"";
            }
            return string.Join("or", kw_words);
        }
        private static int scorecalc(string keyword, SparqlResult singleuri)
        {
            List<string> disambiguate_links = new List<string>();
            string check_disambig;
            int score_redirect = 0;
            int score_resource = 0;
            SparqlResultSet result;
            score_resource = computeLevenshteinDistance(keyword, singleuri.Value("literal").ToString().Replace("@en", ""));
            if (singleuri.Value("redirects") != null)
            {
                if (singleuri.Value("redirects").ToString() == "http://dbpedia.org/resource/Astro")
                {
                    check_disambig = "gg";
                }
                if (disambiguate_links.Contains(singleuri.Value("redirects").ToString()))
                    return 1000;
                check_disambig = "ASK where {<" + singleuri.Value("redirects").ToString() + "> <http://dbpedia.org/ontology/wikiPageDisambiguates>    ?disamb                       }";
                result = Request.RequestWithHTTP(check_disambig);
                if (result.Result == true)
                {
                    disambiguate_links.Add(singleuri.Value("redirects").ToString());
                    return 1000;
                }

                string disamb_query = "select * where{ <" + singleuri.Value("redirects").ToString() + "><http://www.w3.org/2000/01/rdf-schema#label> ?redirect_label}";
                result = Request.RequestWithHTTP(disamb_query);
                if (result.Count != 0)
                {
                    score_redirect = computeLevenshteinDistance(keyword, result[0].Value("redirect_label").ToString().Replace("@en", ""));
                    return (score_redirect <= score_resource ? score_redirect : score_resource);
                }
                else
                {
                    return score_resource;
                }
            }
            return score_resource;

        }
        private static List<string> Find_URIs(string keyword, int MaxUris)
        {


            int levdistance;

            SparqlResultSet result = new SparqlResultSet();
            string query = null;
            List<int> scores = new List<int>(1);
            List<string> uris = new List<string>(1);

            string bifcontains = bifcont_generator(keyword);

            query =
               "select distinct ?subject ?literal ?redirects where{" + 
               "?subject <http://www.w3.org/2000/01/rdf-schema#label> ?literal." + 
               "optional { ?subject <http://dbpedia.org/ontology/wikiPageRedirects> ?redirects}." + 
               "optional {?subject <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?type}." +
               "optional { ?subject   <http://dbpedia.org/ontology/wikiPageDisambiguates>  ?disamb}"+
"Filter ( ( !bound(?type) && !bound(?disamb)) ||  ( !(?type=<http://www.w3.org/2004/02/skos/core#Concept>)&& !(?type= <http://www.w3.org/1999/02/22-rdf-syntax-ns#Property>))) ." +
 
                "?literal bif:contains '" + bifcontains + "'.}" + "limit" + " 100";





            result = Request.RequestWithHTTP(query);

            if (result.Count == 0)
            {
                //a panic mode to be added to generate a more generic query with more results
                bifcontains = bifcont_generator1(keyword);
                query =
                  "select distinct ?subject ?literal ?redirects where{" +
               "?subject <http://www.w3.org/2000/01/rdf-schema#label> ?literal." +
               "optional { ?subject <http://dbpedia.org/ontology/wikiPageRedirects> ?redirects}." +
               "optional {?subject <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?type}." +
               "optional { ?subject   <http://dbpedia.org/ontology/wikiPageDisambiguates>  ?disamb}" +
"Filter ( ( !bound(?type) && !bound(?disamb)) ||  ( !(?type=<http://www.w3.org/2004/02/skos/core#Concept>)&& !(?type= <http://www.w3.org/1999/02/22-rdf-syntax-ns#Property>))) ." +

                "?literal bif:contains '" + bifcontains + "'.}" + "limit" + " 100";

                result = Request.RequestWithHTTP(query);
                if (result.Count == 0)
                    return null;
              

            }

           
                int iterator = 0;
                //add first result to list
                // levdistance = computeLevenshteinDistance(keyword, result[0].Value("literal").ToString());
                levdistance = scorecalc(keyword, result[0]);
                scores.Add(levdistance);
                if (result[0].Value("redirects") == null)
                {


                    uris.Add(result[0].Value("subject").ToString());
                }
                else
                {

                    uris.Add(result[0].Value("redirects").ToString());
                }

                foreach (SparqlResult uri in result)
                {
                  
                    bool broke = false;
                    iterator = 0;
                    levdistance = scorecalc(keyword, uri);

                    foreach (int score in scores.ToList())
                    {

                        if (levdistance <= score)
                        {

                            if (uri.Value("redirects") == null && !(uris.Contains(uri.Value("subject").ToString())))
                            {

                                scores.Insert(iterator, levdistance);
                                uris.Insert(iterator, uri.Value("subject").ToString());
                            }
                            else if (uri.Value("redirects") != null && !(uris.Contains(uri.Value("redirects").ToString())))//ensure uri is not already in the list
                            {
                                scores.Insert(iterator, levdistance);
                                uris.Insert(iterator, uri.Value("redirects").ToString());
                            }
                            else if ((uris.Contains(uri.Value("subject").ToString()) || uris.Contains(uri.Value("redirects").ToString())) && uri.Value("redirects") != null && levdistance <= scores[uris.IndexOf(uri.Value("redirects").ToString())])
                            {
                                int redundant = 0;
                                try
                                {
                                    redundant = uris.IndexOf(uri.Value("redirects").ToString());
                                }
                                catch
                                {
                                    redundant = uris.IndexOf(uri.Value("subject").ToString());

                                }
                                uris.RemoveAt(redundant);
                                scores.RemoveAt(redundant);
                                scores.Insert(iterator, levdistance);
                                try
                                {
                                    uris.Insert(iterator, uri.Value("redirects").ToString());
                                }
                                catch
                                {
                                    uris.Insert(iterator, uri.Value("subject").ToString());
                                }

                            }
                            broke = true;

                            break;

                        }
                       
                        iterator++;
                    }
                    if (broke == false)
                    {


                        if (uri.Value("redirects") == null && !(uris.Contains(uri.Value("subject").ToString())))
                        {
                            uris.Add(uri.Value("subject").ToString());
                            scores.Add(levdistance);
                        }

                        else
                        {
                            if (uri.Value("redirects") != null && !(uris.Contains(uri.Value("redirects").ToString())))//ensure uri is not already in the list
                            {
                                uris.Add(uri.Value("redirects").ToString());
                                scores.Add(levdistance);
                            }
                        }
                    }
                }




            
            if (uris.Count >= MaxUris)
                return uris.GetRange(0, MaxUris);
            else return uris;

        }

        /// <summary>
        /// get the uri that best matches  given keyword(it can be a multiple word keyword), to get the top n matches for 
        /// the given keyword, please use the overloaded method geturi(string keyworn,in MaxUris)
        /// </summary>
        /// <param name="input_query">keyword to get uri for</param>
        /// <returns>the uri that best matches the given keyword</returns>
        /// 

        public static string geturi(string keyword)
        {
            
            return Find_URIs(keyword, 1)[0];
        }
        /// <summary>
        /// gets the top n uris that best matches the given keyword ,you can specify MaxURis with 1 to get the best matching one
        /// /// </summary>
        /// <param name="keyword">keyword to get uri for</param>
        /// <param name="MaxUris">the number of top rated uris to get</param>
        /// <returns>A list of the top uris that best match the given keyword</returns>
        public static List<string> geturi(string keyword, int MaxUris = 1)
        {

            return Find_URIs(keyword, MaxUris);
        }
        /// <summary>
        /// takes a vs separated keywords and finds bets matching uri for each of them
        /// </summary>
        /// <param name="text">string to be split</param>
        /// <returns>List of uris to the given "vs" separated keywords</returns>
        public static List<string> GetUris_VsKeywords(string text)
        {

            List<string> Parsed_keywords = (text.Split(versus_delimeter, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
            for (int i = 0; i < Parsed_keywords.Count; i++)
            {
                Parsed_keywords[i] = Parsed_keywords[i].Trim();


            }
            return geturis_List(Parsed_keywords);

        }
        /// <summary>
        /// takes a vs separated keywords and finds best matching uri for each of them
        /// </summary>
        /// <param name="text">string to be split</param>
        /// <returns>Comma separated uris to the given "vs" separated keywords</returns>
        public static string GetUris_VsKeyword_comma(string text)
        {

            List<string> Parsed_keywords = (text.Split(versus_delimeter, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
            for (int i = 0; i < Parsed_keywords.Count; i++)
            {
                Parsed_keywords[i] = Parsed_keywords[i].Trim();


            }
            return string.Join(",", geturis_List(Parsed_keywords).ToArray());

        }


        /// <summary>
        /// same function as geturi(string keyword) but takes a list of keywords as an argument,it finds the best matching uri for every 
        /// keyword in the list
        /// </summary>
        /// <param name="keywords">The List of keywords to get best matching uris for</param>
        /// <returns>A List of best matching uris for every keyword in the list(ordered in the same order as the input List)</returns>
        /// 

        public static List<string> geturis_List(List<string> keywords)
        {
            List<string> uris = new List<string>();
            foreach (string keyword in keywords)
            {
                uris.Add(Find_URIs(keyword, 1)[0]);

            }
            return uris;

        }
        /// <summary>
        /// takes a list of keywords and returns -for each keyword- A list of best matching uris, 
        /// </summary>
        /// <param name="keywords">List of keywords to get top n matching uris for each one</param>
        /// <param name="MaxUris">List of lists containing a list of best matching uris for each keyword given</param>
        /// <returns></returns>
        public static List<List<string>> geturis_List_WithMaxuris(List<string> keywords, int MaxUris)
        {
            List<List<string>> uris = new List<List<string>>();
            foreach (string keyword in keywords)
            {
                uris.Add(Find_URIs(keyword, MaxUris));

            }
            return uris;

        }
        private static int computeLevenshteinDistance(string s, string t)
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
        //private string permut_panic(string keyword)
        //{

        //}
    }
}
