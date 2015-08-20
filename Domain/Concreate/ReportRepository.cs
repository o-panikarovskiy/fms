using Domain.Abstract;
using Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Concreate
{
	public class ReportRepository<T> : IReportRepository<T> where T : class
	{
		private readonly ApplicationDbContext _dbContext;

		public ReportRepository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IList<T>> ExecuteStoredProcedure(string name, params object[] parameters)
		{
			return await _dbContext.Database.SqlQuery<T>(name, parameters).ToListAsync();
		}
	}
}
