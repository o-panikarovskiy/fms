using FMS.Providers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FMS.Controllers
{
    [Authorize]
    public class UploadController : ApiController
    {

        [HttpPost]
        public async Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            bool exists = System.IO.Directory.Exists(root);
            if (!exists)
            {
                Directory.CreateDirectory(root);
            }

            var provider = new GuidMultipartFormDataStreamProvider(root);
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { Id = Path.GetFileNameWithoutExtension(provider.FileData[0].LocalFileName) });
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }


}
