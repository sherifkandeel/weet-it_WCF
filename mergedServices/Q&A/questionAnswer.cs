using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using System.IO;
using System.Runtime.Serialization;

namespace mergedServices
{
    [DataContract]
    public class questionAnswer
    {
        [DataMember]
        public util.questionTypes questiontype;
        [DataMember]
        public int answerCount;
        
        public List<LexiconLiteral> subjectList;          // literals used to get this Answer Literals in the Sparql Query 
        [DataMember]
        public Dictionary<string, string> subjectUriList;
        [DataMember]
        public Dictionary<string, string> subjectLabelList;
        [DataMember]
        public List<LexiconPredicate> predicateList;
        [DataMember]
        public Dictionary<string, string> predicateUriList;
        [DataMember]
        public Dictionary<string, string> predicateLabelList;
        
        public List<INode> objectNodetList;
        [DataMember]
        public List<string> objectUriList;
        [DataMember]
        public List<string> objectLabelList;  
        
        public SparqlResultSet resultSet;    //the base result set if u want to use it note:u'll have to implement the DotNetRDF libraries

        
        public questionAnswer()
        {
            subjectList = new List<LexiconLiteral>();
            subjectUriList = new Dictionary<string, string>();
            subjectLabelList = new Dictionary<string, string>();

            predicateList = new List<LexiconPredicate>();
            predicateUriList = new Dictionary<string, string>();
            predicateLabelList = new Dictionary<string, string>();

            objectNodetList = new List<INode>();
            objectUriList = new List<string>();
            objectLabelList = new List<string>();
        }

        public questionAnswer(util.questionTypes type)
        {
            questiontype = type;

            subjectList = new List<LexiconLiteral>();
            subjectUriList = new Dictionary<string,string>();
            subjectLabelList = new Dictionary<string,string>();

            predicateList = new List<LexiconPredicate>();
            predicateUriList = new Dictionary<string, string>();
            predicateLabelList = new Dictionary<string, string>();

            objectNodetList = new List<INode>();
            objectUriList = new List<string>();
            objectLabelList = new List<string>();
        }
    }
}
