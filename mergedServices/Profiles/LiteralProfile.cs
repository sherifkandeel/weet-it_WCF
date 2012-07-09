using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using System.Runtime.Serialization;
namespace mergedServices
{
    [DataContract]
    public class LiteralProfile :Profile
    {
        [DataMember]
        public string PredicateLabel;
        [DataMember]
        public string objectValue;
        [DataMember]
        public string objectUnit;
        [DataMember]
        public string imageURI;
        [DataMember]
        public string subjectLabel;
        [DataMember]
        string subjectURI;
        string query_predicate;
       
        public LiteralProfile(string sl, string predlabel, string object_string, string predURI,string subjURI)
        {
            initialize();

            subjectURI = subjURI;
            subjectLabel = sl.Replace("@en","");
            PredicateLabel = predlabel;
            imageURI = imageGrapper.get_fb_link(subjectURI, imageGrapper.E.large);

            if (object_string!=null && object_string.Contains("http://"))
            {
                objectValue = object_string.Substring(0, object_string.IndexOf("^^"));
                objectUnit = check_object_unit(object_string, predURI);
            }
            else
                objectValue = object_string;
            objectValue = objectValue.Replace("@en", "");
        }
        private void initialize()
        {
            PredicateLabel=" ";
            objectValue=" ";
            objectUnit = " ";
                imageURI=" ";
                subjectLabel=" ";
        }
        private string check_object_unit(string object_string, string predicate_URI)
        {
            string object_temp_unit;
            predicate_URI = util.encodeURI(predicate_URI);
            if (object_string.Contains("/XMLSchema"))
            {
                object_temp_unit = util.getLabel(util.encodeURI(predicate_URI));
                try
                {
                    object_temp_unit = object_temp_unit.Substring(object_temp_unit.IndexOf('('));
                }
                catch
                {
                    return object_temp_unit;
                }
               
                return object_temp_unit;
            }
            
            else
            {
                query_predicate = "select distinct  ?predlabel ?predrange where {<"
                + predicate_URI + "> <http://www.w3.org/2000/01/rdf-schema#label> ?predlabel .<"
                + predicate_URI + "><http://www.w3.org/2000/01/rdf-schema#range> ?predrange}";
                 
               
                SparqlResultSet predicate_range = Request.RequestWithHTTP(query_predicate);
              
                if (predicate_range.Count != 0)//&&predicate_range[0].Value("predlabel")!=null&&predicate_range[0].Value("predlabel")!=null)
                {
                    object_temp_unit = check_object_unit(predicate_range[0].Value("predlabel").ToString(), predicate_range[0].Value("predrange").ToString());
                    return object_temp_unit;
                }
                else
                {
                    object_temp_unit = util.getLabel(util.encodeURI(predicate_URI));
                   // object_temp_unit = object_temp_unit.Substring(object_temp_unit.IndexOf('('));
                    //object_temp_unit.Replace("@en", "");
                    return object_temp_unit;

                }
            }

            

        }
       


    }
}
