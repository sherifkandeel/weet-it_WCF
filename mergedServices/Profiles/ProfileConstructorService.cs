using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using VDS.RDF;
using System.Xml;
using System.IO;
using System.Xml.Linq;

namespace mergedServices
{
    //public class ProfileConstructor
    public partial class MergedService : ProfileConstructorInterface
    {
        public enum choiceProfile { micro, mini, full };

        public Profile ConstructLiteralProfile( string subjectURI, string predicate_label, string subject_label,string object_URI,string object_value,string pred_URI)
        {
          
            LiteralProfile LP = new LiteralProfile(subject_label, predicate_label, object_value, pred_URI,subjectURI);
            return LP;
        }
        /// <summary>
        /// The function gets all info about profile according to its type and the content of XML file it uses
        /// </summary>
        /// <param name="subjectURI">The URI of the subject resource as String</param>
        /// <param name="profile">Type of profile(full, mini, micro)</param>
        /// <param name="resultLimit">Max number of results got from queries</param>
        /// <returns>Object of one of the classes (FullProfile, MiniProfile, MicroProfile)</returns>
        public Profile ConstructProfile(String subjectURI, choiceProfile profile, int resultLimit = 10)
        {
            if (util.isInternalURI(subjectURI))
                subjectURI = util.encodeURI(subjectURI);
            subjectURI = useRedirection(subjectURI);
            
            if (profile == choiceProfile.micro)
            {
                MicroProfile micro = new MicroProfile();
                String abst = getAbstract(subjectURI);
                micro.IsShortAbstract = false;
                if ( abst != null && abst.Length > 280)
                {
                    String temp = abst.Substring(279);
                    micro.Abstract = abst.Substring(0, 280 + temp.IndexOf(" ")) + "...";
                    micro.IsShortAbstract = true;
                }
                else
                    micro.Abstract = abst;
                micro.Label = util.getLabel(subjectURI);
                micro.Picture = imageGrapper.get_fb_link(subjectURI, imageGrapper.E.small);
                micro.URI = subjectURI;
                return micro;
            }
            else if (profile == choiceProfile.mini)
            {
                MiniProfile mini = new MiniProfile();
                String abst = getAbstract(subjectURI);
                mini.IsShortAbstract = false;
                if (abst != null && abst.Length > 560)
                {
                    String temp = abst.Substring(559);
                    mini.Abstract = abst.Substring(0, 560 + temp.IndexOf(" ")) + " ...";
                    mini.IsShortAbstract = true;
                }
                else
                    mini.Abstract = abst;
                mini.Label = util.getLabel(subjectURI);
                mini.URI = subjectURI;
                mini.Details = setProfileDetails("mini", subjectURI, resultLimit);
                mini.Picture = imageGrapper.get_fb_link(subjectURI, imageGrapper.E.small);
                return mini;
            }
            else if (profile == choiceProfile.full)
            {
                FullProfile full = new FullProfile();
                full.Abstract = getAbstract(subjectURI);
                full.Label = util.getLabel(subjectURI);
                full.URI = subjectURI;
                List<String> relations = RelationGenerator.getRelatedEntities(subjectURI);
                List<Entity> related = new List<Entity>();
                for (int i = 0; i < relations.Count; i++)
                {
                    String rel = relations[i];
                    if (util.isInternalURI(rel))
                        rel = util.encodeURI(rel);
                    rel = useRedirection(rel);
                    Entity en = new Entity();
                    en.URI = rel;
                    en.Label = util.getLabel(rel);
                    en.Picture = imageGrapper.get_fb_link(rel, imageGrapper.E.small);
                    related.Add(en);
                }
                full.Related = related;
                full.Details = setProfileDetails("full", subjectURI, resultLimit);
                full.Picture = imageGrapper.get_fb_link(subjectURI, imageGrapper.E.large);
                full.Location = getLocation(subjectURI);

                return full;
            }
            return null;
        }

        /// <summary>
        /// The function gets the details part about profile according to its type and the content of XML file it uses
        /// </summary>
        /// <param name="profileType">Type of profile(full, mini, micro)</param>
        /// <param name="subjectURI">The URI of the subject resource as String</param>
        /// <param name="resultLimit">Max number of results got from queries</param>
        /// <returns>The details part of mini and full profiles</returns>
        private List<KeyValuePair<String, List<Entity>>> setProfileDetails(String profileType, String subjectURI, int resultLimit)
        {
            XDocument XMLDoc = XDocument.Load("profile content.xml");            
            String query = "select * where {<" + subjectURI + "> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?obj}";
            List<String> types = new List<String>();
            SparqlResultSet results = Request.RequestWithHTTP(query);
            List<KeyValuePair<String, List<Entity>>> profileContent = new List<KeyValuePair<String, List<Entity>>>();
            bool typeFound = false;
            foreach (SparqlResult result in results)
                types.Add(result.Value("obj").ToString());
            List<XElement> XMLList = XMLDoc.Root.Elements(profileType).Elements().ToList();
			foreach (XElement element in XMLList)
            {
                foreach (String type in types)
                {
                    if (element.Attribute("URI").Value == type)
                    {
                        var elements = element.Elements("predicate");
                        foreach (XElement elem in elements)
                        {
                            List<Entity> entities = new List<Entity>();
                            KeyValuePair<String, List<Entity>> key;
                            if (elem.Attribute("queryType").Value == "getObjects")
                            {
                                entities = getQueryResults("getObjects", subjectURI, (elem.Value), resultLimit);
                                key = new KeyValuePair<String, List<Entity>>(util.getLabel(elem.Value), entities);
                            }
                            else if (elem.Attribute("queryType").Value == "getSubjects")
                            {
                                entities = getQueryResults("getSubjects", subjectURI, (elem.Value), resultLimit);
                                key = new KeyValuePair<String, List<Entity>>(util.getLabel(elem.Value) + " of", entities);
                            }
                            //KeyValuePair<String, List<Entity>> key = new KeyValuePair<String, List<Entity>>(util.getLabel(elem.Value), entities);
                            else
                                key = new KeyValuePair<String, List<Entity>>();
                            profileContent.Add(key);
                        }
                        typeFound = true;
                        break;
                    }
                }
                if (typeFound == true)
                    break;
            }
            if (typeFound == false)
            {
                List<XElement> XMLListExecluded = XMLDoc.Root.Elements("execludedPredicates").Elements("predicate").ToList();
                String execluded = "";
                String x = profileType == "mini" ? "5" : "10";
                String queryExecluded = "select distinct ?predicate   where{" +
                "<" + subjectURI + ">" + "?predicate ?literal";
                foreach (XElement elementExecluded in XMLListExecluded)
                    execluded += "!(?predicate=<" + elementExecluded.Value + "> ) &&";
                if (execluded != "")
                {
                    execluded = execluded.Substring(0, execluded.Length - 2);
                    queryExecluded += ".FILTER (" + execluded + ")";
                }
                queryExecluded += "} limit  " + x;
                SparqlResultSet resultsExecluded = Request.RequestWithHTTP(queryExecluded);
                List<Entity> entitiesExecluded = new List<Entity>();
                List<String> predicateNames = new List<String>();
                foreach (SparqlResult result in resultsExecluded)
                    predicateNames.Add(result.Value("predicate").ToString());
                foreach (String predicateName in predicateNames)
                {
                    entitiesExecluded = getQueryResults("getObjects", subjectURI, predicateName, resultLimit);
                    KeyValuePair<String, List<Entity>> key = new KeyValuePair<String, List<Entity>>(util.getLabel(predicateName), entitiesExecluded);
                    profileContent.Add(key);
                }
            }
            return profileContent;
        }

        /// <summary>
        /// This function returns entities of subjects or objects depending on the first parameter
        /// if the result of the query is literal, its entity will have label only not URI
        /// </summary>
        /// <param name="type">If the query is responsible for getting objects or subjects</param>
        /// <param name="SubjectURI">The URI of the subject resource as String</param>
        /// <param name="predicateURI">The URI of the predicate as String</param>
        /// <param name="resultLimit">Max number of results got from queries</param>
        /// <returns>The entities got from the queries done in the function</returns>
        private List<Entity> getQueryResults(String type, String SubjectURI, String predicateURI, int resultLimit)
        {
            String query = "";
            if (type == "getObjects")
                query += "select * where {<" + SubjectURI + "><" + predicateURI + "> ?obj} limit " + resultLimit;
            else if (type == "getSubjects")
                query += "select * where{ ?obj <" + predicateURI + "> <" + SubjectURI + ">} limit " + resultLimit;
            SparqlResultSet results = Request.RequestWithHTTP(query);
            List<Entity> entities = new List<Entity>();
            foreach (SparqlResult result in results)
            {
                Entity en = new Entity();
                if ((((INode)result[0]).NodeType == NodeType.Uri))
                {
                    if (util.isInternalURI(result.Value("obj").ToString()))
                    {
                        en.Label = util.getLabel(result.Value("obj").ToString());
                        en.URI = result.Value("obj").ToString();
                    }
                    else               //Hack to get webpage without considering it URIs
                        en.Label = result.Value("obj").ToString();    
                }
                else
                {
                    en.Label = ((LiteralNode)result.Value("obj")).Value;
                }
                entities.Add(en);
            }
            return entities;

        }

        /// <summary>
        /// This function gets the abstract data of the resource with the URI sent using query
        /// </summary>
        /// <param name="SubjectURI">The URI of the subject resource as String</param>
        /// <returns>Abstract data about the entity of the URI</returns>
        private String getAbstract(String SubjectURI)
        {            
            String query = "select * where {<" + SubjectURI + "><http://dbpedia.org/ontology/abstract> ?obj}";
            SparqlResultSet results = Request.RequestWithHTTP(query);
            if (results.Count!=0)
            {
                SparqlResult result = results[0];
                return ((LiteralNode)result.Value("obj")).Value;
            }
            else return null;
        }

        /// <summary>
        /// This function gets latitude and longitude of the resource if found with the URI sent using query
        /// </summary>
        /// <param name="SubjectURI">The URI of the subject resource as String</param>
        /// <returns>Location object(latitude and longitude)</returns>
        private Location getLocation(String SubjectURI)
        {
            Location location = new Location();
            String query = "select * where {<" + SubjectURI + "><http://www.w3.org/2003/01/geo/wgs84_pos#long> ?obj}";
            SparqlResultSet resultsLong = Request.RequestWithHTTP(query);
            query = "select * where {<" + SubjectURI + "><http://www.w3.org/2003/01/geo/wgs84_pos#lat> ?obj}";
            SparqlResultSet resultsLat = Request.RequestWithHTTP(query);
            if (resultsLat.Count != 0 && resultsLong.Count != 0)
            {
                SparqlResult resultLat = resultsLat[0];
                SparqlResult resultLong = resultsLong[0];
                location.Latitude = ((LiteralNode)resultLat.Value("obj")).Value;
                location.Longitude = ((LiteralNode)resultLong.Value("obj")).Value;
            }
                return location;
        }

        /// <summary>
        /// This function returns the redirection page to the pages which have ones
        /// </summary>
        /// <param name="uri">This function gets the abstract data of the resource with the URI sent using query</param>
        /// <returns>the redirection page if found else it returns the same URI</returns>
        private String useRedirection(String uri)
        {
            String query = "select * where {<" + uri + "><http://dbpedia.org/ontology/wikiPageRedirects> ?obj}";
            SparqlResultSet results = Request.RequestWithHTTP(query);
            if (results.Count != 0)
            {
                SparqlResult result = results[0];
                return result.Value("obj").ToString();
            }
            else return uri;
        }
    }
}