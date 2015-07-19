using Domain.Abstract;
using Domain.Models;
using FMS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace FMS.Controllers
{
    [Authorize]
    public class DocumentController : ApiController
    {
        private readonly IRepository<Person> _repPeople;
        private readonly IRepository<Document> _repDocs;
        private readonly IRepository<ParameterName> _repParamNames;
        private readonly IRepository<DocumentParameter> _repDocParams;
        private readonly UserManager<ApplicationUser> _userManager;
        public DocumentController(IRepository<Person> repPeople, IRepository<Document> repDocs, IRepository<DocumentParameter> repDocParams,
            IRepository<ParameterName> repParamNames, UserManager<ApplicationUser> userManager)
        {
            _repDocs = repDocs;
            _repDocParams = repDocParams;
            _repParamNames = repParamNames;
            _repPeople = repPeople;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IHttpActionResult> CreateDocument([FromBody] DocumentBindModel docBind)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var personFrom = await _repPeople.FindAsync(p => p.Id == docBind.PersonFromId);

            if (personFrom == null)
            {
                return BadRequest("Анкета не найдена");
            }

            var userId = User.Identity.GetUserId();
            var now = DateTime.Now;

            var doc = new Document
            {
                CreatedById = userId,
                UpdatedById = userId,
                CreatedDate = now,
                UpdatedDate = now,
                HostPersonId = personFrom.Id,
                Type = (DocumentType)docBind.Type
            };

            if (docBind.Type == DocumentType.MigrationRegistration)
            {
                var personTo = _repPeople.Find(p => p.Id == docBind.PersonToId);
                if (personTo == null)
                {
                    return BadRequest("Анкета не найдена");
                }
                if (personTo.Type == PersonType.Host && personFrom.Type == PersonType.Host)
                {
                    return BadRequest("Принимающей стороне невозможно добавить принимающую сторону");
                }
                doc.ApplicantPersonId = personTo.Id;
            }

            _repDocs.Add(doc);

            return Ok(doc);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdateDocument(int id, [FromBody] DocumentViewModel docView)
        {
            var doc = await _repDocs.FindAsync(d => d.Id == id);
            if (doc == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            doc.Number = docView.Number;
            doc.UpdatedDate = DateTime.Now;
            doc.UpdatedById = User.Identity.GetUserId();

            _repDocs.Update(doc);

            UpdateDocumentParams(doc, docView.Parameters);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> RemoveDocument(int id)
        {
            var doc = await _repDocs.FindAsync(d => d.Id == id);
            if (doc == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _repDocs.Remove(doc);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
        private void UpdateDocumentParams(Document doc, IList<ParameterViewModel> parameters)
        {
            var names = _repParamNames.FindAll(d => d.Category == ParameterCategory.Document && d.DocType == doc.Type && d.IsFact == false).ToDictionary(k => k.Name, v => v);
            var docParams = _repDocParams.FindAll(d => d.DocumentId == doc.Id).ToDictionary(k => k.ParameterId, v => v);

            var list = new List<DocumentParameter>();
            foreach (var prm in parameters)
            {
                var pn = names[prm.Name];

                DocumentParameter docParam = null;
                if (docParams.ContainsKey(pn.Id))
                {
                    docParam = docParams[pn.Id];
                    docParam.DateValue = prm.DateValue;
                    docParam.FloatValue = prm.FloatValue;
                    docParam.IntValue = prm.MiscId;
                    docParam.StringValue = prm.StringValue;
                }
                else
                {
                    docParam = new DocumentParameter
                    {
                        ParameterId = pn.Id,
                        DocumentId = doc.Id,
                        DateValue = prm.DateValue,
                        FloatValue = prm.FloatValue,
                        IntValue = prm.MiscId,
                        StringValue = prm.StringValue
                    };
                }

                list.Add(docParam);
            };

            _repDocParams.AddRange(list.Where(p => p.Id == 0).AsEnumerable());
            _repDocParams.UpdateRange(list.Where(p => p.Id != 0).AsEnumerable());
        }
    }
}
