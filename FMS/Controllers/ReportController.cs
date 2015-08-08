using Domain.Abstract;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FMS.Controllers
{
	public class ReportController : Controller
	{
		private readonly IRepository<Document> _repDocs;
		private readonly IRepository<DocumentParameter> _repDocParams;
		private readonly IRepository<ParameterName> _repDocParamNames;

		public ReportController(IRepository<Document> repDocs, IRepository<DocumentParameter> repDocParams, IRepository<ParameterName> repDocParamNames)
		{
			_repDocParamNames = repDocParamNames;
			_repDocParams = repDocParams;
			_repDocs = repDocs;
        }

		// GET: Report
		public ActionResult Index()
		{
			return View();
		}
	}
}