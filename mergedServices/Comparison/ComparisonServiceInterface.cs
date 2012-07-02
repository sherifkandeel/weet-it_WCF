using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace mergedServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ComparisonServiceInterface
    {
        [OperationContract]
        List<ResourceInformation> Compare(List<string> URIs);

        [OperationContract]        
        KeyValuePair<string, string> getID(ResourceInformation ri);

        [OperationContract]
        List<KeyValuePair<string, string>> getPredicates_ResourceIsSubj(ResourceInformation ri);

        [OperationContract]
        List<KeyValuePair<string, string>> getPredicates_ResourceIsObj(ResourceInformation ri);

        [OperationContract]
        List<KeyValuePair<string, string>> getResourcesOfPredicate(KeyValuePair<string, string> pred, ResourceInformation ri);

        
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations
    [DataContract]    
    public class ResourceInformation
    {
        //it's own id 
        [DataMember]
        public KeyValuePair<string, string> ID;
        //predicates information
        //URI, Label
        [DataMember]
        public List<KeyValuePair<string, string>> predicates_resourceIsSubj;//= new List<KeyValuePair<string, string>>(); 

        //resources information
        //URI,label
        [DataMember]
        public List<KeyValuePair<string, string>> resources_resourceIsSubj;//= new  List<KeyValuePair<string, string>>();

        //predicates information
        //URI, Label
        [DataMember]
        public List<KeyValuePair<string, string>> predicates_resourceIsObj;//=new List<KeyValuePair<string, string>>();

        //resources information
        //URI,label
        [DataMember]
        public List<KeyValuePair<string, string>> resources_resourceIsObj;//=new  List<KeyValuePair<string, string>>();

        //resources put in a form of Pred -> List<it's resources>
        [DataMember]
        public List<KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>> rawComparisonObject;

        //common resources between this resource and others in the same component
        [DataMember]
        public List<KeyValuePair<KeyValuePair<string, string>, List<KeyValuePair<string, string>>>> FinalComparisonObject;

    }





}
