using Domain.Abstract;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace FMS.Controllers
{
    public class ReportsController : ApiController
    {
		private readonly IReportRepository<DocumentOnControl> _rep;
		public ReportsController(IReportRepository<DocumentOnControl> rep)
		{
			_rep = rep;
		}

		[HttpGet]
		public async Task<IHttpActionResult> DocumentsOnControl()
		{
			var result = await _rep.ExecuteStoredProcedure("DocumentsOnControl");		
			return Ok(result);
		}
	}
}
