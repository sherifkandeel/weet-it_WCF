using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF;

namespace mergedServices
{
    class RelationGenerator
    {
        /// <summary>
        /// getting entities from the same categories as the original entities and sorting them by the number of occurences
        /// </summary>
        /// <param name="uri">the uri to get the related entities for</param>
        /// <param name="MaxNumberOfEntities">Maximum number of related entities to return</param>
        /// <returns>list of the related entities</returns>
        public static List<String> getRelatedEntities(String uri, int MaxNumberOfEntities=7)
        {
            //SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://weetit:8890/sparql"));
            List<KeyValuePair<int, String>> relations = new List<KeyValuePair<int, String>>();

            Console.WriteLine("Will make queries now ");

            //Getting Everything
            List<SparqlResultSet> sets = new List<SparqlResultSet>();
            sets.Add(Request.RequestWithHTTP("select ?z where {<" + uri + "> ?x ?z.?z <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Thing> } limit 1000"));
            sets.Add(Request.RequestWithHTTP("select ?z where {?z ?x <" + uri + ">.?z <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Thing> } limit 1000"));
            
            //first stage, get related with the same subject
            //Query to extract subjects
            SparqlResultSet cateogires =Request.RequestWithHTTP("select ?z where {<" + uri + "> <http://purl.org/dc/terms/subject> ?z} limit 1000");

            //list to hold all items of the same cateogires
            List<String> allItems = new List<String>();

            //adding only the results
            foreach (SparqlResultSet item in sets)
            {
                foreach (SparqlResult res in item)
                {
                    if (res.Value("z").NodeType == NodeType.Uri)
                        allItems.Add(((UriNode)res.Value("z")).Uri.ToString());
                }
            }


            //adding cateogires
            foreach (SparqlResult category in cateogires)
            {
                //get all entities in all categoreis                
                SparqlResultSet itemsInCategory = Request.RequestWithHTTP(
                    "select ?z where {?z <http://purl.org/dc/terms/subject> <" + ((UriNode)category.Value("z")).Uri.ToString() + ">."
                    + "?z <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Thing>} limit 1000");

                foreach (SparqlResult item in itemsInCategory)
                {
                    allItems.Add(((UriNode)item.Value("z")).Uri.ToString());
                }

            }


            //reordering 
            var g = allItems.GroupBy(i => i).ToList();
            List<KeyValuePair<int, String>> orderedEntities = new List<KeyValuePair<int, String>>();
            foreach (var grp in g)
            {
                orderedEntities.Add(new KeyValuePair<int, String>(grp.Count(), grp.Key));
            }
            orderedEntities = orderedEntities.OrderByDescending(k => k.Key).ToList();
            //removing the original URI (sometimes it appears again, that's why we remove it)

            orderedEntities.RemoveAll(e => e.Value.Equals(System.Web.HttpUtility.UrlDecode((uri))));
            orderedEntities.RemoveAll(e => e.Value.Equals(uri));


            //Returning the required list
            List<String> toReturn = new List<String>();
            int max = MaxNumberOfEntities;
            if (orderedEntities.Count < max)
                max = orderedEntities.Count;
            for (int i = 0; i < max; i++)
            {
                toReturn.Add(orderedEntities[i].Value);
            }
            return toReturn;
        }

    }
}
