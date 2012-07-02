using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{

    public class LexiconPredicate : LexiconToken
    {

        public List<string> domains { get; set; }

        public List<string> ranges { get; set; }

        public string type { get; set; }


        public LexiconPredicate()
        {
            domains = new List<string>();
            ranges = new List<string>();
        }

        /// <summary>
        /// returns a string the descriping the predicate 
        /// </summary>
        /// <returns>predicate containning all the elements of the predicate</returns>
        public override string ToSimpleString()
        {

            string s = "";

            s += "URI: " + this.URI + "\n";
            s += "label: " + this.label + "\n";
            s += "QuestionMatch : " + this.QuestionMatch + "\n";
            s += "identifier : " + this.identifier + "\n";
            s += "domains: \n";
            if (this.domains.Count() != 0)
            {
                foreach (string domain in this.domains)
                    s += domain + " \n ";
            }
            s += "ranges: \n";
            if (this.ranges.Count() != 0)
            {
                foreach (string range in this.ranges)
                    s += range + "\n";
            }
            s += "score :" + this.score + "\n";
            s += "-------------------------------------\n";
            return s;
        }

        /// <summary>
        /// returns a list of strings of predicate queries generated based on the given predicate token, its domains, and its ranges
        /// </summary>
        /// <returns>list of strings of this predicate's part of query</returns>
        public override string BuildQueryPart()
        {
            List<string> simpleDomain = util.URIToSimpleString(domains);
            List<string> simpleRange = util.URIToSimpleString(ranges);
            string predicateQueryPart = "";

            predicateQueryPart += "?" + simpleDomain[0] + " <" + util.encodeURI(this.URI) + "> " + "?Range";

            predicateQueryPart += " . " + "?" + simpleDomain[0] + " <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <" + domains[0] + ">";

            return predicateQueryPart;

            //  !!!!!!!!!!!!!!!!!!!!!! This part will be removed after testing the functionality of this function
            /*
            string tmpRange = GetSimpleRange()[0];
            string predicateQueryPart;

            //if range is of any of this types replace it with the predicate simple string
            if ("float,string,integer,real,boolean,bool".Contains(tmpRange))
                tmpRange = util.URIToSimpleString(this.URI);
            predicateQueryPart = "?" + GetSimpleDomain()[0] + " <" + this.URI + "> " + "?" + tmpRange;
            if (!"float,string,integer,real,boolean,bool,date".Contains(GetSimpleDomain()[0].ToLower()))
                predicateQueryPart = predicateQueryPart + " . " + "?" + GetSimpleDomain()[0] + " <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <" + domains[0] + ">";
            if (!"float,string,integer,real,boolean,bool,date".Contains(GetSimpleRange()[0].ToLower()))
                predicateQueryPart = predicateQueryPart + " . " + "?" + GetSimpleRange()[0] + " <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <" + ranges[0] + ">";
            return predicateQueryPart;
             * */
        }

        /// <summary>
        /// return a clone of the new bucket 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override LexiconToken getClone(LexiconToken token)
        {
            LexiconPredicate predicateToReplace = new LexiconPredicate();
            predicateToReplace.URI = token.URI;
            predicateToReplace.label = token.label;
            predicateToReplace.ranges = (token as LexiconPredicate).ranges.ToList();
            predicateToReplace.QuestionMatch = token.QuestionMatch;
            predicateToReplace.score = token.score;
            predicateToReplace.domains = (token as LexiconPredicate).domains.ToList();


            return predicateToReplace;
        }




    }
}
