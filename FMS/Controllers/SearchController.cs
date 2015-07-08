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

        public SearchController(IRepository<Person> repPerople, IRepository<Document> repDocuments,
            IRepository<SearchQuery> repQueries, IRepository<PrmFactName> repPrmFactNames, IRepository<PersonFact> repPersonFacts, IRepository<PersonParameter> repPersonParams)
        {
            _repPeople = repPerople;
            _repQueries = repQueries;
            _repPersonFacts = repPersonFacts;
            _repPrmFactNames = repPrmFactNames;
            _repPersonParams = repPersonParams;
            _repDocuments = repDocuments;
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

            if (query.Docs != null)
            {
                if (query.Docs.Ap != null && query.Docs.Ap.IsChecked)
                {
                    q = (from p in q
                         join d in _repDocuments.GetAll() on p.Id equals d.ApplicantPersonId ?? d.HostPersonId
                         where d.Type == DocumentType.AdministrativePractice
                         select p).Distinct();

                    if (!string.IsNullOrWhiteSpace(query.Docs.Ap.DocNo))
                    {
                        q = from p in q
                            join d in _repDocuments.GetAll() on p.Id equals d.ApplicantPersonId ?? d.HostPersonId
                            where d.Number.Contains(query.Docs.Ap.DocNo)
                            select p;
                    }
                }
            }

            q = q.OrderBy(p => p.Name);

            var total = q.Count();
            var res = q.Skip(page * limit).Take(limit).ToList();

            return Ok(new { People = res, Total = total, Query = query });
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
    }


}
