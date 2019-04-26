using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Aegis.Network
{
    public class HttpRequestHandler
    {
        private DispatchMethodSelector<HttpRequestData> _methodSelector;





        protected HttpRequestHandler()
        {
            _methodSelector = new DispatchMethodSelector<HttpRequestData>(this, (ref HttpRequestData source, out string key) =>
            {
                key = source.Path;
            });
        }


        internal void Process(HttpRequestData request)
        {
            if (PreprocessRequest(request) == true)
                _methodSelector.Invoke(request);
        }


        protected virtual bool PreprocessRequest(HttpRequestData request)
        {
            return true;
        }
    }
}
