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
                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(new StreamReader("endpointURI.txt").ReadLine()));
                endpoint.Timeout = 999999;
                toreturn = endpoint.QueryWithResultSet(request);
            }
            catch (Exception e)
            {

                util.log(e.Message + "==>" + e.Data);
            }
            return toreturn;

        }


    }
}
