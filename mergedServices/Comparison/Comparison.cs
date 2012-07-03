using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using System.Web;
using System.Runtime.Serialization;
using System.IO;

///usage:
///initialize an instance of the Comparison object giving it a list<string> uris you want to compare between
///then use getComparisonOutput() to get a list<Comparison.ResourceInformation> Object containing all the information about the resources
///Access any information you want to use by using the methods inside ResourceInformation DataStructure

namespace mergedServices
{
    [DataContract]
    public class Comparison
    {
        //[DataMember]
        //public SparqlRemoteEndpoint endpoint;

        [DataMember]
        public List<ResourceInformation> ComparisonOutput;
                
        public Comparison(List<string> uris, List<string> PredicatesToExclude = null)
        {
            //reads from file if the consumer didn't enter any predicates
            if (PredicatesToExclude == null)
            {
                PredicatesToExclude = loadPredicatesToExclude();
            }            
            
            //Initializing the server
            //Uri serverUri = new Uri("http://localhost:8890/sparql");            
            //endpoint = new SparqlRemoteEndpoint(serverUri);
            //endpoint.Timeout = 999999;
            ComparisonOutput = new List<ResourceInformation>();
            getData(uris, PredicatesToExclude);
        }

        private List<string> loadPredicatesToExclude()
        {
            List<string> toReturn = new List<string>();
            StreamReader sr = new StreamReader("PredicatesToExclude.txt");
            while (!sr.EndOfStream)
            {
                toReturn.Add(sr.ReadLine()); 
            }
            return toReturn;
        }


        public List<ResourceInformation> getComparisonOutput()
        {
            return ComparisonOutput;
        }

        /// <summary>
        /// Fills the FinalComparisonObject of each ResourceInformation 
        /// Part of the resource's info that will return when comparing
        /// </summary>
        /// <param name="input">Resource Information</param>
        /// <returns>The same input but after adding the information of FinalComparisonObject</returns>
        public List<ResourceInformation> getCommon(List<ResourceInformation> input)
        {
            List<List<KeyValuePair<string, string>>> AllPredicates = new List<List<KeyValuePair<string, string>>>();
            List<KeyValuePair<string, string>> commonPredicates = new List<KeyValuePair<string, string>>();

            foreach (ResourceInformation item in input)
            {
                var tempPredicates = new List<KeyValuePair<string, string>>();
                foreach (KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>> item2 in item.rawComparisonObject)
                {
                    tempPredicates.Add(item2.Key);
                }
                AllPredicates.Add(tempPredicates);
                                                
            }

            //Getting the intersection between all the lists
            var intersection = AllPredicates.Skip(1).Aggregate(new HashSet<KeyValuePair<string, string>>(AllPredicates.First()), (h, e) => { h.IntersectWith(e); return h; });
            commonPredicates = intersection.ToList();

            //populate the list and their members to each resourceInformation object
            //foreach (ResourceInformation item in input)            
            //util.clearLog();
            for (int i = 0; i < input.Count; i++)
            {
                //logging 
                //util.log("##############################" + input[i].ID.Value + "##############################");

                //comparing the common ones with the ones inside each resource to get the ones that match the key(predicate)
                //we should return to the object of copmarison part inside each resourceInformationObject
                foreach (KeyValuePair<string, string> item2 in commonPredicates)
                {
                    //item3 is each item of the predicate->resources list
                    foreach (KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>> item3 in input[i].rawComparisonObject)
                    {
                        //compare here
                        //if they're equal
                        if (item3.Key.Equals(item2))
                        {
                            input[i].FinalComparisonObject.Add(item3);

                            //For logging purposes only
                            //util.log(item3.Key.Value.ToString());
                            foreach (KeyValuePair<string,string> com in item3.Value)
                            {
                                string format = "{0," + item3.Key.Value.Length + "}{1,-10}";
                                //util.log(string.Format(format," ",com.Value));
                            }                            
                             
                        }

                        
                    }
                    //logging purposes
                    //util.log("-------------------------------------------------------------------------------------------------");
                }
               
            }

            return input;
        }

        /// <summary>
        /// logs all resources information, for testing
        /// </summary>
        /// <param name="input">list of all resourceInformation object</param>
        public void logResults(List<ResourceInformation> input)
        {
            //clearing log
            util.clearLog();
            foreach (ResourceInformation item in input)
            {
                util.log("####################################"+item.ID.Value+"###################################################################");
                util.log("####################################" + item.ID.Key + "###################################################################");
                foreach (KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>> item2 in item.rawComparisonObject)
                {
                    util.log(item2.Key.Value);
                    foreach (KeyValuePair<string, string> item3 in item2.Value)
                    {
                        //"{0,-10} {1,10}"
                        string format = "{0," + item2.Key.Value.Length + "}{1,-10}";
                        util.log(string.Format(format," ",item3.Value));
                    }
                }                
            }
        }

        /// <summary>
        /// Fills The comparisonComponent of each Resource To compare
        /// </summary>
        /// <param name="input">Object to fill its rawComparisonObject</param>
        /// <returns>The same resource Again</returns>
        public ResourceInformation fillComparisonComponent(ResourceInformation input)
        {
            //we have to make sure the resource information is not empty 
            if (input.resources_resourceIsSubj.Count > 1)
            {
                //temp to fill in
                List<KeyValuePair<string, string>> tempList = new List<KeyValuePair<string, string>>();
                KeyValuePair<string, string> tempPred;

                tempPred = input.predicates_resourceIsSubj[0];
                tempList.Add(input.resources_resourceIsSubj[0]);
                for (int i = 1; i < input.predicates_resourceIsSubj.Count; i++)
                {
                    if (input.predicates_resourceIsSubj[i].Equals(tempPred))
                    {
                        if (tempList.Count < 50)
                        {
                            KeyValuePair<string, string> temp = new KeyValuePair<string, string>(util.encodeURI(input.resources_resourceIsSubj[i].Key), input.resources_resourceIsSubj[i].Value);
                            tempList.Add(temp);
                        }
                    }
                    else
                    {
                        input.rawComparisonObject.Add(new KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>(tempPred, tempList));
                        tempPred = input.predicates_resourceIsSubj[i];
                        tempList = new List<KeyValuePair<string, string>>();

                        KeyValuePair<string, string> temp = new KeyValuePair<string, string>(util.encodeURI(input.resources_resourceIsSubj[i].Key), input.resources_resourceIsSubj[i].Value);
                        tempList.Add(temp);
                    }
                }

                input.rawComparisonObject.Add(new KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>(tempPred, tempList));


            }
            if (input.resources_resourceIsObj.Count > 1)
            {
                //temp to fill in
                List<KeyValuePair<string, string>> tempList = new List<KeyValuePair<string, string>>();
                KeyValuePair<string, string> tempPred;

                tempPred = input.predicates_resourceIsObj[0];
                tempList.Add(input.resources_resourceIsObj[0]);
                for (int i = 1; i < input.predicates_resourceIsObj.Count; i++)
                {

                    if (input.predicates_resourceIsObj[i].Equals(tempPred))
                    {
                        if (tempList.Count < 50)
                        {
                            KeyValuePair<string, string> temp = new KeyValuePair<string, string>(util.encodeURI(input.resources_resourceIsObj[i].Key), input.resources_resourceIsObj[i].Value);
                            tempList.Add(temp);
                        }
                    }
                    else
                    {
                        input.rawComparisonObject.Add(new KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>(tempPred, tempList.Distinct().ToList()));
                        tempPred = input.predicates_resourceIsObj[i];
                        tempList = new List<KeyValuePair<string, string>>();

                        KeyValuePair<string, string> temp = new KeyValuePair<string, string>(util.encodeURI(input.resources_resourceIsObj[i].Key), input.resources_resourceIsObj[i].Value);
                        tempList.Add(temp);
                    }
                }

                input.rawComparisonObject.Add(new KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>(tempPred, tempList.Distinct().ToList()));


            }
            //removing duplicates
            for (int i = 0; i < input.rawComparisonObject.Count; i++)
            {
                for (int j = i + 1; j < input.rawComparisonObject.Count; j++)
                {
                    if (input.rawComparisonObject[i].Key.Value.Equals(input.rawComparisonObject[j].Key.Value))
                    {
                        //we remove the one with the smallest resources
                        int indexofremoval = input.rawComparisonObject[i].Value.Count < input.rawComparisonObject[j].Value.Count ? i : j;
                        input.rawComparisonObject.RemoveAt(indexofremoval);
                    }
                }
            }


            return input;
        }
        
        
        /// <summary>
        /// gets the label of the uri, if can't find, it removes the last /
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string getLabel(string uri)
        {
           HashSet<KeyValuePair<string, string>> LabelsAlreadyGot = new HashSet<KeyValuePair<string, string>>();

           
            //at least best one for now
            uri = Uri.EscapeUriString(uri);
            
            DateTime now= DateTime.Now;
            //check to see if we already got it?
            foreach (KeyValuePair<string, string> item in LabelsAlreadyGot)
            {
                if (String.Equals(item.Key, uri))
                    return item.Value;
            }
	        //end check
            //Console.Write("ST=" + (DateTime.Now - now).TotalMilliseconds );

            
            string query = "select * where {<" + uri + "> <http://www.w3.org/2000/01/rdf-schema#label> ?obj}";
            //SparqlResultSet results = endpoint.QueryWithResultSet(query);
            SparqlResultSet results = Request.RequestWithHTTP(query);
            
            //if there's no results from the first query, we will try to get the name 
            if (results.Count < 1)            
            {
                string name_query = "select * where {<" + uri + "> <http://xmlns.com/foaf/0.1/name> ?obj}";
                //results = endpoint.QueryWithResultSet(name_query);
                results = Request.RequestWithHTTP(name_query);

                //if there's no result from the second query
                //get the name after the /
                if (results.Count < 1)                
                {
                    string toreturn = new string(uri.ToCharArray().Reverse().ToArray());//uri.Reverse().ToString();

                    toreturn = toreturn.Remove(toreturn.IndexOf("/"));
                    toreturn = new string(toreturn.ToCharArray().Reverse().ToArray());
                    toreturn = toreturn.Replace("_", " ");
                    //TODO : get back the encoding
                    toreturn = toreturn.Trim();

                    //Add it to the HashSet in RAM
                    LabelsAlreadyGot.Add(new KeyValuePair<string, string>(uri, toreturn));
                    return toreturn;
                }
                else
                {
                    //Add it to the HashSet in RAM
                    LabelsAlreadyGot.Add(new KeyValuePair<string, string>(uri, ((LiteralNode)results[0].Value("obj")).Value));
                    //returning
                    return ((LiteralNode)results[0].Value("obj")).Value;
                    
                }

            }
            else
            {
                //Add it to the HashSet in RAM
                LabelsAlreadyGot.Add(new KeyValuePair<string, string>(uri, ((LiteralNode)results[0].Value("obj")).Value));
                //returning it
                return ((LiteralNode)results[0].Value("obj")).Value;
            }
        }

        

        public void getData(List<string> Input_URIs, List<string> PredicatesToExclude)
        {
            

            //Generating filter string to add to the queries
            string excluded = "";
            if (PredicatesToExclude.Count > 0)
            {
                for (int i = 0; i < PredicatesToExclude.Count; i++)
                {
                    excluded += ". filter (?pred != <" + PredicatesToExclude[i] + ">)";
                }
            }


            //should be returned
            List<ResourceInformation> ResourceInformationList = new List<ResourceInformation>();
            foreach (string item in Input_URIs)
            {
                //constructing the queries, we had to do FROM<dbpedia.org> to prevent garbage data, should be faster too
                string querySide1 = "SELECT *  WHERE {<" + item + "> ?pred ?obj" + excluded + "}";
                string querySide2 = "SELECT *  WHERE {?subj ?pred <" + item + ">" + excluded + "}";
                               
                
                //doing both queries              
                SparqlResultSet res1 = new SparqlResultSet();
                SparqlResultSet res2 = new SparqlResultSet();
                //res1 = endpoint.QueryWithResultSet(querySide1);
                //res2 = endpoint.QueryWithResultSet(querySide2);

                res1 = Request.RequestWithHTTP(querySide1);
                res2 = Request.RequestWithHTTP(querySide2);



                //Initialize the resource
                ResourceInformation temp = new ResourceInformation();
                temp.predicates_resourceIsSubj = new List<KeyValuePair<string, string>>();
                temp.resources_resourceIsSubj = new List<KeyValuePair<string, string>>();
                temp.predicates_resourceIsObj = new List<KeyValuePair<string, string>>();
                temp.resources_resourceIsObj = new List<KeyValuePair<string, string>>();
                temp.rawComparisonObject = new List<KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>>();
                temp.FinalComparisonObject = new List<KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>>();
                temp.ID = new KeyValuePair<string, string>();

                //Filling all the information in case  <ourResourceID> ?x ?y
                foreach (SparqlResult item_res1 in res1)
                {             
                    //string tempo=getLabel(((LiteralNode)item_res1.Value("pred")).ToString());
                    if (String.Equals(((INode)(item_res1.Value("obj"))).GetType().Name.ToString(), "UriNode"))
                    {
                        temp.resources_resourceIsSubj.Add(new KeyValuePair<string, string>(item_res1.Value("obj").ToString(), util.getLabel(item_res1.Value("obj").ToString())));
                        
                    }
                    else
                    {
                        string onlyValue = ((LiteralNode)item_res1.Value("obj")).Value;
                        temp.resources_resourceIsSubj.Add(new KeyValuePair<string, string>(item_res1.Value("obj").ToString(), onlyValue));

                        //To fill out the ID component
                        if (String.Equals(item_res1.Value("pred").ToString(), "http://www.w3.org/2000/01/rdf-schema#label"))
                        {
                            temp.ID = new KeyValuePair<string, string>(item, temp.resources_resourceIsSubj[temp.resources_resourceIsSubj.Count - 1].Value);
                        }
                    }

                    temp.predicates_resourceIsSubj.Add(new KeyValuePair<string, string>(item_res1.Value("pred").ToString(), util.getLabel(item_res1.Value("pred").ToString())));                    

                }

                //Filling all the information in case ?x ?y <ourResourceID>
                foreach (SparqlResult item_res2 in res2)
                {
                    //We add of to the name, child of .....etc 
                    temp.predicates_resourceIsObj.Add(new KeyValuePair<string, string>(item_res2.Value("pred").ToString(), util.getLabel(item_res2.Value("pred").ToString()) + " of"));

                    if (String.Equals(((INode)(item_res2.Value("subj"))).GetType().Name.ToString(), "UriNode"))
                        temp.resources_resourceIsObj.Add(new KeyValuePair<string, string>(item_res2.Value("subj").ToString(), util.getLabel(item_res2.Value("subj").ToString())));
                    else
                    {
                        string onlyValue = ((LiteralNode)item_res2.Value("subj")).Value;
                        temp.resources_resourceIsSubj.Add(new KeyValuePair<string, string>(item_res2.Value("subj").ToString(), onlyValue));
                    }

                }
                
                //filling comparison component
                temp=fillComparisonComponent(temp);                

                //Addding the resource to the list of resourceInformation Objects
                ResourceInformationList.Add(temp);

                //Copying it to the globalVariable
                ComparisonOutput = ResourceInformationList;
                
            }
            //getting common between URIs
            //Console.WriteLine("getting Common");
            ResourceInformationList=getCommon(ResourceInformationList);

            
            //logging
            //logResults(ResourceInformationList);
            
        }


    

    }
}
