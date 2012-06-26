using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Query;


namespace mergedServices
{

    /// <summary>
    /// Lexicon Class which is used to act as a Triple store contains Literals and Predicates that match words in the Question 
    /// it has two main functions Getpredicates , GetLiterals  which returns the matching literals and predicates objects 
    /// </summary>
    class Lexicon
    {

        private List<LexiconPredicate> predicateList;
        private List<LexiconLiteral> literalList;
        private bool predicateFilled;
        private bool literalFilled;

        /// <summary>
        /// constructor of the Lexicon class
        /// </summary>
        public Lexicon()
        {
            predicateList = new List<LexiconPredicate>();
            literalList = new List<LexiconLiteral>();
            predicateFilled = false;
            literalFilled = false;
        }

        /// <summary>
        /// Get all possible stems for each word in the string
        /// </summary>
        /// <param name="question">String (question) to find its words stems</param>
        /// <returns>Dictionary of each word and its stems</returns>
        public Dictionary<string, List<string>> GetStemmedWords(string question)
        {
            //Dictionary to hold word and a list of its stemmed words.
            Dictionary<string, List<string>> stemmedWords = new Dictionary<string, List<string>>();

            //Create Language class object to use its stemming method
            Language Stemmer = new Language();

            //Temp list of strings to hold intermediate results
            List<string> tempStemmedWords;

            //Trimming the string 
            question = question.Trim();
            //Removing Extra spaces
            while (question.Contains("  "))
                question = question.Replace("  ", " ");

            //parsing the input string
            List<string> input = new List<string>();
            input = question.Split(' ').ToList<string>();

            foreach (string word in input)
            {
                tempStemmedWords = Stemmer.getStems(word);

                if(tempStemmedWords.Contains(word))
                {
                    tempStemmedWords.Remove(word);
                }

                if (tempStemmedWords.Count > 0)
                {
                    stemmedWords.Add(word, tempStemmedWords);
                }
            }

            return stemmedWords;
        }

        /// <summary>
        /// get predicates is a method in lexicon class that get all predicates objects that match some words in the Question 
        /// </summary>
        /// <param name="question">question to get matched predicates of it </param>
        /// <param name="topN">the number of top matching results to be returned, default = 10</param>
        /// <param name="Limit">the limit of the number of returned results in the query, default = 20</param>
        /// <returns>list of top matching LexiconPredicates</returns>
        public List<LexiconPredicate> getPredicates(string question, int topN = 20, int Limit = 30)
        {
            DateTime dt = DateTime.Now;  // capturing time for testing 

            List<LexiconPredicate> __predicateList = new List<LexiconPredicate>();

            //getting all permutation of words formed from the question string
            List<string> permutationList = getPermutations(question);

            //removing permutations that most propbably wont return results and will take time in querying 
            permutationList = trimPermutations(permutationList);

            //Get the stemmed version of the question words
            Dictionary<string, List<string>> stemmedWords = GetStemmedWords(question);

            // to check if the predicates are filled before - so returning the matching predicates only - or not
            if (predicateFilled)
            {
                foreach (LexiconPredicate predicate in predicateList)
                {
                    if (permutationList.Contains(predicate.QuestionMatch))
                    {
                        __predicateList.Add(predicate);
                    }
                }
                return __predicateList;
            }

            else
            {
                string bifContainsValue = "";

                // iterating over each permutation of Question left and Query them from virtuoso and return predicate list and add them
                foreach (string questionleft in permutationList)
                {
                    //Get all forms of questionLeft by replacing words with its stemmed version
                    bifContainsValue = "";  //empty string

                    bifContainsValue +="\'" + questionleft + "\'";  //add the original questionleft

                    //Replace words in questionleft with its stem and add it to the bifContainsValue
                    foreach (string word in stemmedWords.Keys)
                    {
                        if (questionleft.Contains(word))
                        {
                            foreach (string stem in stemmedWords[word]) //This is created because a wordcan has many stems (rare case)
                            {
                                bifContainsValue += "or\'" + questionleft.Replace(word, stem) + "\'";
                            }
                        }
                    }


                    string Query = "SELECT  * WHERE { { " +
                                    "?predicate <http://www.w3.org/2000/01/rdf-schema#label> ?label ." +
                                    "?predicate <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#DatatypeProperty>." +
                                    "?label bif:contains \"" + bifContainsValue + "\" } " +
                                    "union {" +
                                    "?predicate <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#ObjectProperty> ." +
                                    "?predicate <http://www.w3.org/2000/01/rdf-schema#label> ?label ." +
                                   "?label bif:contains \"" + bifContainsValue + "\" } " +
                                    "union {" +
                                    "?predicate <http://www.w3.org/1999/02/22-rdf-syntax-ns#type>  <http://www.w3.org/1999/02/22-rdf-syntax-ns#Property>  ." +
                                    "?predicate <http://www.w3.org/2000/01/rdf-schema#label> ?label ." +
                                   "?label bif:contains \"" + bifContainsValue + "\" } " +

                                    "} limit " + Limit;


                    //another Query to Get predicates untill deciding which of them is the best using statistics 
                    string Query2 = "SELECT  ?predicate ?label WHERE {  " +
                                    "{ ?predicate <http://www.w3.org/2000/01/rdf-schema#label> ?label . " +
                                     "?predicate <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?propertyType. " +
                                     "?label bif:contains \"" + bifContainsValue + "\" } " +

                                    "filter ( ?propertyType = <http://www.w3.org/2002/07/owl#DatatypeProperty> || " +
                                    "?propertyType = <http://www.w3.org/2002/07/owl#ObjectProperty> || " +
                                    "?propertyType = <http://www.w3.org/1999/02/22-rdf-syntax-ns#Property>  ) " +
                                    "} limit " + Limit;

                    //SparqlRemoteEndpoint remoteEndPoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql"));

                    try
                    {
                        //executing the Query and finding results
                        //SparqlResultSet resultSet = remoteEndPoint.QueryWithResultSet(Query);
                        SparqlResultSet resultSet = Request.RequestWithHTTP(Query);

                        //iterating over matched predicates in the resultset
                        foreach (SparqlResult result in resultSet)
                        {
                            INode predicateURI = result.Value("predicate");
                            INode predicateLabel = result.Value("label");
                            LexiconPredicate tmplexiconpredicate = new LexiconPredicate();

                            // check that the property is used .. not a non-used property 
                            bool hasResuts = false;
                            string checkQuery = "select distinct * where { ?x <" + predicateURI + "> ?y } limit 1 ";
                            //QueryHandler.startConnection();
                            //SparqlResultSet checkResults = QueryHandler.ExecuteQueryWithString(checkQuery);
                            //QueryHandler.closeConnection();

                            SparqlResultSet checkResults = Request.RequestWithHTTP(checkQuery);

                            if (checkResults.Count != 0)
                            {
                                hasResuts = true;
                            }

                            // check that the predicate doesn't exists in the predicateslist before 
                            bool exists = false;
                            foreach (LexiconPredicate x in __predicateList)
                            {
                                // we added Questionmatch == question left bec new predicates may be added with better score that the old ones so this should be considered
                                if (x.URI == predicateURI.ToString() && x.QuestionMatch == questionleft ) 
                                {
                                    exists = true;
                                    break;
                                }
                            }
                            
                            // adding the new predicate to the __predicatelist 
                            if (!exists && hasResuts)
                            {
                                tmplexiconpredicate.URI = predicateURI.ToString();
                                tmplexiconpredicate.QuestionMatch = questionleft;
                                tmplexiconpredicate.label = predicateLabel.ToString();
                                __predicateList.Add(tmplexiconpredicate);
                            }
                        }


                    }

                    // skipping results that raised timeout exceptions
                    catch
                    {
                        util.log("skipped : " + questionleft + " ---- due to time out ");
                    }
                }

                util.log(" finished getting " + __predicateList.Count + " predicates " + " Time taken : " + DateTime.Now.Subtract(dt).TotalMilliseconds + " msec");

                // now done of collecting predicates scoring them down and get the best n ones 
                this.predicateList = scorePredicates(__predicateList, topN);
                this.predicateList = addDomainAndRange(this.predicateList);

                util.log("total time taken :" + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString() + " mSecs");

                predicateFilled = true;
                return this.predicateList;
            }
        }

        /// <summary>
        /// get predicates is a method in lexicon class that get all LexiconLiterals objects that match some words in the Question 
        /// </summary>
        /// <param name="question">question to get matched predicates of it </param>
        /// <param name="topN">the number of top matching results to be returned, default = 10</param>
        /// <param name="Limit">the limit of the number of returned results in the query, default = 20</param>
        /// <returns>list of top matching LexiconLiterals with it's type of owner and predicate </returns>
        public List<LexiconLiteral> getLiterals(string question, int topN = 30, int Limit = 30)
        {
            DateTime dt = DateTime.Now;  // capturing time for testing 
            List<LexiconLiteral> __literalList = new List<LexiconLiteral>();

            //getting all permutation of words formed from the question string
            List<string> permutationList = getPermutations(question);

            //removing permutations that most propbably wont return results and will take time in querying 
            permutationList = trimPermutations(permutationList);

            // to check if the literals are filled before - so returning the matching Literals only or not
            if (literalFilled)
            {
                foreach (LexiconLiteral literal in this.literalList)
                {
                    if (permutationList.Contains(literal.QuestionMatch))
                    {
                        __literalList.Add(literal);
                    }
                }

                return __literalList;
            }

            else
            {
                // iterating over each permutation of Question left and Query them from virtuoso and return predicate list and add them 
                foreach (string questionleft in permutationList)
                {
                    string Query = "select distinct ?subject ?literal ?redirects ?typeOfOwner ?redirectsTypeOfOwner where{" +
                                    "?subject <http://www.w3.org/2000/01/rdf-schema#label> ?literal." +
                                    "optional { ?subject <http://dbpedia.org/ontology/wikiPageRedirects> ?redirects . " +
                                    "optional {?redirects <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?redirectsTypeOfOwner ." +
                                    "}}." +
                                    "optional {?subject <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?typeOfOwner}." +
                                    "Filter ( !bound(?typeOfOwner) || " +
                                    " ( !(?typeOfOwner = <http://www.w3.org/2004/02/skos/core#Concept>)" +
                                    " && !(?typeOfOwner = <http://www.w3.org/2002/07/owl#Thing>) " +
                                    " && !(?typeOfOwner = <http://www.opengis.net/gml/_Feature>) " +
                                    " && !(?typeOfOwner = <http://www.w3.org/2002/07/owl#ObjectProperty>) " +
                                    " && !(?typeOfOwner = <http://www.w3.org/1999/02/22-rdf-syntax-ns#Property> ) " +
                                    " && !(?typeOfOwner = <http://www.w3.org/2002/07/owl#DatatypeProperty> )))." +
                                    "?literal bif:contains '\"" + questionleft + "\"'.} limit " + Limit;


                    //SparqlRemoteEndpoint remoteEndPoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql"));

                    SparqlResultSet resultSet = new SparqlResultSet();

                    try
                    {
                        //resultSet = remoteEndPoint.QueryWithResultSet(Query);
                        resultSet = Request.RequestWithHTTP(Query);
                    }
                    // skipping results that raised timeout exceptions
                    catch
                    {
                        util.log("skipped  Query3: " + questionleft + " ---- due to time out ");
                    }

                    //iterating over matched Literals in the resultset
                    foreach (SparqlResult result in resultSet)
                    {
                        string resultTypeOfOwner = "";
                        string resultURI;
                        string resultLabel = result.Value("literal").ToString();
                        string resultquestionMatch = questionleft;

                        if (result.Value("redirects") != null)
                        {
                            resultURI = result.Value("redirects").ToString();
                            if (result.Value("redirectsTypeOfOwner") != null)
                            {
                                resultTypeOfOwner = result.Value("redirectsTypeOfOwner").ToString();
                            }
                        }
                        else
                        {
                            resultURI = result.Value("subject").ToString();
                            if (result.Value("typeOfOwner") != null)
                            {
                                resultTypeOfOwner = result.Value("typeOfOwner").ToString();
                            }

                        }


                        // check that the predicate doesn't exists in the predicateslist before 
                        bool exists = false;          // URI + Label only Exists
                        bool totallyExists = false;   // URI + Label + TypeofOwner exists in the literal list 

                        foreach (LexiconLiteral x in __literalList)
                        {
                            if (x.URI == resultURI && x.label == resultLabel && x.QuestionMatch == resultquestionMatch)
                            {
                                exists = true;
                                if (x.typeOfOwner.Contains(resultTypeOfOwner) && resultTypeOfOwner.Length > 0)
                                {
                                    totallyExists = true;
                                    break;
                                }

                            }
                        }

                        // adding the new literals to the literallist.
                        if (!exists)
                        {
                            LexiconLiteral tmpLexiconLiteral = new LexiconLiteral(resultURI, resultLabel, resultquestionMatch, resultTypeOfOwner);
                            __literalList.Add(tmpLexiconLiteral);
                        }

                        if (!totallyExists && exists)
                        {
                            foreach (LexiconLiteral let in __literalList)
                            {
                                if (let.URI == resultURI && let.label == resultLabel)
                                {
                                    let.typeOfOwner.Add(resultTypeOfOwner);
                                }
                            }
                        }
                    }

                }


                //scoring literals . trimming duplicates , 
                __literalList = scoreLiterals(__literalList, topN);

                util.log("total time taken :" + DateTime.Now.Subtract(dt).TotalMilliseconds.ToString() + " msecs ");


                literalFilled = true;
                this.literalList = __literalList;
                return __literalList;
            }

        }

        /// <summary>
        /// scoring predicates calculating the Distance between them and the matching words 
        /// then returning the n ones who have the least distance  
        /// </summary>
        /// <param name="results">list of predicates found</param>
        /// <param name="n">number of results needed to be returned</param>
        /// <returns> the top n ones who have the least distance </returns>
        private List<LexiconPredicate> scorePredicates(List<LexiconPredicate> results, int n)
        {

            foreach (LexiconPredicate predicate in results)
            {
                // adding a levenshtein score to each one of them where predicates of high score will make a bad match
                // removing the @en in the end of each label 
                // removing the terms between brackets like  the dark knight  (the film)
                string tmplabel;

                //use match instead regex 
                if (predicate.label.EndsWith("@en") || Regex.IsMatch(predicate.label, @"\(.*\)"))
                {
                    tmplabel = predicate.label.Remove(predicate.label.Length - 3);
                    if (Regex.IsMatch(predicate.label, @"\(.*\)"))
                    {
                        Match match = Regex.Match(predicate.label, @"\(.*\)", RegexOptions.IgnoreCase);
                        tmplabel = tmplabel.Remove(match.Index, match.Length);
                        tmplabel = tmplabel.Replace("  ", " ");
                        tmplabel = tmplabel.Trim();
                    }
                }
                else
                {
                    tmplabel = predicate.label;
                }

                // sending Questionmatch and label to find the levenshtein distance between them 
                predicate.score = util.computeLevenshteinDistance(predicate.QuestionMatch, tmplabel);
            }

            foreach (LexiconPredicate predicate in results.ToList())
            {
                foreach (LexiconPredicate predicate2 in results.ToList())
                {
                    // if the URI exists before 
                    if (predicate.URI == predicate2.URI && !predicate.Equals(predicate2))
                    {
                        // removing the one of the larger distance 
                        results.Remove((predicate.score >= predicate2.score) ? predicate : predicate2);
                    }
                }
            }

            results.Sort(delegate(LexiconPredicate s1, LexiconPredicate s2) { return s1.score.CompareTo(s2.score); });

            if (results.Count < n) { n = results.Count; };

            foreach (LexiconPredicate result in results.GetRange(0, n))
            {
                Console.WriteLine(result.URI + "\t" + result.label + "\t" + result.QuestionMatch + "\t" + result.score);
            }

            return results.GetRange(0, n);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="results"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private List<LexiconLiteral> scoreLiterals(List<LexiconLiteral> results, int n)
        {
            // adding scores to all literals 
            foreach (LexiconLiteral x in results)
            {
                string tmplabel;
                //use match instead regex 
                if (x.label.EndsWith("@en") || Regex.IsMatch(x.label, @"\(.*\)"))
                {
                    tmplabel = x.label.Remove(x.label.Length - 3);
                    if (Regex.IsMatch(x.label, @"\(.*\)"))
                    {
                        Match match = Regex.Match(x.label, @"\(.*\)", RegexOptions.IgnoreCase);
                        tmplabel = tmplabel.Remove(match.Index, match.Length);
                        tmplabel = tmplabel.Replace("  ", " ");
                        tmplabel = tmplabel.Trim();
                    }
                }
                else
                {
                    tmplabel = x.label;
                }

                x.score = util.computeLevenshteinDistance(x.QuestionMatch, tmplabel);
            }

            //removing duplicates that have the same resource
            foreach (LexiconLiteral x in results.ToList())
            {
                foreach (LexiconLiteral y in results.ToList())
                {
                    // if the URI exists before 
                    if (x.URI == y.URI && !x.Equals(y))
                    {
                        // removing the one of the larger distance 
                        results.Remove((x.score >= y.score) ? x : y);
                    }
                }
            }


            // results.Distinct(delegate(LexiconLiteral l1, LexiconLiteral l2) { return (l1.score >= l2.score) ? l2 : l1 }); 


            // sorting the literals depending on the score 
            results.Sort(delegate(LexiconLiteral s1, LexiconLiteral s2) { return s1.score.CompareTo(s2.score); });

            if (results.Count < n)
            {
                n = results.Count;
            }

            return results.GetRange(0, n);

        }

        /// <summary>
        /// takes the list of Predicates and add tothem the domain and Range data needed  in two steps 
        /// 1st : searching for direct Domain and ranges 
        /// 2nd : searching for predicates who don't have domain and ranges and select the all predicates and objects 
        /// get their types and add them to the domain and range field 
        /// </summary>
        /// <param name="predicateList">the predicate list without the domain and range data</param>
        /// <returns>the predicate list with the predicates have domain and ranges</returns>
        private List<LexiconPredicate> addDomainAndRange(List<LexiconPredicate> predicateList)
        {
            //now interating over the final predicate list and fill the rest of it's details <Domain , Range>  
            // step 1 : the Direct Way 
            foreach (LexiconPredicate x in predicateList)
            {
                string Query = "Select distinct ?domain ?range where { {" +

                    "<" + x.URI + ">" + "<http://www.w3.org/2000/01/rdf-schema#domain> ?domain.}" +
                    "union { <" + x.URI + ">" + " <http://www.w3.org/2000/01/rdf-schema#range> ?range ." +
                    "}}";

                //QueryHandler.startConnection();
                //SparqlResultSet resulSet = QueryHandler.ExecuteQueryWithString(Query);
                //QueryHandler.closeConnection();
                SparqlResultSet resulSet = Request.RequestWithHTTP(Query);

                if (resulSet.Count() != 0)
                {
                    foreach (SparqlResult result in resulSet)
                    {
                        // check that the domain field is not empty  // and check that this domain wasn't added before 
                        if (result.Value("domain") != null)
                            if (!x.domains.Contains(result.Value("domain").ToString()))
                            {
                                x.domains.Add(result.Value("domain").ToString());
                            }

                        // check that the range field is not empty  // and check that this range wasn't added before 
                        if (result.Value("range") != null)
                            if (!x.ranges.Contains(result.Value("range").ToString()) && result.Value("range") != null)
                            {
                                x.ranges.Add(result.Value("range").ToString());
                            }
                    }
                }
            }


            //step2 : the inDirect Way -- for predicates that didn't have a Domain or range selected before 
            foreach (LexiconPredicate x in predicateList)
            {

                bool hasDomain = (x.domains.Count == 0) ? false : true;
                bool hasRange = (x.ranges.Count == 0) ? false : true;

                if (!hasDomain && !hasRange)
                {
                    continue;
                }

                string Query = "Select distinct ";
                Query += (hasDomain) ? "" : "?domain";
                Query += (hasRange) ? "" : "?range";
                Query += " where {{";

                if (!hasDomain)
                {
                    Query += "{ ?X <" + x.URI + "> ?Y ." +
                             "?X a ?domain . " +
                             "filter(!REGEX(STR(?domain) ,'http://www.w3.org/2002/07/owl#Thing','i'))" +
                             "}";
                }

                Query += "}";

                if (!hasRange)
                {
                    Query += "union { ?X <" + x.URI + "> ?Y ." +
                             "?Y a ?range . " +
                             "filter(!REGEX(STR(?range) ,'http://www.w3.org/2002/07/owl#Thing','i'))" +
                             "}";
                }

                Query += "}";

                Query += "limit 20 ";




                //QueryHandler.startConnection();
                //SparqlResultSet resulSet = QueryHandler.ExecuteQueryWithString(Query);
                //QueryHandler.closeConnection();
                SparqlResultSet resulSet = Request.RequestWithHTTP(Query);

                if (resulSet.Count() != 0)
                {
                    foreach (SparqlResult result in resulSet)
                    {
                        // check that the domain field is not empty  // and check that this domain wasn't added before 
                        if (!hasDomain)
                        {
                            if (result.Value("domain") != null)
                                if (!x.domains.Contains(result.Value("domain").ToString()))
                                {
                                    x.domains.Add(result.Value("domain").ToString());
                                }
                        }
                        // check that the range field is not empty  // and check that this range wasn't added before 
                        if (!hasRange)
                        {
                            if (result.Value("range") != null)
                                if (!x.ranges.Contains(result.Value("range").ToString()) && result.Value("range") != null)
                                {
                                    x.ranges.Add(result.Value("range").ToString());
                                }
                        }
                    }
                }
            }

            return predicateList;

        }

        #region helper functions

        /// <summary>
        /// This function returns all permutations of a string
        /// ex: father of barack obama
        /// 1st iteration: father, of, barack, obama
        /// 2nd iteration: father of, of barack, barack obama
        /// 3rd iteration: father of barack, of barack obama
        /// 4th iteration: father of barack obama
        /// </summary>
        /// <param name="questionLeft">The certain string to enter</param>
        /// <returns></returns>
        public List<string> getPermutations(string questionLeft)
        {
            //Trimming the string 
            questionLeft = questionLeft.Trim();
            //Removing Extra spaces
            while (questionLeft.Contains("  "))
                questionLeft = questionLeft.Replace("  ", " ");


            //The list of string that holds all permutations of the input string
            List<string> toReturn = new List<string>();

            //parsing the input string
            List<string> input = new List<string>();
            input = questionLeft.Split(' ').ToList<string>();

            string temp = ""; //holds the temporary constructed string
            string temponeWord = "";
            //string tempUScore = "";
            //The core algorithm to generate permutations
            for (int j = 1; j <= input.Count; j++)//Size of word
            {
                for (int k = 0; k < (input.Count - (j - 1)); k++) //offset
                {
                    for (int l = k; l < (j + k); l++)
                    {
                        temp += input[l] + " ";
                        temponeWord += input[l];
                        //  tempUScore += input[l] + "_";

                    }

                    //Adding to output and clearing the temp
                    toReturn.Add(temp.Remove(temp.Length - 1));
                    toReturn.Add(temponeWord);
                    // toReturn.Add(tempUScore.Remove(temp.Length - 1));
                    temp = "";
                    //tempUScore = "";
                    temponeWord = "";
                }
            }

            //Clearing the duplicates
            toReturn = toReturn.Distinct().ToList<string>();

            return toReturn;

        }

        /// <summary>
        /// after getting all permutations this function is to remove 
        /// permutations that may take time during the Query and return no results
        /// </summary>
        /// <param name="m">List of permutations</param>
        /// <returns>List of permuations after trimming </returns>
        private static List<string> trimPermutations(List<string> m)
        {
            foreach (string x in m.ToList<string>())
            {
                Match match = Regex.Match(x, @"(^the$)|(^and$)|(^of$)|(^that$)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    m.Remove(x);
                }

            }
            return m;

        }
        #endregion

    }

}


