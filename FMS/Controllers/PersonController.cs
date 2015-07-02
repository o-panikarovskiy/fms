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
    //[Authorize]
    public class PersonController : ApiController
    {
        private readonly IRepository<Person> _repPeople;
        private readonly IRepository<PersonFact> _repPersonFacts;
        private readonly IRepository<PrmFactName> _repParameterFactNames;
        private readonly IRepository<PersonParameter> _repPersonParams;
        private readonly IRepository<Misc> _repMisc;
        private readonly IRepository<Document> _repDocs;
        private readonly IRepository<DocumentParameter> _repDocParams;
        public PersonController(IRepository<Person> repPerople, IRepository<PersonFact> repPersonFacts, IRepository<PersonParameter> repPersonParams, IRepository<PrmFactName> repParameterFactNames,
            IRepository<Misc> repMisc, IRepository<Document> repDocs, IRepository<DocumentParameter> repDocParams)
        {
            _repPeople = repPerople;
            _repPersonFacts = repPersonFacts;
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

            pvm.Facts = (from pf in _repPersonFacts.GetAll()
                         join pfn in _repParameterFactNames.GetAll() on pf.FactId equals pfn.Id
                         join pfg in (from pf in _repPersonFacts.GetAll()
                                      where pf.PersonId == person.Id
                                      group pf by pf.FactId into g
                                      select new GroupPersonFact { FactId = g.Key, FactDate = g.Max(e => e.FactDate) }) on pf.FactId equals pfg.FactId
                         join m in _repMisc.GetAll() on pf.IntValue equals m.Id into mlj
                         from x in mlj.DefaultIfEmpty()
                         where pf.FactDate == pfg.FactDate && pf.PersonId == id
                         select new FactViewModel
                         {
                             Id = pf.Id,
                             PrmId = pf.FactId,
                             FactDate = pf.FactDate,
                             Name = pfn.Name,
                             NameRu = pfn.NameRu,
                             MiscId = pf.IntValue,
                             MiscValue = x.MiscValue,
                             DicId = x.MiscId,
                             StringValue = pf.StringValue,
                             DateValue = pf.DateValue,
                             FloatValue = pf.FloatValue,
                             PrmCategory = pfn.Category,
                             PrmType = pfn.Type
                         }).ToDictionary(k => k.Name, v => v);

            pvm.Parameters = (from pp in _repPersonParams.GetAll()
                              join ppn in _repParameterFactNames.GetAll() on pp.ParameterId equals ppn.Id
                              join m in _repMisc.GetAll() on pp.IntValue equals m.Id into mlj
                              from x in mlj.DefaultIfEmpty()
                              where pp.PersonId == id
                              select new ParameterViewModel
                              {
                                  Id = pp.Id,
                                  PrmId = pp.ParameterId,
                                  Name = ppn.Name,
                                  NameRu = ppn.NameRu,
                                  MiscId = pp.IntValue,
                                  MiscValue = x.MiscValue,
                                  DicId = x.MiscId,
                                  StringValue = pp.StringValue,
                                  FloatValue = pp.FloatValue,
                                  DateValue = pp.DateValue,
                                  PrmCategory = ppn.Category,
                                  PrmType = ppn.Type
                              }).ToDictionary(k => k.Name, v => v);

            pvm.DocsCount.Add(DocumentType.AdministrativePractice.ToString(),
                _repDocs.GetAll().Count(d => d.Type == DocumentType.AdministrativePractice && (d.HostPersonId == id || d.ApplicantPersonId == id)));
            pvm.DocsCount.Add(DocumentType.Citizenship.ToString(),
                _repDocs.GetAll().Count(d => d.Type == DocumentType.Citizenship && (d.HostPersonId == id || d.ApplicantPersonId == id)));

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

            person.Birthday = pvm.Birthday;
            person.Category = pvm.Category;
            person.Type = pvm.Type;
            person.Name = pvm.Name;

            await _repPeople.UpdateAsync(person);

            UpdatePersonParams(person, pvm.Parameters);
            UpdatePersonFacts(person, pvm.Facts);

            return Request.CreateResponse(HttpStatusCode.NoContent);
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
                                 Number = d.Number,
                                 Type = d.Type,
                                 CreatedDate = d.CreatedDate,
                                 CreatedBy = d.CreatedBy.Name ?? d.CreatedBy.UserName,
                                 UpdatedDate = d.UpdatedDate,
                                 UpdatedBy = d.UpdatedBy.Name ?? d.UpdatedBy.UserName
                             }).ToList();

            foreach (var d in documents)
            {
                d.Parameters = (from dp in _repDocParams.GetAll()
                                join dpn in _repParameterFactNames.GetAll() on dp.ParameterId equals dpn.Id
                                join m in _repMisc.GetAll() on dp.IntValue equals m.Id into mlj
                                from x in mlj.DefaultIfEmpty()
                                where dp.DocumentId == d.Id
                                select new ParameterViewModel
                                {
                                    Id = dp.Id,
                                    PrmId = dp.ParameterId,
                                    Name = dpn.Name,
                                    NameRu = dpn.NameRu,
                                    StringValue = dp.StringValue,
                                    FloatValue = dp.FloatValue,
                                    DateValue = dp.DateValue,
                                    MiscId = dp.IntValue,
                                    MiscValue = x.MiscValue,
                                    DicId = x.MiscId,
                                    PrmCategory = dpn.Category,
                                    PrmType = dpn.Type
                                }).ToDictionary(k => k.Name, v => v);
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
                        join m in _repMisc.GetAll() on pf.IntValue equals m.Id into mlj
                        join pfn in _repParameterFactNames.GetAll() on pf.FactId equals pfn.Id
                        from x in mlj.DefaultIfEmpty()
                        where pf.PersonId == personId && pf.FactId == factId
                        orderby pf.FactDate descending
                        select new FactViewModel
                        {
                            Id = pf.Id,
                            PrmId = pf.FactId,
                            FactDate = pf.FactDate,
                            Name = pfn.Name,
                            NameRu = pfn.NameRu,
                            MiscId = pf.IntValue,
                            MiscValue = x.MiscValue,
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
            var names = _repParameterFactNames.FindAll(p => p.Category == PrmFactCategory.Person && p.IsFact == false).ToList();
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
            var names = _repParameterFactNames.FindAll(p => p.Category == PrmFactCategory.Person && p.IsFact == true).ToList();
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
