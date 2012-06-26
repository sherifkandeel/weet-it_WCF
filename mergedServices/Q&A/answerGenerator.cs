using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Query;
using VDS.RDF;

namespace mergedServices
{
    class answerGenerator
    {
        private string parsedQuestion;
        private string question;
        private int questionType;
        private Lexicon lexicon;
        

        /// <summary>
        /// class constructor
        /// </summary>
        public answerGenerator()
        {
            lexicon = new Lexicon();
        }

        /// <summary>
        /// takes the Question string and returns the List of the Queries to be executed 
        /// </summary>
        /// <param name="question">Question String</param>
        /// <returns>List of Generated Queries</returns>
        public List<QueryBucket> generateQueries(string question)
        {
            this.question = question.ToLower();
            parsedQuestion = this.question;
            
            // preprocessing of parsedQuestion
            QuestionPreprocessing();

            // compute the sparql for each bucket there are a list of queries 
           List<QueryBucket> queries = buildQueries();

           return queries; 
        }

        public List<questionAnswer> executeQueries(List<QueryBucket> queryBuckets)
        {
            List<questionAnswer> answers = new List<questionAnswer>();

            foreach (QueryBucket queryBucket in queryBuckets)
            {
                if (queryBucket.literalOnly == false)
                {
                    foreach (string queryString in queryBucket.BucketQueryList)
                    {
                        //SparqlResultSet resultSet = QueryHandler.ExecuteQueryWithString(queryString);
                        SparqlResultSet resultSet = Request.RequestWithHTTP(queryString);
                        if (resultSet.Count > 0)
                        {
                            questionAnswer answer = new questionAnswer(queryBucket.QuestionType);
                            answer.answerCount = resultSet.Results.Count();

                            foreach (LexiconToken token in queryBucket.tokens.Distinct<LexiconToken>())
                            {
                                if (token is LexiconPredicate)
                                {
                                    answer.predicateList.Add(token as LexiconPredicate);
                                    answer.predicateUriList.Add(token.URI, token.QuestionMatch);
                                    answer.predicateLabelList.Add(token.label, token.QuestionMatch);
                                }
                                else if (token is LexiconLiteral)
                                {
                                    answer.subjectList.Add(token as LexiconLiteral);
                                    answer.subjectUriList.Add(token.URI, token.QuestionMatch);
                                    answer.subjectLabelList.Add(token.label, token.QuestionMatch);
                                }
                            }

                            foreach (SparqlResult result in resultSet)
                            {
                                INode subjectNode = result[0];
                                INode objectNode = result[1];
                                Type resultType = objectNode.GetType();

                                if (answer.questiontype == util.questionTypes.literalOrURIAnswer)
                                {
                                    if (resultType.Name == "LiteralNode")
                                    {
                                        answer.questiontype = util.questionTypes.literalAnswer;
                                    }
                                    else if (resultType.Name == "UriNode")
                                    {
                                        answer.questiontype = util.questionTypes.URIAsnwer;
                                    }
                                }
                                answer.objectNodetList.Add(objectNode);
                                answer.objectUriList.Add(objectNode.ToString());
                                answer.objectLabelList.Add(util.getLabel(objectNode.ToString()));
                            }
                            answers.Add(answer);
                        }
                    }
                }
                else
                {
                    questionAnswer answer = new questionAnswer();
                    answer.objectUriList.Add(queryBucket.tokens[0].URI);
                    answer.objectLabelList.Add(queryBucket.tokens[0].label);
                    answer.answerCount = 1;
                    answer.questiontype = util.questionTypes.URIAsnwer;

                    answers.Add(answer);
                }
            }
            
            return answers; 
        }


        /// <summary>
        /// takes the parsed Question and Get predicates literals joins them
        /// generate the Query buckets filter the buckets and get for each bucket the list of 
        /// Query strings 
        /// </summary>
        /// <returns></returns>
        private List<QueryBucket> buildQueries()
        {
            List<string> questionAndType = util.GetQuestionType(parsedQuestion); //list of two elements, first is the question, second is the question type
            parsedQuestion = questionAndType[0];    //set the parsed question to the modified question string after knowing its type
            util.questionTypes questionType = util.mapQuestionType(questionAndType[1]);   //set the question type to the type read from the quetion types file

            //Get predicates and literals of the question
            List<LexiconPredicate> predicateList = lexicon.getPredicates(parsedQuestion); //find all matching predicates
            List<LexiconLiteral> literalList = lexicon.getLiterals(parsedQuestion);

            //create list of the queryBuckets
            List<QueryBucket> queryBuckets = new List<QueryBucket>();

            //create queryBucket of the whole question
            QueryBucket tmpBucket = new QueryBucket(parsedQuestion,questionType);

            queryBuckets.Add(tmpBucket);

            // first of all create an ArrayList containing as much QueryBuckets as there are predicates in our predicateList
            foreach (LexiconPredicate predicate in predicateList)
            {
                //create a new QueryBucket object to be added in the end of the for loop 
                tmpBucket = new QueryBucket(parsedQuestion,questionType);

                string[] tmpWords = parsedQuestion.Split(' ');

                // if any of the words in the Question matches the domain of the predicate , adding it to the Question match 
                foreach (string word in tmpWords)
                {
                    if (util.match(word, util.URIToSimpleString(predicate.domains)) && !predicate.QuestionMatch.Contains(word))
                    {
                        if (predicate.QuestionMatch.Length > 0) predicate.QuestionMatch += " ";
                        predicate.QuestionMatch += word;
                    }
                }


                // if any of the words in the Question matches the range of the predicate , adding it to the Question match 
                foreach (string word in tmpWords)
                {
                    if (util.match(word, util.URIToSimpleString(predicate.ranges)) && !predicate.QuestionMatch.Contains(word))
                    {
                        if (predicate.QuestionMatch.Length > 0) predicate.QuestionMatch += " ";
                        predicate.QuestionMatch += word;
                    }
                }

                //testing
                tmpBucket.add(predicate.getClone(predicate));
                //now add the bucket to the queryBuckets ArrayList
                queryBuckets.Add(tmpBucket);
            }



            /* 
             * and now for each QueryBucket search for all the 
             * combination possibilities among the predicates
            */
            /* will remove this part untill a second proposal design is implemented so our design supports only 1 predicate */
            #region merging with other predicates step 2
            util.log("search for combination possibilities among predicates");
            util.log("loop over " + queryBuckets.Count);

            //foreach (QueryBucket bucket in queryBuckets.ToList())
            //{
            //    bool somethingHappened = false;


            //    if (bucket.questionLeft.Length > 0)
            //    {
            //        // getting all the new predicates for the Question left 

            //        foreach (LexiconPredicate predicate in predicateList.ToList())
            //        {

            //            //only go further if the predicate is not already in tmpBucket
            //            if (!bucket.ContainsTokenWithSameURIs(predicate))
            //            {

            //                string[] tmpWords = bucket.questionLeft.Split(' ');

            //                foreach (string word in tmpWords)
            //                {
            //                    if ((util.match(word, util.URIToSimpleString(predicate.domains)) || util.match(word, util.URIToSimpleString(predicate.ranges))) && !predicate.QuestionMatch.Contains(word))
            //                    {
            //                        if (predicate.QuestionMatch.Length > 0) predicate.QuestionMatch += " ";
            //                        predicate.QuestionMatch += word;
            //                    }
            //                }

            //                //testing
            //                QueryBucket newBucket = new QueryBucket(bucket);
            //                somethingHappened = newBucket.add(predicate.getClone(predicate));
            //                if (somethingHappened) queryBuckets.Add(newBucket);

            //            }

            //        }
            //    }
            //}//end for
            #endregion



            //we have now query buckets that has the matched predicates and the part of the question left 
            //if we have a question: Who is the father of barack obama
            //after parsing the question, it would be: father of barack obama
            //after the .getPredicates(), it would be: Question left:barack obama, becasue fatehrof is a predicate
            //find combinations using literals , which could match any of the already made QueryBuckets 

            util.log(" search for combinations using literals among " + queryBuckets.Count + " Querybuckets ");

            foreach (QueryBucket bucket in queryBuckets.ToList())
            {
                bool somethingHappened = false;

                //Literals 
                if (bucket.questionLeft.Length > 0)
                {
                    literalList = lexicon.getLiterals(bucket.questionLeft); //find all matching literals

                    foreach (LexiconLiteral literal in literalList)
                    {

                        //if we only matched because of one single character ("a" for example) jump over this..
                        // useless in our example because we don't match 
                        if (literal.QuestionMatch.Length > 1)
                        { //jump over

                            //add TypeOfOwner to matchstring

                            string[] tmpWords = bucket.questionLeft.Split(' ');

                            //adding to the type of owner and the predicate used \
                            // not used in our case 
                            //foreach (string word in tmpWords)
                            //{
                            //    // some literals don't have type of owner .. it's set to ""  and all don't have a predicate . because we match " label " 
                            //    if ((util.match(util.URIToSimpleString(literal.typeOfOwner), word) || Util.match(tmpLit.getSimplePredicate(), word)) && !literal.QuestionMatch.Contains(word))
                            //    {
                            //        if (literal.QuestionMatch.Length > 0)
                            //            literal.QuestionMatch += " ";
                            //        literal.QuestionMatch +=  word;
                            //    }
                            //}


                            //add additional matchStrings 
                            // not used as well in our case 
                            //List<string> matchStrings = lexicon.getMatchStringsForLiteral(tmpLit);

                            //if (matchStrings != null)
                            //    foreach (string word in tmpWords)
                            //    {
                            //        foreach (string matchStringWord in matchStrings)
                            //        {
                            //            if (Util.match(matchStringWord, word) && !wordsUsed.Contains(word))
                            //            {
                            //                if (wordsUsed.Length > 0)
                            //                    wordsUsed = wordsUsed + " ";
                            //                wordsUsed = wordsUsed + word;
                            //            }
                            //        }
                            //    }

                            QueryBucket newBucket = new QueryBucket(bucket);

                            //if literal contained in the quesion left , add it in a new bucket 
                            // only suitable for one predicate and one literal Questions
                            if (newBucket.questionLeft.Contains(literal.QuestionMatch) || util.match(newBucket.questionLeft, literal.QuestionMatch)) // because it could be included with a space of something
                            {
                                somethingHappened = newBucket.add(literal.getClone(literal));
                                if (somethingHappened) queryBuckets.Add(newBucket);
                            }
                        }
                    }
                }
            }


            // till now query buckets generated containing predicates combined with other predicates  combined with literals 	
            //delete all occurences of ignoreStrings out of questionLeft string
            foreach (QueryBucket bucket in queryBuckets)
            {
                string questionLeft = bucket.questionLeft;
                List<string> ignoreStrings = util.getIgnoreStrings();
                foreach (string ignoreString in ignoreStrings)
                {
                    while (Regex.Match(questionLeft, ".*(\\s|^)" + ignoreString + "(\\s|$).*", RegexOptions.IgnoreCase).Length > 0)
                    {
                        questionLeft = Regex.Replace(questionLeft, "(\\s|^)" + ignoreString + "(\\s|$)", " ");
                        questionLeft = questionLeft.Replace("  ", " "); //delete double spaces
                        questionLeft = questionLeft.Trim();
                        bucket.questionLeft = questionLeft;
                    }
                }

                #region check exceptions
                //    bucket.QuestionLeft = questionLeft.Trim();

                //    if ((bucket.QuestionLeft.Trim().Length > 0) || (bucket.countToDo()/*Replaced by getter count*/ > 0))
                //    {
                //        //ok now check for our exceptions
                //        //exception: words left and those are contained in the string of the unresolved URIs
                //        bool isException = false;
                //        if ((bucket.QuestionLeft.Trim().Length > 0) && (bucket.countToDo()/*Replaced by getter count*/ > 0))
                //        {
                //            string tmpString = Util.stem(bucket.QuestionLeft.Trim().ToLower());
                //            string[] tmpWords = tmpString.Split(' '); allContained = true;
                //            foreach (string word in tmpWords)
                //            {
                //                if (!Util.stem(bucket.getToDoAsSimpleString().replaceAll("([A-Z])", " $1").toLowerCase()).contains(word))        //////////////////////////to be handled later
                //                    allContained = false;
                //            }
                //            if (allContained)
                //                isException = true;
                //        }

                //        //exception: unresolved URIs and the strings of those uri are contained in the parsedQuestion string
                //        if ((bucket.QuestionLeft.Trim().Length == 0) && (bucket.countToDo()/*Replaced by getter count*/ > 0))
                //        {
                //            string tmpString = bucket.getToDoAsSimpleString().replaceAll("([A-Z])", " $1").toLowerCase();    //////////////////////////to be handled later
                //            string[] tmpWords = tmpString.Trim().Split(' ');
                //            allContained = true;
                //            foreach (string word in tmpWords)
                //            {
                //                if (!Util.stem(parsedQuestion).Contains(Util.stem(word)))
                //                    allContained = false;
                //            }
                //            if (allContained)
                //                isException = true;

                //        }

                //        if (!isException)
                //        {
                //            queryBuckets.Remove(bucket);
                //            /*
                //            queryBuckets.Remove(i);
                //            i--;
                //             * */
                //        }

                //    }
                #endregion
            }

            List<QueryBucket> oldbuckets = new List<QueryBucket>(queryBuckets);

            queryBuckets = cleanBucket(queryBuckets);

            //// remove duplicates ie. if for any solution another one has the same content in the bucket remove that
            //foreach (QueryBucket bucket1 in queryBuckets)
            //{
            //    foreach (QueryBucket bucket2 in queryBuckets)
            //    {
            //        if (!bucket1.Equals(bucket2))
            //        {
            //            // not to comparing to itself
            //            if (bucket1.EqualSolution(bucket2))
            //            {
            //                queryBuckets.Remove(bucket1);
            //                break;
            //            }
            //        }
            //    }
            //}


            //and now build queries out of the buckets :-)
            foreach (QueryBucket bucket in queryBuckets)
            {
                if (bucket.questionLeft.Length > 4)
                    continue;
                util.log("-----------------------");
                util.log("QUESTION LEFT: " + bucket.questionLeft);
                //foreach (var item in bucket.tokens)
                //{
                //    //if (item.score ==0)
                //    {
                //        //util.log("\n\nSCORE:" + item.score.ToString());
                //        //util.log("\n\nQUESTION MATCH: " + item.QuestionMatch + "\t" + "URI USED: " + item.URI+"\tSCORE:" + item.score.ToString() 
                //        // + "\tLABEL: " + item.label);
                //    }
                //}
                bucket.GetQuery();
                //util.log(bucket.Query);
                //sparqlQueries.addQuery(Query, bucket.getScore())
            }

            // remove duplicates ie. if for any solution another one has the same content in the bucket remove that
            bool removeFlag;
            for (int i = 0; i < queryBuckets.Count; i++)
            {
                if (queryBuckets[i].BucketQueryList.Count != 0)
                {
                    for (int j = i + 1; j < queryBuckets.Count; j++)
                    {
                        if (queryBuckets[i].BucketQueryList.Count == queryBuckets[j].BucketQueryList.Count)
                        {
                            removeFlag = true;
                            for (int k = 0; k < queryBuckets[i].BucketQueryList.Count; k++)
                            {
                                if (!(queryBuckets[i].BucketQueryList[k].Equals(queryBuckets[j].BucketQueryList[k])))
                                {
                                    removeFlag = false;
                                    break;
                                }
                            }

                            if (removeFlag)
                            {
                                queryBuckets.Remove(queryBuckets[j]);
                                j--;
                            }
                        }
                    }
                }
            }

            //write queries to Log !For Testing!
            foreach (QueryBucket bucket in queryBuckets)
            {
                util.log("New Bucket----------------------------");

                foreach (string query in bucket.BucketQueryList)
                {
                    util.log(query);
                }
            }


            Console.WriteLine("DONE");

            return queryBuckets;

        }

        /// <summary>
        /// change in the parsed question and removes puncituation and stopping words 
        /// </summary>
        private void QuestionPreprocessing()
        {
            Regex tempRegex;
            string substr;
            bool tempBool;

            //remove possible question marks
            parsedQuestion = parsedQuestion.Replace("?", "");

            //remove possible dots
            parsedQuestion = parsedQuestion.Replace(".", "");

            //remove [ and ]
            parsedQuestion = parsedQuestion.Replace("[", "");
            parsedQuestion = parsedQuestion.Replace("]", "");

            //replace commas
            parsedQuestion = parsedQuestion.Replace(",", " ");

            //replace two spaces with one
            parsedQuestion = parsedQuestion.Replace("  ", " ");


            //starting and ending quotes
            while ((parsedQuestion.StartsWith("'") && parsedQuestion.EndsWith("'")) || (parsedQuestion.StartsWith("\"") && parsedQuestion.EndsWith("\"")))
                parsedQuestion = parsedQuestion.Substring(1, parsedQuestion.Length - 1);

            //mark names (words quoted), removes quotes and if the name contains spaces it replace it with underscores '_'
            tempRegex = new Regex(".*'(.*)'.*");
            tempBool = tempRegex.IsMatch(parsedQuestion);

            while (tempBool)
            {
                tempBool = false;
                substr = tempRegex.Match(parsedQuestion).Groups[1].ToString();
                parsedQuestion = parsedQuestion.Replace("'" + substr + "'", substr.Replace(" ", "_"));
                tempBool = tempRegex.IsMatch(parsedQuestion);
            }

            //remove more than one spaces
            while (parsedQuestion.Contains("  "))
                parsedQuestion = parsedQuestion.Replace("  ", " ");

            //////////Ambiguous part
            //tempRegex = new Regex("\\^'(.*)'\\$");
            //parsedQuestion = tempRegex.Replace(parsedQuestion, "$1");  //parsedQuestion = parsedQuestion.replaceAll("\\^'(.*)'\\$","$1");

            //todo :
            //try to find out the question type  
            //int[] tmpType = new int[1];
            //parsedQuestion = QuestionType.getQuestionType(parsedQuestion, tmpType);
            //questionType = tmpType[0];

            //todo : 
            //look for names and mark them with underscores instead of spaces
            //string testString = "";
            //string[] words = parsedQuestion.Split(' ');
            //for (int i = words.Length; i > 1; i--)
            //{
            //    for (int j = 0; j < (words.Length - i + 1); j++)
            //    {
            //        testString = "";
            //        for (int k = 0; k < i; k++)
            //        {
            //            if (testString.Length > 0) testString = testString + " ";
            //            testString = testString + words[k + j];
            //        }
            //        if (lexicon.isName(testString)) parsedQuestion = parsedQuestion.Replace(testString, testString.Replace(" ", "_")); /////isName need to be implemented (Lexicon method)
            //    }
            //}

            //todo : 
            //try to replace matchstrings with classes with the classname itself
            //parsedQuestion = lexicon.replaceMatchstringWithClassname(parsedQuestion);

            //todo : 
            //remove occurences of 'the' 
            //tempRegex = new Regex("([\\s|^])the([\\s|$])");
            //parsedQuestion = tempRegex.Replace(parsedQuestion, "");

            parsedQuestion = parsedQuestion.Trim();
        }

        /// removing the non used predicates domains and the literals type of owners 
        /// </summary>
        /// <param name="tokens">list </param>of tokens
        /// <returns>cleaned list of tokens </returns>
        private List<QueryBucket> cleanBucket(List<QueryBucket> queryBuckets)
        {
            #region removing Buckets which still have question left  >1

            foreach (QueryBucket querybucket in queryBuckets.ToList())
            {
                if (querybucket.questionLeft.Length > 0)
                {
                    queryBuckets.Remove(querybucket);
                }
            }

            #endregion

            #region remove Predicates domains and type of owners

            foreach (QueryBucket bucket in queryBuckets.ToList())
            {
                //adding predicates and literals to a list
                List<LexiconPredicate> predicateList = new List<LexiconPredicate>();
                List<LexiconLiteral> literalList = new List<LexiconLiteral>();
                foreach (LexiconToken token in bucket.tokens)
                {
                    if (token is LexiconPredicate)
                        predicateList.Add(token as LexiconPredicate);

                    if (token is LexiconLiteral)
                        literalList.Add(token as LexiconLiteral);
                }

                if (predicateList.Count > 0)
                {

                    //removing domains and ranges that are not used 
                    foreach (LexiconToken token in bucket.tokens.ToList())
                    {
                        if (token is LexiconPredicate)
                        {
                            //casting the lexicontoken to lexicon predicate
                            LexiconPredicate oldPredicate = token as LexiconPredicate;
                            //cloning the token to be modified 
                            LexiconPredicate predicateToReplace = (LexiconPredicate)token.getClone(token);

                            foreach (string oldPredDomain in oldPredicate.domains.ToList())
                            {
                                bool exist = false;
                                foreach (LexiconLiteral tmpliteral in literalList)
                                {
                                    if (tmpliteral.typeOfOwner.Contains(oldPredDomain))
                                        exist = true;
                                }

                                //if this domains doesn't contained in any of literals type of owners remove it as it wont match|join with anything
                                if (!exist)
                                {

                                    //old bucket = new bucket and then modify in the new in order then to be able to remove the old 
                                    predicateToReplace = oldPredicate.getClone(oldPredicate) as LexiconPredicate;

                                    //removing domain not used
                                    predicateToReplace.domains.Remove(oldPredDomain);
                                    //remove the old bucket and replace it with new modified one // needed because of reference issues
                                    bucket.tokens.Remove(oldPredicate);
                                    bucket.tokens.Add(predicateToReplace);

                                    oldPredicate = predicateToReplace;

                                    //remove the predicate if it doesnt have any domains left 
                                    if (oldPredicate.domains.Count == 0)
                                    {
                                        bucket.tokens.Remove(oldPredicate);
                                        predicateList.Remove(oldPredicate as LexiconPredicate);
                                        //remove the bucket if it's free from predicates
                                        if (bucket.tokens.Count == 0)
                                        {
                                            queryBuckets.Remove(bucket);
                                        }
                                    }
                                }
                            }
                        }

                        if (token is LexiconLiteral)
                        {
                            LexiconLiteral oldLiteral = token as LexiconLiteral;
                            LexiconLiteral newLiteral = token.getClone(token) as LexiconLiteral;

                            foreach (string typeofowner in oldLiteral.typeOfOwner.ToList())
                            {
                                bool exist = false;
                                foreach (LexiconPredicate tmmpredicate in predicateList)
                                {
                                    if (tmmpredicate.domains.Contains(typeofowner))
                                        exist = true;
                                }

                                if (!exist)
                                {

                                    //taking a copy from the old literal in order to remove it from the bucket when replacing it with the newliteral
                                    newLiteral = oldLiteral.getClone(oldLiteral) as LexiconLiteral;

                                    // removing typeofowner not used
                                    newLiteral.typeOfOwner.Remove(typeofowner);
                                    // updating the bucket tokens by replacing the old literal with the new one 
                                    bucket.tokens.Remove(oldLiteral);
                                    bucket.tokens.Add(newLiteral);

                                    oldLiteral = newLiteral;

                                    if (oldLiteral.typeOfOwner.Count == 0)
                                    {
                                        bucket.tokens.Remove(oldLiteral);
                                        literalList.Remove(oldLiteral as LexiconLiteral);
                                        //remove the bucket if it's free from Tokens
                                        if (bucket.tokens.Count == 0)
                                        {
                                            queryBuckets.Remove(bucket);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    bucket.literalOnly = true;
                }
            }

            #endregion

            #region remove the multiple domains and multiple ranges
            foreach (QueryBucket bucket in queryBuckets)
            {
                foreach (LexiconToken predicateToken in bucket.tokens)
                {
                    if (predicateToken is LexiconPredicate)
                    {
                        foreach (LexiconToken literalToken in bucket.tokens)
                        {
                            if (literalToken is LexiconLiteral && Enumerable.SequenceEqual((predicateToken as LexiconPredicate).domains, (literalToken as LexiconLiteral).typeOfOwner))
                            {
                                (predicateToken as LexiconPredicate).domains.RemoveRange(1, (predicateToken as LexiconPredicate).domains.Count - 1);
                                (literalToken as LexiconLiteral).typeOfOwner.RemoveRange(1, (literalToken as LexiconLiteral).typeOfOwner.Count - 1);
                            }
                        }
                    }
                }
            }
            #endregion

            return queryBuckets;
        }



    }
}
