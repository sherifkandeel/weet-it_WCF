using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using VDS.RDF;

namespace mergedServices
{
    public class PartialAnswer
    {
        public util.questionTypes questiontype;

        public int answerCount;

        public List<string> objectUriList;

        public List<string> objectLabelList;

        public PartialAnswer(questionAnswer answer)
        {
            this.questiontype = answer.questiontype;

            this.answerCount = answer.answerCount;

            this.objectUriList = answer.objectUriList;

            this.objectLabelList = answer.objectLabelList;
        }
    }
}
