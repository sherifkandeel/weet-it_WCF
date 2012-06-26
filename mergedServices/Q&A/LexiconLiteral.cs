using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{

    public class LexiconLiteral : LexiconToken
    {

        public List<string> typeOfOwner { get; set; }

        public string predicate { get; set; }
        

        #region constructors
        public LexiconLiteral()
        {
            typeOfOwner = new List<string>();

        }
        public LexiconLiteral(string URI, string label, string QuestionMatch, string typeOfOwner)
        {
            this.URI = URI;
            this.label = label;
            this.QuestionMatch = QuestionMatch;
            List<string> typeOfOwnerList = new List<string>();
            typeOfOwnerList.Add(typeOfOwner);
            this.typeOfOwner = typeOfOwnerList;
        }
        public LexiconLiteral(string URI, string label, string QuestionMatch, List<string> typeOfOwnerList)
        {
            this.URI = URI;
            this.label = label;
            this.QuestionMatch = QuestionMatch;
            this.typeOfOwner = typeOfOwnerList;
        }
        #endregion

        /// <summary>
        /// returns a string the descriping the Literal 
        /// </summary>
        /// <returns>predicate containning all the elements of the Literal</returns>
        public override string ToSimpleString()
        {
            string s = "";

            s += "URI: " + this.URI + "\n";
            s += "label: " + this.label + "\n";
            s += "QuestionMatch : " + this.QuestionMatch + "\n";
            s += "identifier : " + this.identifier + "\n";
            s += "typeOfOwner : " + string.Join("\n", this.typeOfOwner.ToArray());
            s += "predicate : " + this.predicate + "\n";
            s += "score :" + this.score + "\n";
            s += "-------------------------------------\n";
            return s;
        }

        /// <summary>
        /// returns a string of a part of query generated based on the given literal token and its type of owners
        /// </summary>
        /// <returns>string of this literal's part of query</returns>
        public override string BuildQueryPart()
        {
            string literalQueryPart;

            if (typeOfOwner.Count == 1)
            {
                literalQueryPart = "filter( ?" + util.URIToSimpleString(typeOfOwner[0]) + " = <" + this.URI + "> )";
                return literalQueryPart;
            }
            else if (typeOfOwner.Count > 1)
            {
                literalQueryPart = "filter( ?" + util.URIToSimpleString(typeOfOwner[0]) + " = <" + this.URI + ">";

                for (int i = 1; i < typeOfOwner.Count; i++)
                {
                    literalQueryPart += " || ?" + util.URIToSimpleString(typeOfOwner[i]) + " = <" + this.URI + ">";
                }

                literalQueryPart += " )";
                return literalQueryPart;
            }
            else
                return "";

        }

        /// <summary>
        /// return a clone of the token send to the function
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override LexiconToken getClone(LexiconToken token)
        {
            LexiconLiteral literalToReplace = new LexiconLiteral();
            literalToReplace.URI = token.URI;
            literalToReplace.label = token.label;
            literalToReplace.typeOfOwner = (token as LexiconLiteral).typeOfOwner.ToList();
            literalToReplace.QuestionMatch = token.QuestionMatch;
            literalToReplace.score = token.score;

            return literalToReplace;
        }

    }
}
