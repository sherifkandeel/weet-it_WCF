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
    public interface RelationGeneratorServiceInterface
    {
        [OperationContract]
        List<List<string>> simpleGetRelations(List<string> uri, int Distance, int Limit = 50);

        [OperationContract]
        List<List<KeyValuePair<string, string>>> simpleGetRelationWithLabels(List<string> uri, int Distance, int Limit = 50);

        [OperationContract]
        List<Relation> getRelations(List<string> uri, int Distance, int Limit = 50);

        [OperationContract]
        List<Relation> getRelationWithLabels(List<string> uri, int Distance, int Limit = 50);

    
    
    }
}
