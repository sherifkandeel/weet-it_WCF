using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace mergedServices
{
    class QueryBucket
    {
        //flag to check if bucket contains lexicon literal only
        public bool literalOnly = false;

        //tokens of the query bucket
        public List<LexiconToken> tokens;
        
        //question left of the bucket
        public string questionLeft ;

        //containing all URIs of a certain bucket
        private List<string> uriUsed;

        //containing all URIs not yet resolved!!!!!!
        private List<string> uriToDo; 

        //structure used in the questionMatch component
        public struct customMatchObject
        {
            public List<string> wordsUsed;
            public string word;
            public string separator;
        }

        //query string of this bucket
        string query;

        //list of queries for this bucket
        List<string> bucketQueryList = new List<string>();

        //question type, set to unkown as default value
        util.questionTypes questionType = util.questionTypes.unkown;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="question">the question of the query bucket</param>
        public QueryBucket(string question,util.questionTypes type)
        {
            tokens = new List<LexiconToken>();
            questionLeft = question;
            uriToDo = new List<string>();
            uriUsed = new List<string>();
            questionType = type;
        }

        /// <summary>
        /// a constructor taking another bucket to simple clone it here
        /// </summary>
        /// <param name="bucket">the query bucket to clone it here</param>
        public QueryBucket(QueryBucket bucket)
        {            
            this.tokens = bucket.tokens.ToList();
            this.questionLeft = bucket.questionLeft.ToString();
            this.uriUsed = new List<string>(bucket.uriUsed);
            this.uriToDo = new List<string>(bucket.uriToDo);
            this.questionType = bucket.questionType;
        }

        /// <summary>
        /// Checks if there is a token already exists in the consumed tokens list 
        /// that connects the same URIs (has same range and domain)
        /// </summary>
        /// <param name="token">the lexicon token to compare to</param>
        /// <returns>true or false</returns>
        public bool ContainsTokenWithSameURIs(LexiconToken token)
        {
            foreach (var instance in tokens)
            {
                if (token.URI.Equals(instance.URI))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if there is a token already exists in the consumed tokens list that has same identifier
        /// </summary>
        /// <param name="token">the token to compare to </param>
        /// <returns>true or false</returns>
        public bool ContainsTokenWithSameIdentifier(LexiconToken token)
        {
            //todo:
            //foreach (LexiconToken item in tokens)
            //{
            //    if (token.identifier.Equals(item.identifier))
            //        return true;
            //}
            //return false;

            return false;
        }


        //wait tell we see it with omar
        public bool ContainsQueryPart(string queryPart)
        {
            foreach (var item in tokens)
            {
                // if(item.
                return true;
            }
            return false;
        }


        /// <summary>
        /// adds a new token to the bucket
        /// </summary>
        /// <param name="token">the token to add to the bucket</param>
        /// <returns>if it was added successfully</returns>
        public bool add(LexiconToken token)
        {

            ///*If this token has domain and range(is a connector token or predicate) i.e. uriList size > 1, 
            // *check all URIs of that token are already present in any token in the tokens hashtable
            // *If already present, dont add (return false)*/
            ////if ((uriList.Count > 1) && (this.ContainsTokenWithSameURIs(token)))
            ////    return false;

           
          //check whether any URI of token is contained in the uriToDo vector 
          //but only if there is already something in the uriToDo vector or in the uriUsed hashtable
		 
            bool inToDo;
            bool inURIUsed;
            if ((uriToDo.Count > 0) || (uriUsed.Count > 0)) /////////////////To be checked again
            {
                inToDo = false;
                if (uriToDo.Contains(token.URI))
                    inToDo = true;

                inURIUsed = false;
                if (uriUsed.Contains(token.URI))
                    inURIUsed = true;

                if (!inToDo && !inURIUsed)
                    return false;
            }

           
            //will reduce the questionleft according to the questionmatch
            questionLeft = questionMatch(token.QuestionMatch);
            questionLeft = questionLeft.Trim();

            //adding the token to the token list
            tokens.Add(token);

            //if this token has an uri in it that is part of the uriToDo Vector,
            //remove it from the vector an move it into the uriUsed Hashtable,
            //else if the uri is already contained in the uriUsed Hashtable do nothing
            //else put it into the uriToDo Vector
            for (int i = 0; i < uriToDo.Count; i++)
            {
                if (uriToDo[i].Equals(token.URI))
                    uriToDo.Remove(uriToDo[i]);
                uriUsed.Add(token.URI);
            }

            bool virginURI = true;
            for (int i = 0; i < uriUsed.Count; i++)
            {
                if (uriUsed[i].Equals(token.URI))
                    virginURI = false;
            }
            if (!virginURI)
                uriToDo.Add(token.URI);
            //------------------------------------------------------------------
            return true;
        }

        public void GetQuery()
        {
            List<LexiconPredicate> predicateList = new List<LexiconPredicate>();
            List<LexiconLiteral> literalList = new List<LexiconLiteral>();

            List<string> queryList = new List<string>();

            List<string> tmpLiteralPart = new List<string>();
            
            foreach (LexiconToken token in tokens)
            {
                if (token is LexiconPredicate)
                    predicateList.Add(token as LexiconPredicate);

                if (token is LexiconLiteral)
                    literalList.Add(token as LexiconLiteral);
            }

            foreach (LexiconPredicate predicate in predicateList)
            {
                queryList.Add(predicate.BuildQueryPart());

                tmpLiteralPart.Add("");
            }

            //build literal part of query
            if (literalList.Count > 0)
            {
                foreach (LexiconLiteral tmpLiteral in literalList)
                {
                    for (int i = 0 ; i < queryList.Count ; i++)
                    {
                        foreach(string typeOfOwner in tmpLiteral.typeOfOwner)
                        {
                            if (queryList[i].Contains("?" + util.URIToSimpleString(typeOfOwner)) && !tmpLiteralPart[i].Contains("?" + util.URIToSimpleString(typeOfOwner)))
                            {
                                if (tmpLiteralPart[i].Length > 0)
                                    tmpLiteralPart[i] += " || ";

                                tmpLiteralPart[i] += "?" + util.URIToSimpleString(typeOfOwner) + " = <" + util.encodeURI(tmpLiteral.URI) + ">";
                            }
                        }
                    }
                }
            }

            //complete query form
            for (int i = 0; i < tmpLiteralPart.Count; i++)
            {
                if (tmpLiteralPart[i].Length > 0)
                {
                    queryList[i] += " filter(" + tmpLiteralPart[i] + ")";
                }

                bucketQueryList.Add("select distinct *" + " WHERE { " + queryList[i] + " }");
            }
        }

        ///// <summary>
        ///// Builds the sparql query for this bucket
        ///// </summary>
        ///// <returns>string of this bucket's query</returns>
        //public void GetQuery()
        //{

        //    string literalQuery = "";
        //    string predicateQuery = "";
        //    string query;

        //    /*
        //     *!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! postponed till discussing rating part
        //     * 
        //     //reset rating
        //     if (tokens.Count == 0) 
        //         rating[0] = 0; //set low rating for empty bucket
        //     else 
        //         rating[0] = 10000; //most of the rating changes decrease the rating.
        //     */

        //    foreach (LexiconToken value in tokens)
        //    {
        //        LexiconToken tmpToken = value;
        //        //string tmpWordsUsed = (string)value[1];

        //        if (tmpToken is LexiconLiteral)/*check if type LexiconLiteral*/
        //        {
        //            if ((literalQuery.Length > 0) && (literalQuery != null))
        //                literalQuery = literalQuery + " . ";
        //            literalQuery = literalQuery + tmpToken.BuildQueryPart();/*LexconToken method*/
        //        }
        //        else
        //        {
        //            if (predicateQuery.Length > 0)
        //                predicateQuery = predicateQuery + " . ";
        //            predicateQuery = predicateQuery + tmpToken.BuildQueryPart();	/*LexconToken method*/
        //        }
        //        /*   
        //         * Postponed till discussing rating
        //         * 
        //         * 
        //        double r = rating[0] * 0.9;
        //        rating[0] = new Long(Math.round(r)).intValue();
        //         * */
        //    }

        //    query = predicateQuery;

        //    if ((literalQuery.Length > 0) && (predicateQuery.Length > 0))
        //        query = query + " . ";
        //    query = query + literalQuery;

        //    query = "select distinct *" +
        //    " WHERE { " +
        //        query +
        //        " }";

        //    //set query string of the bucket to the built query
        //    this.query = query;
        //}



        #region helper_methods
        /// <summary>
        /// gets the permutations based on the "_" and ""
        /// </summary>
        /// <param name="questionLeft">the input questionLeft</param>
        /// <returns>returnning the list of customObject</returns>
        public List<customMatchObject> getPermutations(string questionLeft)
        {
            //Trimming the string 
            questionLeft = questionLeft.Trim();

            //Removing Extra spaces
            while (questionLeft.Contains("  "))
                questionLeft = questionLeft.Replace("  ", " ");

            //Creating the new list of objects that would be returned 
            List<customMatchObject> listObject = new List<customMatchObject>();
            
            //The list of string that holds all permutations of the input string
            List<string> toReturn = new List<string>();

            //parsing the input string
            List<string> input = new List<string>();
            input = questionLeft.Split(' ').ToList<string>();

            //holds the temporary constructed string
            string temp = ""; 
            string temponeWord = "";
            string tempUScore = "";

            //list holds the words used to generate a certain permutation
            List<string> wordsUsed = new List<string>();

            //The core algorithm to generate permutations
            for (int j = 1; j <= input.Count; j++)//Size of word
            {
                for (int k = 0; k < (input.Count - (j - 1)); k++) //offset
                {
                    for (int l = k; l < (j + k); l++)
                    {
                        temp += input[l] + " ";
                        temponeWord += input[l];
                        tempUScore += input[l] + "_";
                        wordsUsed.Add(input[l]);
                    }

                    //add the generated strigns to the list and return it
                    customMatchObject tempobj1 = new customMatchObject();
                    tempobj1.wordsUsed = wordsUsed.Distinct().ToList<string>(); ;
                    tempobj1.word = temp.Remove(temp.Length - 1);
                    tempobj1.separator = " ";
                    listObject.Add(tempobj1);

                    customMatchObject tempobj2 = new customMatchObject();
                    tempobj2.wordsUsed = wordsUsed.Distinct().ToList<string>(); ;
                    tempobj2.word = temponeWord;
                    tempobj2.separator = "";
                    listObject.Add(tempobj2);

                    customMatchObject tempobj3 = new customMatchObject();
                    tempobj3.wordsUsed = wordsUsed.Distinct().ToList<string>();
                    tempobj3.word = tempUScore.Remove(temp.Length - 1);
                    tempobj3.separator = "_";
                    listObject.Add(tempobj3);

                    //resetting the variables again
                    temp = "";
                    tempUScore = "";
                    temponeWord = "";
                    wordsUsed = new List<string>();
                }
            }

            
            return listObject;

        }

        /// <summary>
        /// Matches the question of the new token with the questionleft in the bucekt "_" and oneword is supported
        /// </summary>
        /// <param name="questionOfToken">the question of the token</param>
        /// <returns>the new quesitonLeft</returns>
        private string questionMatch(string input)
        {
            input = input.ToLower();
            questionLeft = questionLeft.ToLower();
            //list that will hold the permutaitons of the string
            List<customMatchObject> permutations = getPermutations(questionLeft);
            input = input.Trim();
            input = input.Insert(0, " ");
            input = input.Insert(input.Length, " ");

            //trimming the questionLeft
            questionLeft = questionLeft.Trim();
            questionLeft = questionLeft.Insert(0, " ");
            questionLeft = questionLeft.Insert(questionLeft.Length, " ");

            //looping through all permutations
            foreach (var item in permutations)
            {
                //if a word is found, it's removed from all the questionLeft variable also with it's components
                //ex: Mac book would remove Macbook, Mac, book from the questionleft
                if ((input.Contains(" " + (item.word) + " ")))                
                {
                    questionLeft = questionLeft.Replace(" " + item.word + " ", " ");                   

                    foreach (var wd in item.wordsUsed)
                    {
                        if (questionLeft.Contains(" " + wd + " "))
                            questionLeft = questionLeft.Replace(" " + wd + " ", " ");
                    }

                }

            }
            while (questionLeft.Contains("  "))
                questionLeft = questionLeft.Replace("  ", " ");
            questionLeft = questionLeft.Trim();

            return questionLeft;

        }
        
        #endregion 

        //OMAR'S PART

        //    public string GetQuery()
        //    {
        //        string literalQuery = "";
        //        string predicateQuery = "";

        //        //reset rating
        //        if (tokens.Count == 0) 
        //            rating[0] = 0; //set low rating for empty bucket
        //        else 
        //            rating[0] = 10000; //most of the rating changes decrease the rating.

        //        foreach (List<object> value in tokens.Values)
        //        {
        //            object /*LexiconToken*/ tmpToken = /*(LexiconToken casting)*/value[0];
        //            string tmpWordsUsed = (string)value[1];
        //            if (tmpToken.GetType() == /*LexiconLiteral.class*/)/*check if type LexiconLiteral*/
        //            {
        //                if (literalQuery.Length > 0) 
        //                    literalQuery = literalQuery + " . ";
        //                literalQuery = literalQuery + tmpToken.buildQueryPart(tmpWordsUsed, rating);/*LexconToken method*/		
        //            } 
        //            else 
        //            {
        //                if (predicateQuery.Length > 0) 
        //                    predicateQuery = predicateQuery + " . ";
        //                predicateQuery = predicateQuery + tmpToken.buildQueryPart(tmpWordsUsed, rating);	/*LexconToken method*/
        //            }
        //            /*   
        //             * Postpone till discussing rating
        //             * 
        //             * 
        //            double r = rating[0] * 0.9;
        //            rating[0] = new Long(Math.round(r)).intValue();
        //             * */
        //        }
        //        string query = predicateQuery;
        //        if ( (literalQuery.Length > 0) && (predicateQuery.Length > 0)) 
        //            query = query + " . ";
        //        query = query + literalQuery;

        //        query = "select distinct *" + 
        //        " WHERE { " +
        //            query + 
        //            " }";
        //        return query;
        //    }

        //    public bool EqualSolution(QueryBucket bucket)
        //    {
        //        if (this.tokens.Count != bucket.tokens.Count)
        //            return false;

        //        foreach (List<object> value in tokens.Values)
        //        {
        //            object /*LexiconToken*/ tmpToken = /*(LexiconToken casting)*/value[0];
        //            string tmpWordsUsed = (string)value[1];
        //            if (!bucket.ContainsQueryPart(tmpToken.buildQueryPart(tmpWordsUsed)/*LexiconToken method*/))
        //                return false;
        //        }

        //        return true;
        //    }

        //    #endregion

        //    //Setters and Getters
        //    #region


        //    public string QuestionLeft
        //    {
        //        get { return questionLeft; }
        //        set { questionLeft = value; }
        //    }

        //    public Dictionary<object, object> Tokens
        //    {
        //        get { return tokens; }
        //        set { tokens = value; }
        //    }
        //    #endregion
        //}

        public bool EqualSolution(QueryBucket bucket2)
        {
            return false; 
        }

#region setters and getters
        /// <summary>
        /// get query string
        /// </summary>
        public string Query
        {
            get { return query; }
        }

        /// <summary>
        /// get list of queries for this bucket
        /// </summary>
        public List<string> BucketQueryList
        {
            get { return bucketQueryList; }
        }

        /// <summary>
        /// get or set the question type to shoose the suitable query
        /// </summary>
        public util.questionTypes QuestionType
        {
            get { return questionType; }
            set { questionType = value; }
        }
#endregion
    }
}
