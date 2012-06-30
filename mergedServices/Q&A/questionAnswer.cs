using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;
using System.IO;
using System.Runtime.Serialization;

namespace mergedServices
{

    public class questionAnswer
    {

        public util.questionTypes questiontype;

        public int answerCount;

        public List<LexiconLiteral> subjectList;          // literals used to get this Answer Literals in the Sparql Query 

        public Dictionary<string, string> subjectUriList;

        public Dictionary<string, string> subjectLabelList;

        public List<LexiconPredicate> predicateList;

        public Dictionary<string, string> predicateUriList;

        public Dictionary<string, string> predicateLabelList;
        
        public List<INode> objectNodetList;

        public List<string> objectUriList;

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
