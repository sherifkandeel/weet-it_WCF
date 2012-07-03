using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using VDS.RDF.Query;
using VDS.RDF;
using System.Web;


namespace mergedServices
{
    public static class util
    {
        public enum questionTypes
        {
            literalOrURIAnswer,
            literalAnswer,
            URIAsnwer,
            countAnswer,
            unkown
        };

        private readonly static string reservedCharacters = "!*'();@&=+$,?%[]";    //charactes to be encoded in the url encoding

        const float minMatchDistance = 0.6f;
        /// <summary>
        /// log text to the log file 
        /// </summary>
        /// <param name="s">string to be logged in the Logfile</param>
        public static void log(string s)
        {

            StreamWriter logWriter = File.AppendText(@".\Log.txt");
            logWriter.Write(s + "\t" + DateTime.Now.ToLongTimeString().ToString() + "\n");
            logWriter.Close();
        }
        /// <summary>
        /// empty the log file 
        /// </summary>
        public static void clearLog()
        {
            FileStream fileStream = File.Open(@".\Log.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fileStream.SetLength(0);
            fileStream.Flush();
            fileStream.Close();

        }

        /// <summary>
        /// Encodes URIs
        /// </summary>
        /// <param name="value">the input url to be encoded</param>
        /// <returns>encoded url</returns>
        public static string UrlEncode(string value)
        {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            var sb = new StringBuilder();

            foreach (char @char in value)
            {
                if (reservedCharacters.IndexOf(@char) == -1)
                    sb.Append(@char);
                else
                    sb.AppendFormat("%{0:X2}", (int)@char);
            }
            return sb.ToString();
        }


        /// <summary>
        /// compute the levenstein Distance between two strings 
        /// </summary>
        /// <param name="s">string1 </param>
        /// <param name="t">string2</param>
        /// <returns>the distance in integer</returns>
        public static int computeLevenshteinDistance(string s, string t)
        {
            s = s.ToLower();
            t = t.ToLower();
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

        /// <summary>
        /// returns a simple string of the URI by taking the last part of it , or the part after # if exists 
        /// </summary>
        /// <param name="URI">the URI</param>
        /// <returns>simple string represents the URI</returns>
        public static string URIToSimpleString(string URI)
        {
            if (URI.Contains("#"))
            {
                return URI.Split('#')[1];
            }
            else
            {
                return Regex.Match(URI, @"[^/]*$", RegexOptions.IgnoreCase).Groups[0].Value;
            }
        }
        /// <summary>
        /// convert list of URIs to simple string list  
        /// </summary>
        /// <param name="URIs">list of URIs </param>
        /// <returns>list of simple strings </returns>
        public static List<string> URIToSimpleString(List<string> URIs)
        {
            List<string> toreturn = new List<string>();
            foreach (string URI in URIs)
            {
                if (URI.Contains("#"))
                {
                    toreturn.Add(URI.Split('#')[1]);
                }
                else
                {
                    toreturn.Add(Regex.Match(URI, @"[^/]*$", RegexOptions.IgnoreCase).Groups[0].Value);
                }

            }

            return toreturn;
        }

        /// <summary>
        /// return true if s matches 40% - or min match distance - of s2 otherwise return false  
        /// </summary>
        /// <param name="s">string to be matched</param>
        /// <param name="s2">string to be matched with </param>
        /// <returns>true if matches min match distance and false otherwise</returns>
        public static bool match(string s, string s2)
        {
            int distance = util.computeLevenshteinDistance(s, s2);
            float matching = s2.Length - distance;
            float matchPercent = (float)matching / s2.Length;

            if (matchPercent >= minMatchDistance) return true;
            else return false;
        }


        /// <summary>
        /// returns true if match s matches 40% of any of the strings in s2 
        /// //todo :  
        /// </summary>
        /// <param name="s">string that used to check if it matches or not</param>
        /// <param name="s2">list of strings </param>
        /// <returns>matches or not </returns>
        public static bool match(string s, List<string> s2)
        {
            foreach (string x in s2)
            {
                if (match(s, x))
                {
                    return true;
                }
            }
            return false;
        }

        //private static HashSet<KeyValuePair<string, string>> gottenlabels = new HashSet<KeyValuePair<string, string>>();
        private static Dictionary<string, string> gottenlabels = new Dictionary<string, string>();

        /// <summary>
        /// this method adds the new labels from queries to the dataset
        /// </summary>
        /// <param name="URI">uri to add</param>
        /// <param name="label">label of the URI</param>
        private static void addtohashset(string URI, string label)
        {
            if (gottenlabels.Count > 300000)
            {
                try
                {
                    gottenlabels.ToList().RemoveRange(0, 1000);
                }
                catch
                {
                    gottenlabels = new Dictionary<string, string>();
                }

            }
            else
            {

                try
                {
                    if (!gottenlabels.ContainsKey(URI))
                        gottenlabels.Add(URI, label);
                    //gottenlabels.Add(URI, label);
                }
                catch
                {
                }
            }
        }

        //static value to load only at the beginning, containing the uris
        public static List<string> allowedURIs = loadAllowedLabels();

        /// <summary>
        /// loading the allowed uris of our database
        /// </summary>
        /// <returns>list of strings containing the allowerd uris</returns>
        public static List<string> loadAllowedLabels()
        {
            List<string> toreturn = new List<string>();
            StreamReader sr = new StreamReader("URIsToGetLabelFor.txt");
            while (!sr.EndOfStream)
            {
                toreturn.Add(sr.ReadLine());
            }
            sr.Close();
            return toreturn;
        }

        /// <summary>
        /// checks to see if the uri is from our dbpedia databse
        /// </summary>
        /// <param name="input">the uri to check</param>
        /// <returns>whether or not it's internal</returns>
        public static bool isInternalURI(string input)
        {
            for (int i = 0; i < allowedURIs.Count; i++)
            {
                if (input.Contains(allowedURIs[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// HELPER FUNCTION FOR THE encdoeURI
        /// </summary>
        /// <param name="s">the string to encode</param>
        /// <returns>the upper case encoded string</returns>
        private static string UpperCaseUrlEncode(string s)
        {
            char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }

        /// <summary>
        /// SHERIF WAY OF ENCODING OUR URIS (ONLY WORKS WITH DBPEDIA AND SUCH)
        /// POSSIBLE BUG
        /// </summary>
        /// <param name="input">INPUT URI</param>
        /// <returns>THE ENCODED OUTPUT</returns>
        public static string encodeURI(string input)
        {
            if (isInternalURI(input))
            {
                int indexAfterSlash = input.LastIndexOf("/") + 1;
                string afterSlash = input.Substring(indexAfterSlash);
                string encodedAfterSlash = UpperCaseUrlEncode(afterSlash);

                string toreturn = input.Replace(afterSlash, encodedAfterSlash);
                return toreturn;
            }
            else
                return input;


        }

        /// <summary>
        /// This method tries it's best to get a label for the wanted URI
        /// </summary>
        /// <param name="URI">uri to get label to</param>
        /// <returns>the label gotten</returns>
        public static string getLabel(string URI)
        {
            //if the string is empty return it as it is
            if (URI.Length == 0)
                return URI;
            else
            {
                //gottenlabels.Select(e=>e
                //string temp = gottenlabels.Select(e=>e.Key.Equals(URI)).Value;
                string temp = "";
                if (gottenlabels.TryGetValue(URI, out temp))
                {
                    return temp;
                }

                else
                {
                    //URI = Uri.EscapeUriString(URI);
                    if (isInternalURI(URI))
                        URI = encodeURI(URI);
                    if (isInternalURI(URI) && Uri.IsWellFormedUriString(URI, UriKind.Absolute))
                    {

                        //at least best one for now

                        //SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql"));
                        string query = "select * where {<" + URI + "> <http://www.w3.org/2000/01/rdf-schema#label> ?obj}";
                        //SparqlResultSet results = endpoint.QueryWithResultSet(query);
                        SparqlResultSet results = Request.RequestWithHTTP(query);
                        //if there's no results from the first query, we will try to get the name 
                        if (results.Count < 1)
                        {
                            string name_query = "select * where {<" + URI + "> <http://xmlns.com/foaf/0.1/name> ?obj}";
                            //results = endpoint.QueryWithResultSet(name_query);
                            results = Request.RequestWithHTTP(name_query);

                            //if there's no result from the second query
                            //get the name after the /
                            if (results.Count < 1)
                            {
                                //to decode the uri to remove the %xx characters
                                string newURI = HttpUtility.UrlDecode(URI);

                                while (newURI.Contains("%"))
                                    newURI = HttpUtility.UrlDecode(newURI);

                                //reversing the string and removing the last / and returning what's after it
                                string toreturn = new string(newURI.ToCharArray().Reverse().ToArray());//URI.Reverse().ToString();                    
                                toreturn = toreturn.Remove(toreturn.IndexOf("/"));
                                toreturn = new string(toreturn.ToCharArray().Reverse().ToArray());
                                toreturn = toreturn.Replace("_", " ");
                                //TODO : get back the encoding
                                toreturn = toreturn.Trim();
                                //This part to return the one after the #
                                if (toreturn.Contains("#") && toreturn.Length > toreturn.LastIndexOf("#"))
                                {
                                    //adding to the hashset
                                    addtohashset(URI, toreturn.Substring(toreturn.IndexOf("#") + 1));

                                    //return
                                    return toreturn.Substring(toreturn.IndexOf("#") + 1);
                                }
                                if (toreturn.Length > 0)
                                {
                                    //adding to the hashset
                                    addtohashset(URI, toreturn);

                                    //return
                                    return toreturn;
                                }
                                //this one is to return if empty
                                else
                                {
                                    //adding to the hashset
                                    addtohashset(URI, URI);

                                    //returning
                                    return newURI;
                                }

                            }
                            else
                            {
                                //adding to the hashset
                                addtohashset(URI, ((LiteralNode)results[0].Value("obj")).Value);

                                //returning
                                return ((LiteralNode)results[0].Value("obj")).Value;
                            }
                        }
                        else
                        {
                            //adding to the hashset
                            addtohashset(URI, ((LiteralNode)results[0].Value("obj")).Value);

                            //returning it
                            return ((LiteralNode)results[0].Value("obj")).Value;
                        }
                    }
                    else
                    {
                        return URI;
                    }
                }
            }
        }

        /// <summary>
        /// more strings to be added  
        /// </summary>
        /// <returns>return a list of strings to be igonred if the setteled in the Question match </returns>
        public static List<string> getIgnoreStrings()
        {
            List<string> toreturn = new List<string>();

            toreturn.Add("a");
            toreturn.Add("of");
            toreturn.Add("the");
            toreturn.Add("am");
            toreturn.Add("is");
            toreturn.Add("are");
            toreturn.Add("was");
            toreturn.Add("were");
            toreturn.Add("does");
            toreturn.Add("do");
            toreturn.Add("did");
            toreturn.Add("has");
            toreturn.Add("have");
            toreturn.Add("had");
            toreturn.Add("please");

            return toreturn;
        }




        /// <summary>
        ///Gets the type of the question and remove words used to match question type from the question string
        /// </summary>
        /// <param name="question">Input question to check type and remove words used to match the type</param>
        /// <returns>the modified question and its type</returns>
        public static List<string> GetQuestionType(string question)
        {
            List<string> questionAndType = new List<string>();   //question type initialized to 0 the default case
            string inputLine;   //string holds the lines read from the file

            List<string> questionTypeList = new List<string>();  //List to hold the contents of the question type file

            questionAndType.Add(question);  //add the original question to return it if no modifications happen
            questionAndType.Add("normal");    //add question type equals 0 (normal) as default

            bool cleanQuestion = false;

            using (StreamReader questionTypeFile = new StreamReader("QuestionTypes.txt"))   //read question types file into memory
            {
                inputLine = questionTypeFile.ReadLine();

                while ((inputLine != null))
                {
                    /*
                     * this will force a file format where:
                     * 1)regex of question start with '^' which is a regex reserved character means "the beginning of a string"
                     * 2)line starts with small english letters [a-z] only and this represent the type of the question
                     * It will allow comment lines (not blocks) in the file that will start with any character except '^' or [a-z]
                     * Better to write "//" as the comment line sign as its the common adopted style for writting comments
                     * */

                    while (!(Regex.IsMatch(inputLine, @"^(\^|[a-z])")))
                        inputLine = questionTypeFile.ReadLine();

                    questionTypeList.Add(inputLine);

                    inputLine = questionTypeFile.ReadLine();
                }
            }

            while (!cleanQuestion)
            {
                cleanQuestion = true;   //flag to tell if all the questions kewords are consumed

                for (int i = 0; i < questionTypeList.Count; i++)
                {
                    if (Regex.IsMatch(question, questionTypeList[i]))
                    {
                        question = Regex.Replace(question, questionTypeList[i], "");

                        question = question.Trim();

                        questionAndType[0] = question;

                        questionAndType[1] = questionTypeList[++i];

                        cleanQuestion = false;

                        break;
                    }
                    else
                        i++;
                }

            }

            return questionAndType;
        }

        /// <summary>
        /// maps the string of question type to enum type
        /// </summary>
        /// <param name="type">string of question type</param>
        /// <returns>enum of question type</returns>
        public static util.questionTypes mapQuestionType(string type)
        {

            switch (type)
            {
                case "count":
                    {
                        return questionTypes.countAnswer;
                    }
                case "normal":
                    {
                        return questionTypes.literalOrURIAnswer;
                    }
                default:
                    {
                        return questionTypes.unkown;
                    }
            }
        }
    }
}
