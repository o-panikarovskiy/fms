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
		private readonly IRepository<Misc> _repository;
		private readonly IRepository<MiscName> _repositoryNames;

		public MiscController(IRepository<Misc> repository, IRepository<MiscName> repositoryNames)
		{
			_repository = repository;
			_repositoryNames = repositoryNames;
		}

		[HttpGet]
		public IEnumerable<MiscName> GetListMiscNames()
		{
			var list = _repositoryNames.GetAll().OrderBy(m => m.Name);
			return list.AsEnumerable();
		}

		[HttpPost]
		public async Task<HttpResponseMessage> CreateMisc(MiscName misc)
		{
			if (!ModelState.IsValid)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			}

			if (_repositoryNames.GetAll().Any(m => m.Name == misc.Name && m.DocType == misc.DocType))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "В этой группе уже используется данное значение.");
			}

			await _repositoryNames.AddAsync(misc);

			return Request.CreateResponse(HttpStatusCode.OK, misc);
		}

		[HttpPut]
		public async Task<HttpResponseMessage> UpdateMisc(MiscName misc)
		{
			if (!ModelState.IsValid)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			}

			var miscDb = await _repositoryNames.FindAsync(m => m.Id == misc.Id);

			if (miscDb == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound);
			}

			miscDb.Name = misc.Name;

			await _repositoryNames.UpdateAsync(miscDb);

			return Request.CreateResponse(HttpStatusCode.NoContent);
		}

		[HttpDelete]
		public async Task<HttpResponseMessage> RemoveMisc(int id)
		{
			var misc = await _repositoryNames.FindAsync(m => m.Id == id);
			if (misc == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound);
			}

			if (_repository.GetAll().Any(m => m.MiscId == id))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Этот словарь нельзя удалить.");
			}

			await _repositoryNames.RemoveAsync(misc);

			return Request.CreateResponse(HttpStatusCode.NoContent);
		}

		[HttpGet]
		[Route("api/misc/{id}/values")]
		public IEnumerable<Misc> GetMiscValues(int id)
		{
			var list = _repository.FindAll(m => m.MiscId == id).OrderBy(m => m.MiscValue);
			return list.AsEnumerable();
		}
	}
}
