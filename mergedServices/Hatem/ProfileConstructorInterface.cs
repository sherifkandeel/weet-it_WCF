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
    }
}
