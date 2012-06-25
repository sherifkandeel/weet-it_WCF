using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace mergedServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    partial class MergedService : keywordSearchServiceInterface
    {

        public string geturi_bestMatch(string keyword)
        {
            return KwSearch.geturi(keyword);
        }
        public List<string> geturi(string keyword, int MaxUris = 1)
        {
            return KwSearch.geturi(keyword, MaxUris);
        }
        public List<string> GetUris_VsKeywords(string text)
        {
            return KwSearch.GetUris_VsKeywords(text);
        }

        public string GetUris_VsKeyword_comma(string text)
        {
            return KwSearch.GetUris_VsKeyword_comma(text);
        }

        public List<string> geturis_List(List<string> keywords)
        {
            return KwSearch.geturis_List(keywords);
        }
        public List<List<string>> geturis_List_WithMaxuris(List<string> keywords, int MaxUris)
        {
            return KwSearch.geturis_List_WithMaxuris(keywords, MaxUris);
        }
    }
}
