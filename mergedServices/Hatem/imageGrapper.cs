using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VDS.RDF.Query;

namespace mergedServices
{
    public static class imageGrapper
    {
        //we should consider replacing the api call with querying from the local server
        static string sameasServicelink = "http://sameas.org/text";
        static string fb_imageApi = "https://usercontent.googleapis.com/freebase/v1/image/m/";
        public enum E { small, medium, large };
        static string largeimg = "?maxwidth=500&maxheight=500";
        static string smallimg = "?maxwidth=100&maxheight=100";

        /// <summary>
        /// this function will check first for dbpedia image link
        /// </summary>
        /// <param name="dbpediaUri">e.g http://dbpedia.org/resource/Egypt</param>
        /// <param name="imgsize">max sizeof the image required</param>
        /// <returns></returns>
        public static List<string> retrieve_img(string dbpediaUri, E imgsize = E.small)
        {

            string image_query = "select distinct * where{<" + dbpediaUri + "><http://xmlns.com/foaf/0.1/depiction> ?z}";
            SparqlResultSet dbimglink = Request.RequestWithHTTP(image_query);
            List<string> image_urls = new List<string>();
            if (dbimglink.Count != 0 && check_link(dbimglink[0].Value("z").ToString()))
            {
                image_urls.Add(dbpediaUri);
                return image_urls;
            }
            else if (dbimglink.Count != 0)
            {
                image_urls.Add(get_fb_link(dbpediaUri, imgsize));
                return image_urls;
            }
            else
                return null;
        }

        private static bool check_link(string dbpedialink)
        {
            HttpWebRequest imgchecker;
            HttpWebResponse response;
            Uri dbpediaUri;


            try
            {
                dbpediaUri = new Uri(dbpedialink);
                imgchecker = (HttpWebRequest)HttpWebRequest.Create(dbpediaUri);

                response = (HttpWebResponse)imgchecker.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        /// <summary>
        /// this one will get freebaseimage directly without checking dbpedia link
        /// </summary>
        /// <param name="dbpedialink">e.g http://dbpedia.org/resource/Syria</param>
        /// <param name="imgsize"></param>
        /// <returns></returns>
        public static string get_fb_link(string dbpedialink, E imgsize)
        {
            string freebaselink;
            string entity_freebase_id;
            List<string> similaruris;
            string[] sep = new string[] { "\n" };
            WebClient dondloadtext = new WebClient();
            dondloadtext.QueryString.Add("uri", dbpedialink);

            string temp = dondloadtext.DownloadString(sameasServicelink);
            similaruris = temp.Split(sep, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            
            foreach (string it in similaruris)
            {
                if (it.Contains("rdf.freebase") && it.Contains("m."))
                {
                    
                    freebaselink = it;

                    freebaselink = freebaselink.Replace("<", "");
                    freebaselink = freebaselink.Replace(">", "");
                    entity_freebase_id = freebaselink.Substring(freebaselink.IndexOf("/m.") + 3);
                    switch (imgsize)
                    {
                        case E.small:
                            return (fb_imageApi + entity_freebase_id + smallimg);
                        case E.large:
                            return (fb_imageApi + entity_freebase_id + largeimg);

                    }

                }

            }


            return null;
        }

    }
}
