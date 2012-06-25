using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web;
using VDS.RDF.Query;
using VDS.RDF;
using mergedServices;

namespace mergedServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface keywordSearchServiceInterface
    {
       
        [OperationContract]
        string geturi_bestMatch(string keyword);
        [OperationContract]
        List<string> geturi(string keyword, int MaxUris = 1);
        [OperationContract]
        List<string> GetUris_VsKeywords(string text);
        [OperationContract]
        string GetUris_VsKeyword_comma(string text);
        [OperationContract]
        List<string> geturis_List(List<string> keywords);
        [OperationContract]
        List<List<string>> geturis_List_WithMaxuris(List<string> keywords, int MaxUris);
       
        // TODO: Add your service operations here
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations

    /// <summary>
    /// Process search for single word or multiple-words queries
    /// </summary>
    /// 
    
}
