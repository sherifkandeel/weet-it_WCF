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
          
            LiteralProfile LP = new LiteralProfile(subject_label, predicate_label, object_value, pred_URI);
            return LP;
        }

        public Profile ConstructProfile(String subjectURI, choiceProfile profile, int resultLimit = 10)
        {
            if (profile == choiceProfile.micro)
            {
                MicroProfile micro = new MicroProfile();
                micro.Abstract = getAbstract(subjectURI);
                micro.Label = util.getLabel(subjectURI);
                micro.Picture = imageGrapper.get_fb_link(subjectURI, imageGrapper.E.small);
                micro.URI = subjectURI;
                return micro;
            }
            else if (profile == choiceProfile.mini)
            {
                MiniProfile mini = new MiniProfile();
                mini.Abstract = getAbstract(subjectURI);
                mini.Label = util.getLabel(subjectURI);
                mini.URI = subjectURI;
                mini.Details = setProfileContents("mini", subjectURI, resultLimit);
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
                foreach (String rel in relations)
                {
                    Entity en = new Entity();
                    en.URI = rel;
                    en.Label = util.getLabel(rel);
                    en.Picture = imageGrapper.get_fb_link(rel, imageGrapper.E.small);
                    related.Add(en);
                }
                full.Related = related;
                full.Details = setProfileContents("full", subjectURI, resultLimit);
                full.Picture = imageGrapper.get_fb_link(subjectURI, imageGrapper.E.large);
                return full;
            }
            return null;
        }

        private List<KeyValuePair<String, List<Entity>>> setProfileContents(String profileType, String subjectURI, int resultLimit)
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
                            if (elem.Attribute("queryType").Value == "getObjects")
                                entities = getQueryResults("getObjects", subjectURI, (elem.Value), resultLimit);
                            else if (elem.Attribute("queryType").Value == "getSubjects")
                            {
                                entities = getQueryResults("getSubjects", subjectURI, (elem.Value), resultLimit);
                            }
                            KeyValuePair<String, List<Entity>> key = new KeyValuePair<String, List<Entity>>(util.getLabel(elem.Value), entities);
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
                    if (result.Value("obj").ToString().Contains("dbpedia.org"))
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
    }
}