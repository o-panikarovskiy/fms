using Domain.Abstract;
using Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using FMS.Models;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace FMS.Controllers
{
	[Authorize]
	public class SearchController : ApiController
	{
		private readonly IRepository<Person> _repPeople;
		private readonly IRepository<SearchQuery> _repQueries;
		private readonly IRepository<ParameterName> _repPrmFactNames;
		private readonly IRepository<PersonFact> _repPersonFacts;
		private readonly IRepository<PersonParameter> _repPersonParams;
		private readonly IRepository<Document> _repDocuments;
		private readonly IRepository<DocumentParameter> _repDocParams;

		public SearchController(IRepository<Person> repPerople, IRepository<Document> repDocuments,
			IRepository<SearchQuery> repQueries, IRepository<ParameterName> repPrmFactNames, IRepository<PersonFact> repPersonFacts,
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

				if (query.Person.StBirthday != null)
				{
					q = q.Where(p => p.Birthday >= query.Person.StBirthday);
				}

				if (query.Person.EndBirthday != null)
				{
					q = q.Where(p => p.Birthday <= query.Person.EndBirthday);
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
						join pfg in GetPersonFactsSubQuery("Личный документ") on pf.PersonId equals pfg.PersonId
						where pf.FactDate == pfg.FactDate && pf.IntValue == query.Person.DocType
						select p;
				}

				if (!string.IsNullOrWhiteSpace(query.Person.DocNo))
				{
					q = from p in q
						join pf in _repPersonFacts.GetAll() on p.Id equals pf.PersonId
						join pfg in GetPersonFactsSubQuery("Личный документ") on pf.PersonId equals pfg.PersonId
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

			if (query.Docs.Values.Any(v => v.IsChecked))
			{
				foreach (var sdoc in query.Docs.Where(kp => kp.Value.IsChecked))
				{
					var docs = from d in _repDocuments.GetAll() where d.Type == sdoc.Key select d;
					if (!string.IsNullOrWhiteSpace(sdoc.Value.DocNo))
					{
						docs = from d in docs where d.Number.Contains(sdoc.Value.DocNo) select d;
					}

					foreach (var param in sdoc.Value.DocParams)
					{
						if (param.Value.Type == ParameterType.Date)
						{
							if (param.Value.Start != null)
							{
								docs = from d in docs
									   join dp in _repDocParams.GetAll() on d.Id equals dp.DocumentId
									   where dp.ParameterId == param.Key && dp.DateValue >= param.Value.Start
									   select d;
							}
							if (param.Value.End != null)
							{
								docs = from d in docs
									   join dp in _repDocParams.GetAll() on d.Id equals dp.DocumentId
									   where dp.ParameterId == param.Key && dp.DateValue <= param.Value.End
									   select d;
							}
						}
						else if (param.Value.Type == ParameterType.Str)
						{
							var str = param.Value.Value as string;
							if (!string.IsNullOrWhiteSpace(param.Value.Value as string))
							{
								docs = from d in docs
									   join dp in _repDocParams.GetAll() on d.Id equals dp.DocumentId
									   where dp.ParameterId == param.Key && dp.StringValue.Contains(str)
									   select d;
							}
						}
						else if (param.Value.Type == ParameterType.Misc && param.Value.Value != null)
						{
							int i = 0;
							if (int.TryParse(param.Value.Value.ToString(), out i))
							{
								docs = from d in docs
									   join dp in _repDocParams.GetAll() on d.Id equals dp.DocumentId
									   where dp.ParameterId == param.Key && dp.IntValue == i
									   select d;
							}
						}
						else if (param.Value.Type == ParameterType.Float && param.Value.Value != null)
						{
							float f = 0.0f;
							if (float.TryParse(param.Value.Value.ToString(), out f))
							{
								docs = from d in docs
									   join dp in _repDocParams.GetAll() on d.Id equals dp.DocumentId
									   where dp.ParameterId == param.Key && dp.FloatValue == f
									   select d;
							}
						}
					}

					q = from p in q
						join d in docs on p.Id equals d.ApplicantPersonId ?? d.HostPersonId
						select p;
				}
				q = q.Distinct();
			}

			q = q.OrderBy(p => p.Name);

			var total = q.Count();
			var res = q.Skip(page * limit).Take(limit).ToList();

			return Ok(new { People = res, Total = total, Query = query });
		}

		[HttpGet]
		[Route("api/search/people/{name}")]
		public IHttpActionResult GetByName(string name, [FromUri] PersonType? type = null)
		{
			var query = _repPeople.GetAll().Where(p => p.Name.Contains(name));

			if (type != null)
			{
				query = query.Where(p => p.Type == type);
			}

			var res = query.OrderBy(p => p.Name).Take(20).ToList();
			return Ok(res);
		}

		private IQueryable<GroupPersonFact> GetPersonFactsSubQuery(string factName)
		{
			return from pf in _repPersonFacts.GetAll()
				   join pfn in _repPrmFactNames.GetAll() on pf.FactId equals pfn.Id
				   where pfn.Name == factName && pfn.Category == ParameterCategory.Person && pfn.IsFact == true
				   group pf by pf.PersonId into g
				   select new GroupPersonFact { PersonId = g.Key, FactDate = g.Max(e => e.FactDate) };
		}

		private IQueryable<PersonParameter> GetPersonParametersSubQuery(string parameterName)
		{
			return from pp in _repPersonParams.GetAll()
				   join ppn in _repPrmFactNames.GetAll() on pp.ParameterId equals ppn.Id
				   where ppn.Name == parameterName && ppn.Category == ParameterCategory.Person && ppn.IsFact == false
				   select pp;
		}

		private IQueryable<DocumentParameter> GetDocParametersSubQuery(string parameterName)
		{
			return from dp in _repDocParams.GetAll()
				   join dpn in _repPrmFactNames.GetAll() on dp.ParameterId equals dpn.Id
				   where dpn.Name == parameterName && dpn.Category == ParameterCategory.Document && dpn.IsFact == false
				   select dp;
		}
	}


}
