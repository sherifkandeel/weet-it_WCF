using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using VDS.RDF;

namespace mergedServices
{
    [DataContract]
    public class PartialAnswer
    {
        [DataMember]
        public util.questionTypes questiontype;

        [DataMember]
        public int answerCount;

        [DataMember]
        public List<string> objectUriList;
        [DataMember]
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
