using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace mergedServices
{
    [ServiceContract]
    public interface CompareWithOnePredicateServiceInterface
    {
        [OperationContract]
        List<List<String>> CompareWithRespect(List<String> subjectsNames, String predicateURI, int limit);
    }
}
