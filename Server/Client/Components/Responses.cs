using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Client
{
    class Response
    {
        public ResponseStatus Status { get; set; }
        public RequestType OnRequest { get; set; }
        public Dictionary<String, String> Data { get; set; }
    }

    enum ResponseStatus
    {
        Success,
        Error
    }

    class ResponsesList
    {
        public List<Response> Items { get; set; }
        private static ResponsesList instance;

        private ResponsesList()
        {
            Items = new List<Response>();
        }

        public static ResponsesList GetInstance()
        {
            if (instance == null)
                instance = new ResponsesList();
            return instance;
        }

        public Response GetResponseTo(RequestType Type, CancellationToken CancellationToken)
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                Response _response = Items.Find(response => response?.OnRequest == Type);
                if (_response != null)
                {
                    Items.Remove(_response);
                    return _response;
                }
            }
            return null;
        }
    }
}
