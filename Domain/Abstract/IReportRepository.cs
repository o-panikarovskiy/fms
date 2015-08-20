using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IReportRepository<T> where T : class
    {
		Task<IList<T>> ExecuteStoredProcedure(string name, params object[] parameters);
    }
}