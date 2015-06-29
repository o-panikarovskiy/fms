using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FMS.Providers
{
    public class GuidMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        public GuidMultipartFormDataStreamProvider(string path) : base(path) { }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            return Guid.NewGuid().ToString();
        }
    }
}
