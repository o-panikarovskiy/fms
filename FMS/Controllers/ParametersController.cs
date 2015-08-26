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
	public class ParametersController : ApiController
	{
		private readonly IRepository<Misc> _repMisc;
		private readonly IRepository<ParameterName> _repParamNames;
		private readonly IRepository<DocumentParameter> _repDocParams;		

		public ParametersController(IRepository<Misc> repMisc, IRepository<ParameterName> repParamNames, IRepository<DocumentParameter> repDocParams)
		{
			_repMisc = repMisc;
			_repParamNames = repParamNames;
			_repDocParams = repDocParams;
		}

		[HttpGet]
		public IEnumerable<ParameterName> GetList()
		{
			var list = _repParamNames.GetAll().Where(p => p.Category == ParameterCategory.Document).OrderBy(m => m.OrderIndex);
			return list.ToList();
		}

		[HttpPost]
		public async Task<HttpResponseMessage> Create(ParameterName param)
		{
			if (!ModelState.IsValid)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			}

			if (param.Type == ParameterType.Misc && param.MiscParentId == null)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Для типа данных \"словарь\" необходимо выбрать словарь.");
			}

			if (_repParamNames.GetAll().Any(p => p.Name == param.Name && p.DocType == param.DocType))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "В этом типе документов уже используется данный параметр.");
			}


			param.Category = ParameterCategory.Document;
			param.PersonCategory = null;
			param.IsFact = false;
			param.CanRemove = true;

			await _repParamNames.AddAsync(param);

			return Request.CreateResponse(HttpStatusCode.OK, param);
		}

		[HttpPut]
		public async Task<HttpResponseMessage> Update(ParameterName param)
		{
			if (!ModelState.IsValid)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
			}

			var paramDb = await _repParamNames.FindAsync(p => p.Id == param.Id);

			if (paramDb == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound);
			}

			paramDb.OrderIndex = param.OrderIndex;
			await _repParamNames.UpdateAsync(paramDb);



			return Request.CreateResponse(HttpStatusCode.NoContent);
		}


		[HttpDelete]
		public async Task<HttpResponseMessage> Remove(int id)
		{
			var param = await _repParamNames.FindAsync(p => p.Id == id);
			if (param == null)
			{
				return Request.CreateResponse(HttpStatusCode.NotFound);
			}

			if (!param.CanRemove)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Этот параметр используется при импорте данных, его нельзя удалить.");
			}

			if (_repDocParams.GetAll().Any(d => d.ParameterId == id))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Этот параметр используется в документах, его нельзя удалить.");
			}

			await _repParamNames.RemoveAsync(param);

			return Request.CreateResponse(HttpStatusCode.NoContent);
		}


	}
}
