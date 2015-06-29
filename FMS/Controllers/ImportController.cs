using Domain.Abstract;
using Domain.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using FMS.Models;
using System.Linq;
using System.IO;
using System.Web;
using System.Net.Http;
using System.Net;

namespace FMS.Controllers
{
    [Authorize]
    public class ImportController : ApiController
    {
        private readonly IFileImport _import;
        public ImportController(IFileImport import)
        {
            _import = import;
        }

        [HttpPost]
        [Route("api/import/{docType}/{fileId}")]
        public async Task<HttpResponseMessage> Index(string docType, string fileId)
        {
            DocumentType edocType;
            if(!Enum.TryParse(docType, out edocType))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Неверный тип документа");
            };

            string userID = User.Identity.GetUserId();
            var progress = await _import.GetProgressByFileNameAsync(fileId, userID);
            if (progress != null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Файл уже импортируется");
            };

            progress = await _import.CreateProgressAsync(fileId, userID);

            string filePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), fileId);
            HostingEnvironment.QueueBackgroundWorkItem(ct => _import.StartImport(progress, edocType, filePath, userID));

            return Request.CreateResponse(HttpStatusCode.OK, progress);
        }

        [HttpGet]
        [Route("api/import/progress")]
        public async Task<HttpResponseMessage> Progress()
        {
            var userID = User.Identity.GetUserId();

            var progress = await _import.GetCurrentUserProgressAsync(userID);
            if (progress != null && !progress.IsCompleted)
            {
                 return Request.CreateResponse(HttpStatusCode.OK, progress);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("api/import/progress/{id:int}")]
        public async Task<HttpResponseMessage> Progress(int id)
        {
            var progress = await _import.GetProgressByIdAsync(id, User.Identity.GetUserId());

            if (progress != null)
            {
                if (!progress.HasErrors)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, progress);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, progress);
                }
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
