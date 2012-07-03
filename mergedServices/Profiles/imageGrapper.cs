using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VDS.RDF.Query;

namespace mergedServices
{
    /// <summary>
    /// the glass grabs an image for the given entity URI
    /// </summary>
    public static class imageGrapper
    {
        static string freebase_api_key="&key=AIzaSyCNg92r5xbwPoUThpkGAFoyZU4MSckqdgg";
        //we should consider replacing the api call with querying from the local server
        // static string sameasServicelink = "http://sameas.org/text";
        static string fb_imageApi = "https://usercontent.googleapis.com/freebase/v1/image/m/";
        public enum E { small, medium, large };
        static string largeimg = "?maxwidth=500&maxheight=500";
        static string smallimg = "?maxwidth=100&maxheight=100";
        static string medimg = "?maxwidth=250&maxheight=250";
        static string sameAs_URI_property = "http://www.w3.org/2002/07/owl#sameAs";
        /// <summary>
        /// this function will check first for dbpedia image link
        /// </summary>
        /// <param name="dbpediaUri">e.g http://dbpedia.org/resource/Egypt </param>
        /// <param name="imgsize">max sizeof the image required</param>
        /// <returns></returns>
        public static List<string> retrieve_img(string dbpediaUri, E imgsize = E.small)
        {
            dbpediaUri = util.encodeURI(dbpediaUri);
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
        /// <param name="dbpedialink">e.g http://dbpedia.org/resource/Syria </param>
        /// <param name="imgsize"></param>
        /// <returns></returns>
        public static string get_fb_link(string dbpedialink, E imgsize)
        {
            dbpedialink = util.encodeURI(dbpedialink);
            string SameAs_query = "select distinct ?freebaselink where {<" + dbpedialink + "> <" + sameAs_URI_property + "> ?freebaselink }";

            string freebaselink = "";
            string entity_freebase_id;
            //  List<string> similaruris = new List<string>() ;
            //   string[] sep = new string[] { "\n" };
            //WebClient dondloadtext = new WebClient();
            //dondloadtext.QueryString.Add("uri", dbpedialink);
            //try
            //{
            //     temp = dondloadtext.DownloadString(sameasServicelink);
            //     similaruris = temp.Split(sep, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            //}
            //catch( HttpListenerException noService)
            //{

            //    return null;
            //}

            SparqlResultSet freebase_sameAs = Request.RequestWithHTTP(SameAs_query);

            if (freebase_sameAs.Count != 0)
            {
                freebaselink = freebase_sameAs[0].Value("freebaselink").ToString();
                freebaselink = freebaselink.Replace("<", "");
                freebaselink = freebaselink.Replace(">", "");
                entity_freebase_id = freebaselink.Substring(freebaselink.IndexOf("/m/") + 3);

                switch (imgsize)
                {
                    case E.small:
                        return (fb_imageApi + entity_freebase_id + smallimg+freebase_api_key);
                    case E.large:
                        return (fb_imageApi + entity_freebase_id + largeimg + freebase_api_key);
                    case E.medium:
                        return (fb_imageApi + entity_freebase_id + medimg + freebase_api_key);

                }
            }
            else
            {
                
                    string image_query = "select distinct * where{<" + dbpedialink + "><http://xmlns.com/foaf/0.1/depiction> ?z}";
             
                    SparqlResultSet dbpedia_imageurl = Request.RequestWithHTTP(image_query);
                if(dbpedia_imageurl.Count!=0)
                    return dbpedia_imageurl[0].Value("z").ToString();
            }



            return "#";
        }

    }
}
