using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;

namespace mergedServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    public interface QAServiceInterface
    {
        List<questionAnswer> GetAnswerWithQuestionStructure(string question);

        List<PartialAnswer> GetPartialAnswer(string question);
    }
}
