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
        string subjectURI;
        string query_predicate;
        string predicateURI;

        public LiteralProfile(string sl, string predlabel, string object_string, string predURI)
        {
            subjectLabel = sl;
            PredicateLabel = predlabel;
            imageURI = imageGrapper.get_fb_link(subjectURI, imageGrapper.E.large);

            if (object_string.Contains("http://"))
            {
                objectValue = object_string.Substring(0, object_string.IndexOf("^^"));
                objectUnit = check_object_unit(object_string, predURI);
            }
            else
                objectValue = object_string;
        }
        private string check_object_unit(string object_string, string predicate_URI)
        {
            string object_temp_unit;
            if (object_string.Contains("http://"))
            {
                //string objectURI = object_string.Substring("http://");
                query_predicate = "select ?predrange  ?predlabel where {<" + predicate_URI + "> <http://www.w3.org/2000/01/rdf-schema#range> ?predrange . " +
                " <" + predicate_URI + "> <www.w3.org/2000/01/rdf-schema#label> ?predlabel .}. ";
                SparqlResultSet predicate_range = Request.RequestWithHTTP(query_predicate);
                if (predicate_range.Count != 0)//&&predicate_range[0].Value("predlabel")!=null&&predicate_range[0].Value("predlabel")!=null)
                {
                    object_temp_unit = check_object_unit(predicate_range[0].Value("predlabel").ToString(), predicate_range[0].Value("predrange").ToString());
                    return object_temp_unit;
                }
                else
                {
                    object_temp_unit = util.getLabel(object_string.Substring(object_string.IndexOf("http://")));
                    return object_temp_unit;
                }

            }
            else 
            {
                return object_string;
            }

            

        }
        private void Construct_Profile()
        {

        }


    }
}
