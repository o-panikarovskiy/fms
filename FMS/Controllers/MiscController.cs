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
	[Authorize(Roles = "Admin")]
	public class MiscController : ApiController
	{
		private readonly IRepository<Misc> _repMisc;
		private readonly IRepository<MiscName> _repMiscNames;
		private readonly IRepository<PersonParameter> _repPersonParams;
		private readonly IRepository<PersonFact> _repPersonFacts;
		private readonly IRepository<DocumentParameter> _repDocParams;

		public MiscController(IRepository<Misc> repMisc,
			IRepository<MiscName> repMiscNames,
			IRepository<PersonParameter> repPersonParams,
			IRepository<PersonFact> repPersonFacts,
			IRepository<DocumentParameter> repDocParams)
		{
			_repMisc = repMisc;
			_repMiscNames = repMiscNames;
			_repPersonParams = repPersonParams;
			_repPersonFacts = repPersonFacts;
			_repDocParams = repDocParams;
		}

		[HttpGet]
		public IEnumerable<MiscName> GetListMiscNames()
		{
			var list = _repMiscNames.GetAll().OrderBy(m => m.Name);
			return list.AsEnumerable();
		}

		[HttpPost]
		public async Task<HttpResponseMessage> CreateMisc(MiscName misc)
		{
			if (!ModelState.IsValid)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			}

			if (_repMiscNames.GetAll().Any(m => m.Name == misc.Name && m.DocType == misc.DocType))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "В этой группе уже используется данное значение.");
			}

			await _repMiscNames.AddAsync(misc);

			return Request.CreateResponse(HttpStatusCode.OK, misc);
		}

		[HttpPut]
		public async Task<HttpResponseMessage> UpdateMisc(MiscName misc)
		{
			if (!ModelState.IsValid)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			}

			if (_repMisc.GetAll().Any(m => m.MiscId == misc.Id))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Этот словарь содержит значения, его нельзя переименовать.");
			}

			if (_repMiscNames.GetAll().Any(m => m.Name == misc.Name))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Эта группа словарей уже содержит словарь с таким названием.");
			}

			var miscDb = await _repMiscNames.FindAsync(m => m.Id == misc.Id);

			if (miscDb == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound);
			}

			miscDb.Name = misc.Name;

			await _repMiscNames.UpdateAsync(miscDb);

			return Request.CreateResponse(HttpStatusCode.NoContent);
		}

		[HttpDelete]
		public async Task<HttpResponseMessage> RemoveMisc(int id)
		{
			var misc = await _repMiscNames.FindAsync(m => m.Id == id);
			if (misc == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound);
			}

			if (_repMisc.GetAll().Any(m => m.MiscId == id))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Этот словарь содержит значения, его нельзя удалить.");
			}

			await _repMiscNames.RemoveAsync(misc);

			return Request.CreateResponse(HttpStatusCode.NoContent);
		}

		[HttpGet]
		[Route("api/misc/{id}/values")]
		public IEnumerable<Misc> GetMiscValues(int id)
		{
			var list = _repMisc.FindAll(m => m.MiscId == id).OrderBy(m => m.MiscValue);
			return list.AsEnumerable();
		}

		[HttpPost]
		[Route("api/misc/{id}/values")]
		public async Task<HttpResponseMessage> CreateMiscValue(Misc misc)
		{
			if (!ModelState.IsValid)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			}

			if (!_repMiscNames.GetAll().Any(m => m.Id == misc.MiscId))
			{
				return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Родительский словарь не найден.");
			}

			if (_repMisc.GetAll().Any(m => m.MiscValue == misc.MiscValue && m.MiscId == misc.MiscId))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "В этом словаре уже используется данное значение.");
			}

			await _repMisc.AddAsync(misc);

			return Request.CreateResponse(HttpStatusCode.OK, misc);
		}

		[HttpPut]
		[Route("api/misc/{id}/values")]
		public async Task<HttpResponseMessage> UpdateMiscValue(Misc misc)
		{
			if (!ModelState.IsValid)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			}

			if (_repMisc.GetAll().Any(m => m.MiscValue == misc.MiscValue && m.MiscId == misc.MiscId))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "В этом словаре уже используется данное значение.");
			}

			var miscDb = await _repMisc.FindAsync(m => m.Id == misc.Id);

			if (miscDb == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound);
			}

			miscDb.MiscValue = misc.MiscValue;

			await _repMisc.UpdateAsync(miscDb);

			return Request.CreateResponse(HttpStatusCode.NoContent);
		}

		[HttpDelete]
		[Route("api/misc/{id}/values/{miscId}")]
		public async Task<HttpResponseMessage> RemoveMiscValue(int miscId)
		{
			var misc = await _repMisc.FindAsync(m => m.Id == miscId);
			if (misc == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound);
			}

			if (_repPersonParams.GetAll().Any(p => p.IntValue == misc.Id && p.Parameter.MiscParentId == misc.MiscId) ||
				_repPersonFacts.GetAll().Any(p => p.IntValue == misc.Id && p.Fact.MiscParentId == misc.MiscId) ||
				_repDocParams.GetAll().Any(d => d.IntValue == misc.Id && d.Parameter.MiscParentId == misc.MiscId))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Это значение используется в параметрах документов, его нельзя удалить.");
			}

			await _repMisc.RemoveAsync(misc);

			return Request.CreateResponse(HttpStatusCode.NoContent);
		}
	}
}
