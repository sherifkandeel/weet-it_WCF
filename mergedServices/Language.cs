using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHunspell;

namespace mergedServices
{
    class Language
    {
        Hunspell hunspell;
        MyThes thes;
        public Language()
        {
            hunspell = new Hunspell(@"Language\en_us.aff", @"Language\en_us.dic");
            thes = new MyThes(@"Language\th_en_us_v2.dat");
        }

        public List<string> getSynonyoms(string word)
        {
            List<string> toReturn = new List<string>();
            //Synonyms for words            
            ThesResult tr = thes.Lookup(word, hunspell);  
            if(tr!=null)
            foreach (ThesMeaning meaning in tr.Meanings)
            {               
                foreach (string synonym in meaning.Synonyms)
                {
                    toReturn.Add(synonym);

                }
            }

            return toReturn;
        }


        public List<string> getStems(string word)
        {
            return  hunspell.Stem(word);
        }

    }
}
