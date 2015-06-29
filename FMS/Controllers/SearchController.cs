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
        private readonly IRepository<PrmFactName> _repPersonFactNames;
        private readonly IRepository<PersonFact> _repPersonFacts;

        public SearchController(IRepository<Person> repPerople, IRepository<SearchQuery> repQueries, IRepository<PrmFactName> repPersonFactNames,
             IRepository<PersonFact> repPersonFacts)
        {
            _repPeople = repPerople;
            _repQueries = repQueries;
            _repPersonFacts = repPersonFacts;
            _repPersonFactNames = repPersonFactNames;
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
                    q = q.Where(p => p.Name.Contains(query.Person.Name));
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
                        join pfn in _repPersonFactNames.GetAll() on pf.FactId equals pfn.Id
                        join pfg in GetPersonFactsSubQuery("Гражданство") on pf.PersonId equals pfg.PersonId
                        where pfn.NameRu == "Гражданство" && pf.FactDate == pfg.FactDate && pf.IntValue == query.Person.Citizenship
                        select p;
                }

                if (!string.IsNullOrWhiteSpace(query.Person.Address))
                {
                    q = from p in q
                        join pf in _repPersonFacts.GetAll() on p.Id equals pf.PersonId
                        join pfn in _repPersonFactNames.GetAll() on pf.FactId equals pfn.Id
                        join pfg in GetPersonFactsSubQuery("Адрес") on pf.PersonId equals pfg.PersonId
                        where pfn.NameRu == "Адрес" && pf.FactDate == pfg.FactDate && pf.StringValue.Contains(query.Person.Address)
                        select p;
                }

                if (query.Person.DocType != null)
                {
                    q = from p in q
                        join pf in _repPersonFacts.GetAll() on p.Id equals pf.PersonId
                        join pfn in _repPersonFactNames.GetAll() on pf.FactId equals pfn.Id
                        join pfg in GetPersonFactsSubQuery("Тип документа") on pf.PersonId equals pfg.PersonId
                        where pfn.NameRu == "Тип документа" && pf.FactDate == pfg.FactDate && pf.IntValue == query.Person.DocType
                        select p;
                }

                if (!string.IsNullOrWhiteSpace(query.Person.DocNo))
                {
                    q = from p in q
                        join pf in _repPersonFacts.GetAll() on p.Id equals pf.PersonId
                        join pfn in _repPersonFactNames.GetAll() on pf.FactId equals pfn.Id
                        join pfg in GetPersonFactsSubQuery("Тип документа") on pf.PersonId equals pfg.PersonId
                        where pfn.NameRu == "Тип документа" && pf.FactDate == pfg.FactDate && pf.StringValue.Contains(query.Person.DocNo)
                        select p;
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
                   join pfn in _repPersonFactNames.GetAll() on pf.FactId equals pfn.Id
                   where pfn.NameRu == factName
                   group pf by pf.PersonId into g
                   select new GroupPersonFact { PersonId = g.Key, FactDate = g.Max(e => e.FactDate) };
        }
    }


}
