using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace mergedServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    partial class MergedService : QAServiceInterface
    {
        public List<questionAnswer> GetAnswerWithQuestionStructure(string question)
        {
            Lexicon mylexicon = new Lexicon();

            answerGenerator answerGenerator = new answerGenerator();

            List<QueryBucket> queries = answerGenerator.generateQueries(question);

            List<questionAnswer> answers = answerGenerator.executeQueries(queries);

            return answers;
        }

        public List<PartialAnswer> GetPartialAnswer(string question)
        {
            List<PartialAnswer> answer = new List<PartialAnswer>();

            List<questionAnswer> fullAnswer = GetAnswerWithQuestionStructure(question);

            foreach (questionAnswer tempAns in fullAnswer)
            {
                answer.Add(new PartialAnswer(tempAns));
            }

            return answer;
        }
    }


}
