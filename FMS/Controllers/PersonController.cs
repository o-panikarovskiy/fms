using Domain.Abstract;
using Domain.Models;
using FMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace FMS.Controllers
{
    [Authorize]
    public class PersonController : ApiController
    {
        private readonly IRepository<Person> _repPeople;
        private readonly IRepository<PersonFact> _repPersonFacts;
        private readonly IRepository<ParameterName> _repParameterFactNames;
        private readonly IRepository<PersonParameter> _repPersonParams;
        private readonly IRepository<MiscName> _repMiscNames;
        private readonly IRepository<Misc> _repMisc;
        private readonly IRepository<Document> _repDocs;
        private readonly IRepository<DocumentParameter> _repDocParams;
        public PersonController(IRepository<Person> repPerople, IRepository<PersonFact> repPersonFacts, IRepository<PersonParameter> repPersonParams, IRepository<ParameterName> repParameterFactNames,
            IRepository<MiscName> repMiscNames, IRepository<Misc> repMisc, IRepository<Document> repDocs, IRepository<DocumentParameter> repDocParams)
        {
            _repPeople = repPerople;
            _repPersonFacts = repPersonFacts;
            _repMiscNames = repMiscNames;
            _repMisc = repMisc;
            _repPersonParams = repPersonParams;
            _repDocs = repDocs;
            _repDocParams = repDocParams;
            _repParameterFactNames = repParameterFactNames;
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetPerson(int id)
        {
            var person = await _repPeople.FindAsync(p => p.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            var pvm = new PersonViewModel(person);

            pvm.Facts = (from fn in _repParameterFactNames.GetAll()
                         join f in (from pf in _repPersonFacts.GetAll()
                                    join pfg in (from pf in _repPersonFacts.GetAll()
                                                 where pf.PersonId == person.Id
                                                 group pf by pf.FactId into g
                                                 select new GroupPersonFact { FactId = g.Key, FactDate = g.Max(e => e.FactDate) }) on pf.FactId equals pfg.FactId
                                    where pf.FactDate == pfg.FactDate && pf.PersonId == id
                                    select pf) on fn.Id equals f.FactId into ft
                         from fl in ft.DefaultIfEmpty()
                         where (fn.PersonCategory & person.Category) > 0 && fn.IsFact == true && fn.Category == ParameterCategory.Person
                         select new FactViewModel
                         {
                             Id = fl.Id,
                             PrmId = fn.Id,
                             FactDate = fl.FactDate,
                             Name = fn.Name,
                             MiscId = fl.IntValue,
                             DicId = fn.MiscParentId,
                             StringValue = fl.StringValue,
                             DateValue = fl.DateValue,
                             FloatValue = fl.FloatValue,
                             PrmCategory = fn.Category,
                             PrmType = fn.Type
                         }).ToDictionary(k => k.Name, v => v);

            pvm.Parameters = (from pn in _repParameterFactNames.GetAll()
                              join pp in _repPersonParams.GetAll().Where(p => p.PersonId == id) on pn.Id equals pp.ParameterId into pt
                              from pl in pt.DefaultIfEmpty()
                              where (pn.PersonCategory & person.Category) > 0 && pn.IsFact == false && pn.Category == ParameterCategory.Person
                              select new ParameterViewModel
                              {
                                  Id = pl.Id,
                                  PrmId = pn.Id,
                                  Name = pn.Name,
                                  MiscId = pl.IntValue,
                                  DicId = pn.MiscParentId,
                                  StringValue = pl.StringValue,
                                  FloatValue = pl.FloatValue,
                                  DateValue = pl.DateValue,
                                  PrmCategory = pn.Category,
                                  PrmType = pn.Type
                              }).ToDictionary(k => k.Name, v => v);

            pvm.DocsCount = (from d in _repDocs.GetAll()
                             where d.HostPersonId == id || d.ApplicantPersonId == id
                             group d by d.Type into g
                             select new { Type = g.Key, Count = g.Count() }).ToDictionary(k => k.Type.ToString(), v => v.Count);

            return Ok(pvm);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> UpdatePerson(int id, [FromBody] PersonViewModel pvm)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            var person = await _repPeople.FindAsync(p => p.Id == id);

            if (person == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (_repPeople.GetAll().Any(pr => (pr.Name == pvm.Name && pr.Birthday == pvm.Birthday &&
               pr.Category == PersonCategory.Individual) || (pr.Code == pvm.Code && pr.Name == pvm.Name && pr.Category == PersonCategory.Legal)))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    new ArgumentException("Анкета с такими данными уже существует"));
            }

            person.Birthday = pvm.Birthday;
            person.Code = pvm.Code;
            person.Category = pvm.Category;
            person.Type = pvm.Type;
            person.Name = pvm.Name;

            await _repPeople.UpdateAsync(person);

            UpdatePersonParams(person, pvm.Parameters);
            UpdatePersonFacts(person, pvm.Facts);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> CreatePerson([FromBody] Person p)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (_repPeople.GetAll().Any(pr => (pr.Name == p.Name && pr.Birthday == p.Birthday &&
                pr.Category == PersonCategory.Individual) || (pr.Code == p.Code && pr.Name == p.Name && pr.Category == PersonCategory.Legal)))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    new ArgumentException("Анкета с такими данными уже существует"));
            }

            var person = new Person
            {
                Name = p.Name,
                Birthday = p.Birthday,
                Code = p.Code,
                Type = p.Type,
                Category = p.Category
            };

            await _repPeople.AddAsync(person);
            return Request.CreateResponse(HttpStatusCode.OK, person);
        }

        [HttpGet]
        [Route("api/person/{personId}/document/{docType}")]
        public async Task<IHttpActionResult> GetPersonDocuments(int personId, DocumentType docType)
        {
            var person = await _repPeople.FindAsync(p => p.Id == personId);
            if (person == null)
            {
                return NotFound();
            }

            var documents = (from d in _repDocs.GetAll()
                             where d.Type == docType && (d.HostPersonId == personId || d.ApplicantPersonId == personId)
                             orderby d.CreatedDate descending
                             select new DocumentViewModel
                             {
                                 Id = d.Id,
                                 CorrPerson = d.ApplicantPersonId == personId ? d.HostPerson : d.ApplicantPerson,
                                 Number = d.Number,
                                 Type = d.Type,
                                 CreatedDate = d.CreatedDate,
                                 CreatedBy = d.CreatedBy.Name ?? d.CreatedBy.UserName,
                                 UpdatedDate = d.UpdatedDate,
                                 UpdatedBy = d.UpdatedBy.Name ?? d.UpdatedBy.UserName
                             }).ToList();

            foreach (var d in documents)
            {
                d.Parameters = (from pn in _repParameterFactNames.GetAll()
                                join dp in _repDocParams.GetAll().Where(dc => dc.DocumentId == d.Id) on pn.Id equals dp.ParameterId into dt
                                from dj in dt.DefaultIfEmpty()
                                where pn.DocType == d.Type && pn.IsFact == false && pn.Category == ParameterCategory.Document
                                orderby pn.OrderIndex
                                select new ParameterViewModel
                                {
                                    Id = dj.Id,
                                    PrmId = pn.Id,
                                    Name = pn.Name,
                                    StringValue = dj.StringValue,
                                    FloatValue = dj.FloatValue,
                                    DateValue = dj.DateValue,
                                    MiscId = dj.IntValue,
                                    DicId = pn.MiscParentId,
                                    PrmCategory = pn.Category,
                                    PrmType = pn.Type
                                }).ToList();
            };

            return Ok(new { Documents = documents });
        }

        [HttpGet]
        [Route("api/person/{personId}/fact/{factId}")]
        public async Task<IHttpActionResult> GetPersonFactsHistory(int personId, int factId)
        {
            var person = await _repPeople.FindAsync(p => p.Id == personId);
            if (person == null)
            {
                return NotFound();
            }

            var list = (from pf in _repPersonFacts.GetAll()
                        join pfn in _repParameterFactNames.GetAll() on pf.FactId equals pfn.Id
                        join m in _repMisc.GetAll() on pf.IntValue equals m.Id into mt
                        from ml in mt.DefaultIfEmpty()
                        where pf.PersonId == personId && pf.FactId == factId
                        orderby pf.FactDate descending
                        select new FactViewModel
                        {
                            Id = pf.Id,
                            PrmId = pf.FactId,
                            FactDate = pf.FactDate,
                            Name = pfn.Name,
                            MiscId = pf.IntValue,
                            MiscValue = ml.MiscValue,
                            DicId = pfn.MiscParentId,
                            StringValue = pf.StringValue,
                            DateValue = pf.DateValue,
                            FloatValue = pf.FloatValue,
                            PrmCategory = pfn.Category,
                            PrmType = pfn.Type
                        }).Skip(1).ToList();

            return Ok(new { Facts = list });
        }

        private void UpdatePersonParams(Person person, IDictionary<string, ParameterViewModel> parameters)
        {
            var names = _repParameterFactNames.FindAll(p => p.Category == ParameterCategory.Person && p.IsFact == false).ToList();
            var allPrms = _repPersonParams.FindAll(p => p.PersonId == person.Id).ToList();

            var list = new List<PersonParameter>();
            foreach (var key in parameters.Keys)
            {
                var prm = parameters[key];

                var pn = names.Single(p => string.Compare(p.Name, key, true) == 0);
                var dbParam = allPrms.SingleOrDefault(p => p.ParameterId == pn.Id);

                if (dbParam != null)
                {
                    dbParam.DateValue = prm.DateValue;
                    dbParam.FloatValue = prm.FloatValue;
                    dbParam.IntValue = prm.MiscId;
                    dbParam.StringValue = prm.StringValue;
                }
                else
                {
                    dbParam = new PersonParameter
                    {
                        ParameterId = pn.Id,
                        PersonId = person.Id,
                        DateValue = prm.DateValue,
                        FloatValue = prm.FloatValue,
                        IntValue = prm.MiscId,
                        StringValue = prm.StringValue
                    };
                }

                list.Add(dbParam);
            };

            _repPersonParams.AddRange(list.Where(p => p.Id == 0).AsEnumerable());
            _repPersonParams.UpdateRange(list.Where(p => p.Id != 0).AsEnumerable());
        }

        private void UpdatePersonFacts(Person person, IDictionary<string, FactViewModel> facts)
        {
            var names = _repParameterFactNames.FindAll(p => p.Category == ParameterCategory.Person && p.IsFact == true).ToList();
            var allFacts = _repPersonFacts.FindAll(p => p.PersonId == person.Id).ToList();

            var list = new List<PersonFact>();
            foreach (var key in facts.Keys)
            {
                var fact = facts[key];

                var fn = names.Single(f => string.Compare(f.Name, key, true) == 0);
                var dbFact = allFacts.Where(p => p.FactId == fn.Id).OrderByDescending(f => f.FactDate).FirstOrDefault();

                if (dbFact == null || dbFact.FloatValue != fact.FloatValue || dbFact.IntValue != fact.MiscId || dbFact.StringValue != fact.StringValue || dbFact.DateValue != fact.DateValue)
                {
                    dbFact = new PersonFact
                    {
                        FactDate = DateTime.Now,
                        FactId = fn.Id,
                        PersonId = person.Id,
                        DateValue = fact.DateValue,
                        FloatValue = fact.FloatValue,
                        IntValue = fact.MiscId,
                        StringValue = fact.StringValue
                    };
                    list.Add(dbFact);
                }
            };

            _repPersonFacts.AddRange(list);
        }
    }

}
