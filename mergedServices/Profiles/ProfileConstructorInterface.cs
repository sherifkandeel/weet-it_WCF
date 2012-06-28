using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace mergedServices
{
    [ServiceContract]
    public interface ProfileConstructorInterface
    {
        [OperationContract]
        Profile ConstructProfile(String subjectURI, MergedService.choiceProfile profile, int resultLimit = 10);

        [OperationContract]
        Profile ConstructLiteralProfile(string subjectURI, string predicate_label, string subject_label, string object_URI, string object_value, string pred_URI);
        
    }
}
