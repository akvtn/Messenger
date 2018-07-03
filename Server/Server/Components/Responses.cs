using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Response
    {
        public ResponseStatus Status { get; set; }
        public RequestType OnRequest { get; set; }
        public Dictionary<String, String> Data { get; set; }

        public Response(ResponseStatus status, RequestType onRequest)
        {
            OnRequest = onRequest;
            Status = status;
            Data = null;
        }

        public Response(ResponseStatus status, RequestType onRequest, Dictionary<String, String> data)
        {
            OnRequest = onRequest;
            Status = status;
            Data = data;
        }
    }

    enum ResponseStatus
    {
        Success,
        Error
    }
}
