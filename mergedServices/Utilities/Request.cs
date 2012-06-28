using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;
using System.IO;
using VDS.RDF.Storage;
using VDS.RDF.Parsing;


namespace mergedServices
{
    public static class Request
    {
        public static SparqlResultSet RequestWithHTTP(string request)
        {
            SparqlResultSet toreturn = new SparqlResultSet();            
            try
            {
                StreamReader sr = new StreamReader("endpointURI.txt");
                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(sr.ReadLine()));
                sr.Close();
                endpoint.Timeout = 999999;
                toreturn = endpoint.QueryWithResultSet(request);
            }
            catch (Exception e)
            {

                util.log(request + e.Message + "==>" + e.Data);
            }
            return toreturn;

        }


    }
}
