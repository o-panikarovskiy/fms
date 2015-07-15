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
    public class DictionaryController : ApiController
    {
        private readonly IRepository<Misc> _repository;
        private readonly IRepository<MiscName> _repositoryNames;

        public DictionaryController(IRepository<Misc> repository, IRepository<MiscName> repositoryNames)
        {
            _repository = repository;
            _repositoryNames = repositoryNames;
        }

        public IHttpActionResult GetDictionary(string id)
        {
            if (string.Compare(id, "PersonCategory", true) == 0)
            {
                var result = new List<MiscViewModel>(2);
                result.Add(new MiscViewModel { Key = (int)PersonCategory.Individual, Value = "Физическое лицо" });
                result.Add(new MiscViewModel { Key = (int)PersonCategory.Legal, Value = "Юридическое лицо" });
                return Ok(new DictioanryMiscViewModel { Dictionary = result.OrderBy(r => r.Value).ToList() });
            }

            if (string.Compare(id, "PersonType", true) == 0)
            {
                var result = new List<MiscViewModel>(2);
                result.Add(new MiscViewModel { Key = (int)PersonType.Applicant, Value = "Соискатель" });
                result.Add(new MiscViewModel { Key = (int)PersonType.Host, Value = "Принимающая сторона" });
                return Ok(new DictioanryMiscViewModel { Dictionary = result.OrderBy(r => r.Value).ToList() });
            }

            if (string.Compare(id, "DocumentType", true) == 0)
            {
                var result = new List<MiscViewModel>(5);
                result.Add(new MiscViewModel { Key = (int)DocumentType.AdministrativePractice, Value = "Административная практика" });
                result.Add(new MiscViewModel { Key = (int)DocumentType.Citizenship, Value = "Гражданство" });
                result.Add(new MiscViewModel { Key = (int)DocumentType.MigrationRegistration, Value = "Миграционный учёт" });
                result.Add(new MiscViewModel { Key = (int)DocumentType.Residence, Value = "Разрешение на временное проживание" });
                result.Add(new MiscViewModel { Key = (int)DocumentType.TemporaryResidencePermit, Value = "Вид на жительство" });
                return Ok(new DictioanryMiscViewModel { Dictionary = result.OrderBy(r => r.Value).ToList() });
            }



            int dicId;
            IQueryable<MiscViewModel> q;
            if (int.TryParse(id, out dicId))
            {
                q = from m in _repository.GetAll()
                    where m.MiscId == dicId
                    orderby m.MiscValue
                    select new MiscViewModel { Key = m.Id, Value = m.MiscValue };
            }
            else
            {
                q = from m in _repository.GetAll()
                    join mn in _repositoryNames.GetAll() on m.MiscId equals mn.Id
                    where mn.Name == id 
                    orderby m.MiscValue
                    select new MiscViewModel { Key = m.Id, Value = m.MiscValue };
            }

            return Ok(new DictioanryMiscViewModel { Dictionary = q.ToList() });
        }
    }
}
