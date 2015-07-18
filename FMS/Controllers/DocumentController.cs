using Domain.Abstract;
using Domain.Models;
using FMS.Models;
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

        private readonly IRepository<Document> _repDocs;
        private readonly IRepository<ParameterName> _repParamNames;
        private readonly IRepository<DocumentParameter> _repDocParams;
        public DocumentController(IRepository<Document> repDocs, IRepository<DocumentParameter> repDocParams, IRepository<ParameterName> repParamNames)
        {
            _repDocs = repDocs;
            _repDocParams = repDocParams;
            _repParamNames = repParamNames;
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

            _repDocs.Update(doc);

            UpdateDocumentParams(doc, docView.Parameters);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        private void UpdateDocumentParams(Document doc, IList<ParameterViewModel> parameters)
        {
            var names = _repParamNames.FindAll(d => d.Category == ParameterCategory.Document && d.DocType == doc.Type && d.IsFact == false).ToDictionary(k => k.Name, v => v);
            var allPrms = _repDocParams.FindAll(d => d.DocumentId == doc.Id).ToDictionary(k => k.ParameterId, v => v);

            var list = new List<DocumentParameter>();
            foreach (var prm in parameters)
            {
                var pn = names[prm.Name];
                var dbParam = allPrms[pn.Id];

                if (dbParam != null)
                {
                    dbParam.DateValue = prm.DateValue;
                    dbParam.FloatValue = prm.FloatValue;
                    dbParam.IntValue = prm.MiscId;
                    dbParam.StringValue = prm.StringValue;
                }
                else
                {
                    dbParam = new DocumentParameter
                    {
                        ParameterId = pn.Id,
                        DocumentId = doc.Id,
                        DateValue = prm.DateValue,
                        FloatValue = prm.FloatValue,
                        IntValue = prm.MiscId,
                        StringValue = prm.StringValue
                    };
                }

                list.Add(dbParam);
            };

            _repDocParams.AddRange(list.Where(p => p.Id == 0).AsEnumerable());
            _repDocParams.UpdateRange(list.Where(p => p.Id != 0).AsEnumerable());
        }
    }
}
