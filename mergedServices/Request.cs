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
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(new StreamReader("endpointURI.txt").ReadLine()));
            endpoint.Timeout = 999999;
            return endpoint.QueryWithResultSet(request);

        }


    }
}
