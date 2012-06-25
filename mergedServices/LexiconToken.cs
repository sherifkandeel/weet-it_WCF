using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace mergedServices
{
    
    public abstract class LexiconToken
    {

        public string URI { get; set; }

        public string label { get; set; }

        public string QuestionMatch { get; set; }

        public string identifier { get; set; }

        public int score { get; set; }

        /// <summary>
        /// to returns the component of thelexicon token in a simple string 
        /// </summary>
        /// <returns>string containing the components of the lexicon token </returns>
        public abstract string ToSimpleString();

        public abstract string BuildQueryPart();

        public abstract LexiconToken getClone(LexiconToken token);
    }
}
