using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using VDS.RDF.Query;
using VDS.RDF;

namespace mergedServices
{
    public class RelFinder
    {
        /// <summary>
        ///  it's used to get combinations of URIs each combination contains a different pair of URIs 
        /// </summary>
        /// <param name="s">List of URIs</param>
        /// <returns>List containing combination of URIs pairs to be sent into the QueryGenerator to find relation between each pair</returns>
        private static List<KeyValuePair<string, string>> getCombinations(List<string> s)
        {
            List<KeyValuePair<string, string>> combinations = new List<KeyValuePair<string, string>>();

            foreach (string i in s)
            {
                foreach (string j in s)
                {
                    if (!i.Equals(j) && !combinations.Contains(new KeyValuePair<string, string>(i, j)))
                    {
                        combinations.Add(new KeyValuePair<string, string>(i, j));
                    }
                }
            }
            comparer c = new comparer();
            combinations = combinations.Distinct(c).ToList();
            return combinations;

        }

        /// <summary>
        /// gets the relations between a list of uris the relations is in form of predicates and objects 
        /// </summary>
        /// <param name="uri"> is the list of URI to fnd relation between</param>
        /// <param name="Distance">find the relations that lies at this distance ie : if distance = 3 there are 3 predicates between two URIs</param>
        /// <param name="Limit">the limit of relations to find between each two URIs</param>
        /// <returns>list of containing lists , each lists contains strings in order to discribe the relation 
        ///  Egypt >> type >> populated place << type<< syria 
        /// </returns>
        public static List<List<string>> getRelations(List<string> uri, int Distance, int Limit = 50)
        {

           List<string> ignoredPredicates = Regex.Split(File.ReadAllText("ignoredPredicates.txt"), "\r\n|\n").ToList();
           List<string> ignoredObjects = Regex.Split(File.ReadAllText("ignoredObjects.txt"), "\r\n|\n").ToList();

            //Making combinations of URIs to get relations between 
            List<KeyValuePair<string, string>> URIPairs = getCombinations(uri);

            SPARQLQueryBuilder QueryBuilder = new SPARQLQueryBuilder();
            List<SPARQLQueryBuilder.InnerQuery> Queries = new List<SPARQLQueryBuilder.InnerQuery>();

            //Building Queries 
            foreach (KeyValuePair<string, string> pair in URIPairs)
            {
                List<SPARQLQueryBuilder.InnerQuery> tmpinnerQueries = QueryBuilder.buildQueries(pair.Key, pair.Value, Distance, Limit, ignoredObjects, ignoredPredicates, 1);
                Queries.AddRange(tmpinnerQueries);
            }


            //executing Queries
            List<List<string>> URIS = new List<List<string>>();
            foreach (SPARQLQueryBuilder.InnerQuery Q in Queries)
            {
                SparqlResultSet resultSet = QueryHandler.ExecuteQueryWithString(Q.queryText);

                if (Q.connectState == SPARQLQueryBuilder.connectionType.connectedDirectly || Q.connectState == SPARQLQueryBuilder.connectionType.connectedDirectlyInverted)
                {
                    foreach (SparqlResult result in resultSet)
                    {
                        List<string> toAdd = new List<string>();
                        toAdd.Add(Q.object1);
                        /// foreach (KeyValuePair<string, INode> row in result.ToList());
                        for (int i = 1; i <= Distance; i++)
                        {
                            if (result.HasValue("pf" + i))
                                toAdd.Add(result.Value("pf" + i).ToString());

                            if (result.HasValue("of" + i))
                                toAdd.Add(result.Value("of" + i).ToString());
                        }
                        toAdd.Add(Q.object2);
                        URIS.Add(toAdd);
                    }
                }
                else if (Q.connectState == SPARQLQueryBuilder.connectionType.connectedViaMiddle || Q.connectState == SPARQLQueryBuilder.connectionType.connectedViaMiddleInverted)
                {

                    foreach (SparqlResult result in resultSet)
                    {
                        List<string> toAdd = new List<string>();
                        toAdd.Add(Q.object1);

                        bool lastAdded = false; // false for literal . true for predicate
                        for (int i = 1; i < Distance; i++)
                        {
                            if (result.HasValue("pf" + i) || result.HasValue("of" + i))
                            {
                                if (!lastAdded)
                                {
                                    if (result.HasValue("pf" + i))
                                    {
                                        toAdd.Add(result.Value("pf" + i).ToString());
                                        lastAdded = true;
                                    }
                                    if (result.HasValue("of" + i))
                                    {
                                        toAdd.Add(result.Value("of" + i).ToString());
                                        lastAdded = false;
                                    }
                                }
                                else
                                {
                                    if (result.HasValue("of" + i))
                                    {
                                        toAdd.Add(result.Value("of" + i).ToString());
                                        lastAdded = false;
                                    }
                                    if (result.HasValue("pf" + i))
                                    {
                                        toAdd.Add(result.Value("pf" + i).ToString());
                                        lastAdded = true;
                                    }
                                }
                            }
                        }

                        toAdd.Add(result.Value("middle").ToString());
                        lastAdded = false;   // false for literal . true for predicate
                        for (int i = Distance; i >= 1; i--)
                        {
                            if (result.HasValue("ps" + i) || result.HasValue("os" + i))
                            {
                                if (!lastAdded)
                                {
                                    if (result.HasValue("ps" + i))
                                    {
                                        toAdd.Add(result.Value("ps" + i).ToString());
                                        lastAdded = true;
                                    }
                                    if (result.HasValue("os" + i))
                                    {
                                        toAdd.Add(result.Value("os" + i).ToString());
                                        lastAdded = false;
                                    }
                                }
                                else
                                {
                                    if (result.HasValue("os" + i))
                                    {
                                        toAdd.Add(result.Value("os" + i).ToString());
                                        lastAdded = false;
                                    }
                                    if (result.HasValue("ps" + i))
                                    {
                                        toAdd.Add(result.Value("ps" + i).ToString());
                                        lastAdded = true;
                                    }
                                }
                            }

                        }
                        toAdd.Add(Q.object2);
                        URIS.Add(toAdd);
                    }
                }
            }

            //relationsComparer c = new relationsComparer(); 
            return URIS;//.Distinct(c).ToList();

        }

        /// <summary>
        /// gets the relations between a list of labels the relations is in form of predicates and objects 
        /// </summary>
        /// <param name="uri"> is the list of URI to fnd relation between</param>
        /// <param name="Distance">find the relations that lies at this distance ie : if distance = 3 there are 3 predicates between two URIs</param>
        /// <param name="Limit">the limit of relations to find between each two URIs</param>
        /// <returns>list of containing lists , each lists contains strings in order to discribe the relation 
        ///  Egypt >> type >> populated place << type<< syria 
        /// </returns>
        public static List<List<KeyValuePair<string, string>>> getRelationWithLabels(List<string> uri, int Distance, int Limit = 50)
        {
            List<List<string>> relationsWithoutLabels = getRelations(uri, Distance, Limit);
            List<List<KeyValuePair<string, string>>> relationsWithLabels = new List<List<KeyValuePair<string, string>>>();
            HashTable<string, string> labelsTable = new HashTable<string, string>();

            foreach (List<string> list in relationsWithoutLabels)
            {
                List<KeyValuePair<string , string >> toAdd = new List<KeyValuePair<string,string>>() ; 
                foreach (string s in list)
                {
                    if (!labelsTable.ContainsKey(s))
                    {
                        string label = util.getLabel(s);
                        labelsTable.Add(s, label);
                    }
                    toAdd.Add(new KeyValuePair<string,string>(s, labelsTable.GetValues(s).ToArray()[0]));
                }
                relationsWithLabels.Add(toAdd);
            }

            return relationsWithLabels;
        }
    }


    #region helpers

    /// <summary>
    /// class used as a comparer implementes Equalitycomparer interface used to get combinations of the URI
    /// </summary>
    public class comparer : IEqualityComparer<KeyValuePair<string, string>>
    {

        public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
        {
            if (x.Key.Equals(y.Key))
            {
                if (x.Value.Equals(y.Value))
                    return true;
            }
            if (x.Value.Equals(y.Key))
            {
                if (x.Key.Equals(y.Value))
                    return true;
            }

            return false;
        }


        public int GetHashCode(KeyValuePair<string, string> obj)
        {
            string x = (obj.Key.GetHashCode() >= obj.Value.GetHashCode()) ? obj.Key + obj.Value : obj.Value + obj.Key;
            return x.ToString().GetHashCode();
        }

    }

    public class relationsComparer : IEqualityComparer<List<string>>
    {
        public bool Equals(List<string> x, List<string> y)
        {
            bool equivalent = true;
            foreach (string i in x)
            {
                bool equals = false;
                foreach (string j in y)
                {
                    if (i == j)
                        equals = true;
                }
                if (!equals)
                {
                    equivalent = false;
                    break;
                }
            }

            return equivalent;

        }

        public int GetHashCode(List<string> obj)
        {
            string hash = "";
            obj.Sort();
            foreach (string s in obj)
                hash += s.GetHashCode();

            return hash.GetHashCode();
        }
    }

    #endregion
}
