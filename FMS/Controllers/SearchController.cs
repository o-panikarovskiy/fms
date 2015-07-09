using Domain.Abstract;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FMS.Models;
using Domain.Utils;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace FMS.Controllers
{
    [Authorize]
    public class SearchController : ApiController
    {
        private readonly IRepository<Person> _repPeople;
        private readonly IRepository<SearchQuery> _repQueries;
        private readonly IRepository<PrmFactName> _repPrmFactNames;
        private readonly IRepository<PersonFact> _repPersonFacts;
        private readonly IRepository<PersonParameter> _repPersonParams;
        private readonly IRepository<Document> _repDocuments;
        private readonly IRepository<DocumentParameter> _repDocParams;

        public SearchController(IRepository<Person> repPerople, IRepository<Document> repDocuments,
            IRepository<SearchQuery> repQueries, IRepository<PrmFactName> repPrmFactNames, IRepository<PersonFact> repPersonFacts,
            IRepository<PersonParameter> repPersonParams, IRepository<DocumentParameter> repDocParams)
        {
            _repPeople = repPerople;
            _repQueries = repQueries;
            _repPersonFacts = repPersonFacts;
            _repPrmFactNames = repPrmFactNames;
            _repPersonParams = repPersonParams;
            _repDocuments = repDocuments;
            _repDocParams = repDocParams;
        }

        [HttpPost]
        public async Task<IHttpActionResult> SaveUserQuery([FromBody]SearchQueryBindingModel query)
        {
            if (query == null)
            {
                return BadRequest();
            }

            var sq = JsonConvert.SerializeObject(query);
            var q = await _repQueries.FindAsync(x => x.Query == sq);

            if (q == null)
            {
                q = new SearchQuery
                {
                    Query = sq
                };
                await _repQueries.AddAsync(q);
            }

            return Ok(new { id = q.Id });
        }

        [HttpGet]
        public IHttpActionResult GetSearchResults(int id, [FromUri]int limit = 20, [FromUri]int page = 0)
        {
            var squery = _repQueries.Find(x => x.Id == id);
            if (squery == null)
            {
                return NotFound();
            }

            var query = JsonConvert.DeserializeObject<SearchQueryBindingModel>(squery.Query);

            var q = _repPeople.GetAll();

            #region Person

            if (query.Person != null)
            {
                if (!string.IsNullOrWhiteSpace(query.Person.Name))
                {
                    var code = query.Person.Name;
                    q = q.Where(p => p.Name.Contains(query.Person.Name) || p.Code.Contains(code));
                }

                if (query.Person.Birthday != null)
                {
                    q = q.Where(p => p.Birthday == query.Person.Birthday);
                }

                if (query.Person.Category != null)
                {
                    q = q.Where(p => p.Category == query.Person.Category);
                }

                if (query.Person.Type != null)
                {
                    q = q.Where(p => p.Type == query.Person.Type);
                }

                if (query.Person.Citizenship != null)
                {
                    q = from p in q
                        join pf in _repPersonFacts.GetAll() on p.Id equals pf.PersonId
                        join pfg in GetPersonFactsSubQuery("Гражданство") on pf.PersonId equals pfg.PersonId
                        where pf.FactDate == pfg.FactDate && pf.IntValue == query.Person.Citizenship
                        select p;
                }

                if (!string.IsNullOrWhiteSpace(query.Person.Address))
                {
                    q = from p in q
                        join pf in _repPersonFacts.GetAll() on p.Id equals pf.PersonId
                        join pfg in GetPersonFactsSubQuery("Адрес") on pf.PersonId equals pfg.PersonId
                        where pf.FactDate == pfg.FactDate && pf.StringValue.Contains(query.Person.Address)
                        select p;
                }

                if (query.Person.DocType != null)
                {
                    q = from p in q
                        join pf in _repPersonFacts.GetAll() on p.Id equals pf.PersonId
                        join pfg in GetPersonFactsSubQuery("Тип документа") on pf.PersonId equals pfg.PersonId
                        where pf.FactDate == pfg.FactDate && pf.IntValue == query.Person.DocType
                        select p;
                }

                if (!string.IsNullOrWhiteSpace(query.Person.DocNo))
                {
                    q = from p in q
                        join pf in _repPersonFacts.GetAll() on p.Id equals pf.PersonId
                        join pfg in GetPersonFactsSubQuery("Тип документа") on pf.PersonId equals pfg.PersonId
                        where pf.FactDate == pfg.FactDate && pf.StringValue.Contains(query.Person.DocNo)
                        select p;
                }

                if (!string.IsNullOrWhiteSpace(query.Person.Phone))
                {
                    q = from p in q
                        join pp in GetPersonParametersSubQuery("Телефон") on p.Id equals pp.PersonId
                        where pp.StringValue.Contains(query.Person.Phone)
                        select p;
                }

                if (!string.IsNullOrWhiteSpace(query.Person.Email))
                {
                    q = from p in q
                        join pp in GetPersonParametersSubQuery("Электронная почта") on p.Id equals pp.PersonId
                        where pp.StringValue.Contains(query.Person.Email)
                        select p;
                }
            }

            #endregion

            if (query.Docs != null)
            {
                List<DocumentType> docTypes = new List<DocumentType>(5);
                var docs = _repDocuments.GetAll();

                #region Административная практика

                if (query.Docs.Ap != null && query.Docs.Ap.IsChecked)
                {
                    docTypes.Add(DocumentType.AdministrativePractice);

                    if (!string.IsNullOrWhiteSpace(query.Docs.Ap.DocNo))
                    {
                        docs = from d in docs
                               where d.Number.Contains(query.Docs.Ap.DocNo)
                               select d;
                    }

                    if (query.Docs.Ap.StDateCreate != null && query.Docs.Ap.EndDateCreate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата составления") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Ap.StDateCreate && dp.DateValue <= query.Docs.Ap.EndDateCreate
                               select d;
                    }
                    else if (query.Docs.Ap.StDateCreate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата составления") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Ap.StDateCreate
                               select d;
                    }
                    else if (query.Docs.Ap.EndDateCreate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата составления") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Ap.EndDateCreate
                               select d;
                    }

                    if (query.Docs.Ap.Article != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Статья") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Ap.Article
                               select d;
                    }

                    if (query.Docs.Ap.CrimeType != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Вид правонарушения") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Ap.CrimeType
                               select d;
                    }

                    if (query.Docs.Ap.StateDepartment != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Орган рассмотрения") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Ap.StateDepartment
                               select d;
                    }

                    if (query.Docs.Ap.DocStatus != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Статус дела") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Ap.DocStatus
                               select d;
                    }

                    if (query.Docs.Ap.StDecreeDate != null && query.Docs.Ap.EndDecreeDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата постановления") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Ap.StDecreeDate && dp.DateValue <= query.Docs.Ap.EndDecreeDate
                               select d;
                    }
                    else if (query.Docs.Ap.StDecreeDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата постановления") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Ap.StDecreeDate
                               select d;
                    }
                    else if (query.Docs.Ap.EndDecreeDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата постановления") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Ap.EndDecreeDate
                               select d;
                    }

                    if (query.Docs.Ap.DecreeStr != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Принятое решение") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Ap.DecreeStr
                               select d;
                    }

                    if (query.Docs.Ap.PenaltyType != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Тип взыскания") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Ap.PenaltyType
                               select d;
                    }

                }

                #endregion

                #region Миграционный учёт

                if (query.Docs.Mu != null && query.Docs.Mu.IsChecked)
                {
                    docTypes.Add(DocumentType.MigrationRegistration);

                    if (!string.IsNullOrWhiteSpace(query.Docs.Mu.DocNo))
                    {
                        docs = from d in docs
                               where d.Number.Contains(query.Docs.Mu.DocNo)
                               select d;
                    }

                    if (query.Docs.Mu.CardMark != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Отметка проставлена") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Mu.CardMark
                               select d;
                    }

                    if (query.Docs.Mu.PurposeOfEntry != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Цель въезда") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Mu.PurposeOfEntry
                               select d;
                    }

                    if (query.Docs.Mu.PrimaryExtend != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Первично/Продлено") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Mu.PrimaryExtend
                               select d;
                    }

                    if (query.Docs.Mu.KPP != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("КПП въезда") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Mu.KPP
                               select d;
                    }

                    if (query.Docs.Mu.StIncomeDate != null && query.Docs.Mu.EndIncomeDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата въезда") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Mu.StIncomeDate && dp.DateValue <= query.Docs.Mu.EndIncomeDate
                               select d;
                    }
                    else if (query.Docs.Mu.StIncomeDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата въезда") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Mu.StIncomeDate
                               select d;
                    }
                    else if (query.Docs.Mu.EndIncomeDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата въезда") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Mu.EndIncomeDate
                               select d;
                    }

                    if (query.Docs.Mu.StIssueDate != null && query.Docs.Mu.EndIssueDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Mu.StIssueDate && dp.DateValue <= query.Docs.Mu.EndIssueDate
                               select d;
                    }
                    else if (query.Docs.Mu.StIssueDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Mu.StIssueDate
                               select d;
                    }
                    else if (query.Docs.Mu.EndIssueDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Mu.EndIssueDate
                               select d;
                    }

                    if (query.Docs.Mu.RegDateFrom != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата регистрации С") on d.Id equals dp.DocumentId
                               where dp.DateValue == query.Docs.Mu.RegDateFrom
                               select d;
                    }

                    if (query.Docs.Mu.RegDateTo != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата регистрации ДО") on d.Id equals dp.DocumentId
                               where dp.DateValue == query.Docs.Mu.RegDateTo
                               select d;
                    }

                }

                #endregion

                #region РВП

                if (query.Docs.Rvp != null && query.Docs.Rvp.IsChecked)
                {
                    docTypes.Add(DocumentType.TemporaryResidencePermit);

                    if (!string.IsNullOrWhiteSpace(query.Docs.Rvp.DocNo))
                    {
                        docs = from d in docs
                               where d.Number.Contains(query.Docs.Rvp.DocNo)
                               select d;
                    }

                    if (!string.IsNullOrWhiteSpace(query.Docs.Rvp.DecisionNo))
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Номер решения") on d.Id equals dp.DocumentId
                               where dp.StringValue.Contains(query.Docs.Rvp.DecisionNo)
                               select d;
                    }
                    if (!string.IsNullOrWhiteSpace(query.Docs.Rvp.RvpNo))
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Номер РВП") on d.Id equals dp.DocumentId
                               where dp.StringValue.Contains(query.Docs.Rvp.RvpNo)
                               select d;
                    }

                    if (query.Docs.Rvp.AdmissionReason != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Основание для приема") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Rvp.AdmissionReason
                               select d;
                    }

                    if (query.Docs.Rvp.DecisionBase != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Основание решения") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Rvp.DecisionBase
                               select d;
                    }

                    if (query.Docs.Rvp.DecisionUser != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Пользователь решения") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Rvp.DecisionUser
                               select d;
                    }

                    //Дата приема заявления
                    if (query.Docs.Rvp.StDateOfReceipt != null && query.Docs.Rvp.EndDateOfReceipt != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата приема заявления") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Rvp.StDateOfReceipt && dp.DateValue <= query.Docs.Rvp.EndDateOfReceipt
                               select d;
                    }
                    else if (query.Docs.Rvp.StDateOfReceipt != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата приема заявления") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Rvp.StDateOfReceipt
                               select d;
                    }
                    else if (query.Docs.Rvp.EndDateOfReceipt != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата приема заявления") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Rvp.EndDateOfReceipt
                               select d;
                    }

                    //Дата решения
                    if (query.Docs.Rvp.StDecisionDate != null && query.Docs.Rvp.EndDecisionDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата решения") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Rvp.StDecisionDate && dp.DateValue <= query.Docs.Rvp.EndDecisionDate
                               select d;
                    }
                    else if (query.Docs.Rvp.StDecisionDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата решения") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Rvp.StDecisionDate
                               select d;
                    }
                    else if (query.Docs.Rvp.EndDecisionDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата решения") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Rvp.EndDecisionDate
                               select d;
                    }

                    //Дата печати
                    if (query.Docs.Rvp.StPrintDate != null && query.Docs.Rvp.EndPrintDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата печати") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Rvp.StPrintDate && dp.DateValue <= query.Docs.Rvp.EndPrintDate
                               select d;
                    }
                    else if (query.Docs.Rvp.StPrintDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата печати") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Rvp.StPrintDate
                               select d;
                    }
                    else if (query.Docs.Rvp.EndPrintDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата печати") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Rvp.EndPrintDate
                               select d;
                    }

                    //Дата фактической выдачи
                    if (query.Docs.Rvp.StActualDate != null && query.Docs.Rvp.EndActualDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата фактической выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Rvp.StActualDate && dp.DateValue <= query.Docs.Rvp.EndActualDate
                               select d;
                    }
                    else if (query.Docs.Rvp.StActualDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата фактической выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Rvp.StActualDate
                               select d;
                    }
                    else if (query.Docs.Rvp.EndActualDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата фактической выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Rvp.EndActualDate
                               select d;
                    }
                }

                #endregion

                #region ВНЖ

                if (query.Docs.Vng != null && query.Docs.Vng.IsChecked)
                {
                    docTypes.Add(DocumentType.Residence);

                    if (!string.IsNullOrWhiteSpace(query.Docs.Vng.DocNo))
                    {
                        docs = from d in docs
                               where d.Number.Contains(query.Docs.Vng.DocNo)
                               select d;
                    }

                    if (!string.IsNullOrWhiteSpace(query.Docs.Vng.DocNo2))
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Номер дела") on d.Id equals dp.DocumentId
                               where dp.StringValue.Contains(query.Docs.Vng.DocNo2)
                               select d;
                    }
                    if (!string.IsNullOrWhiteSpace(query.Docs.Vng.VngNo))
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Номер ВНЖ") on d.Id equals dp.DocumentId
                               where dp.StringValue.Contains(query.Docs.Vng.VngNo)
                               select d;
                    }

                    if (query.Docs.Vng.DocActionType != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Тип дела") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Vng.DocActionType
                               select d;
                    }

                    if (query.Docs.Vng.DocAdmission != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Основание дела") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Vng.DocAdmission
                               select d;
                    }

                    if (query.Docs.Vng.DecisionType != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Тип решения") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Vng.DecisionType
                               select d;
                    }

                    if (query.Docs.Vng.Series != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Серия ВНЖ") on d.Id equals dp.DocumentId
                               where dp.IntValue == query.Docs.Vng.Series
                               select d;
                    }

                    //Дата приема заявления
                    if (query.Docs.Vng.StDateOfReceipt != null && query.Docs.Vng.EndDateOfReceipt != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата приема заявления") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Vng.StDateOfReceipt && dp.DateValue <= query.Docs.Vng.EndDateOfReceipt
                               select d;
                    }
                    else if (query.Docs.Vng.StDateOfReceipt != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата приема заявления") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Vng.StDateOfReceipt
                               select d;
                    }
                    else if (query.Docs.Vng.EndDateOfReceipt != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата приема заявления") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Vng.EndDateOfReceipt
                               select d;
                    }

                    //Дата решения
                    if (query.Docs.Vng.StDecisionDate != null && query.Docs.Vng.EndDecisionDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата решения") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Vng.StDecisionDate && dp.DateValue <= query.Docs.Vng.EndDecisionDate
                               select d;
                    }
                    else if (query.Docs.Vng.StDecisionDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата решения") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Vng.StDecisionDate
                               select d;
                    }
                    else if (query.Docs.Vng.EndDecisionDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата решения") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Vng.EndDecisionDate
                               select d;
                    }

                    //Дата выдачи
                    if (query.Docs.Vng.StIssueDate != null && query.Docs.Vng.EndIssueDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Vng.StIssueDate && dp.DateValue <= query.Docs.Vng.EndIssueDate
                               select d;
                    }
                    else if (query.Docs.Vng.StIssueDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Vng.StIssueDate
                               select d;
                    }
                    else if (query.Docs.Vng.EndIssueDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Vng.EndIssueDate
                               select d;
                    }

                    //Дата фактической выдачи
                    if (query.Docs.Vng.StActualDate != null && query.Docs.Vng.EndActualDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата фактической выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Vng.StActualDate && dp.DateValue <= query.Docs.Vng.EndActualDate
                               select d;
                    }
                    else if (query.Docs.Vng.StActualDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата фактической выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue >= query.Docs.Vng.StActualDate
                               select d;
                    }
                    else if (query.Docs.Vng.EndActualDate != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Дата фактической выдачи") on d.Id equals dp.DocumentId
                               where dp.DateValue <= query.Docs.Vng.EndActualDate
                               select d;
                    }

                    //Действителен С
                    if (query.Docs.Vng.ValidFrom != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Действителен С") on d.Id equals dp.DocumentId
                               where dp.DateValue == query.Docs.Vng.ValidFrom
                               select d;
                    }

                    //Действителен ПО
                    if (query.Docs.Vng.ValidTo != null)
                    {
                        docs = from d in docs
                               join dp in GetDocParametersSubQuery("Действителен ПО") on d.Id equals dp.DocumentId
                               where dp.DateValue == query.Docs.Vng.ValidTo
                               select d;
                    }
                }

                #endregion

                foreach (var dt in docTypes)
                {
                    q = from p in q
                        join d in docs on p.Id equals d.ApplicantPersonId ?? d.HostPersonId
                        where d.Type == dt
                        select p;
                }

                q = q.Distinct();
            }

            q = q.OrderBy(p => p.Name);

            var total = q.Count();
            var res = q.Skip(page * limit).Take(limit).ToList();

            return Ok(new { People = res, Total = total, Query = query, Sql = q.ToString() });
        }

        private IQueryable<GroupPersonFact> GetPersonFactsSubQuery(string factName)
        {
            return from pf in _repPersonFacts.GetAll()
                   join pfn in _repPrmFactNames.GetAll() on pf.FactId equals pfn.Id
                   where pfn.NameRu == factName && pfn.Category == PrmFactCategory.Person && pfn.IsFact == true
                   group pf by pf.PersonId into g
                   select new GroupPersonFact { PersonId = g.Key, FactDate = g.Max(e => e.FactDate) };
        }

        private IQueryable<PersonParameter> GetPersonParametersSubQuery(string parameterName)
        {
            return from pp in _repPersonParams.GetAll()
                   join ppn in _repPrmFactNames.GetAll() on pp.ParameterId equals ppn.Id
                   where ppn.NameRu == parameterName && ppn.Category == PrmFactCategory.Person && ppn.IsFact == false
                   select pp;
        }

        private IQueryable<DocumentParameter> GetDocParametersSubQuery(string parameterName)
        {
            return from dp in _repDocParams.GetAll()
                   join dpn in _repPrmFactNames.GetAll() on dp.ParameterId equals dpn.Id
                   where dpn.NameRu == parameterName && dpn.Category == PrmFactCategory.Document && dpn.IsFact == false
                   select dp;
        }
    }


}
