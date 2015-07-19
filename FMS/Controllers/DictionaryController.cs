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

        [HttpPost]
        public IHttpActionResult GetDictionary([FromBody] DictioanaryBindModel query)
        {
            if (string.Compare(query.Name, "PersonCategory", true) == 0)
            {
                var result = new List<MiscEnumViewModel>(2);
                result.Add(new MiscEnumViewModel { Key = PersonCategory.Individual, Value = "Физическое лицо" });
                result.Add(new MiscEnumViewModel { Key = PersonCategory.Legal, Value = "Юридическое лицо" });
                return Ok(new DictioanaryMiscEnumViewModel { Dictionary = result.OrderBy(r => r.Value).ToList() });
            }

            if (string.Compare(query.Name, "PersonType", true) == 0)
            {
                var result = new List<MiscEnumViewModel>(2);
                result.Add(new MiscEnumViewModel { Key = PersonType.Applicant, Value = "Соискатель" });
                result.Add(new MiscEnumViewModel { Key = PersonType.Host, Value = "Принимающая сторона" });
                return Ok(new DictioanaryMiscEnumViewModel { Dictionary = result.OrderBy(r => r.Value).ToList() });
            }

            if (string.Compare(query.Name, "DocumentType", true) == 0)
            {
                var result = new List<MiscEnumViewModel>(5);
                result.Add(new MiscEnumViewModel { Key = DocumentType.AdministrativePractice, Value = "Административная практика" });
                result.Add(new MiscEnumViewModel { Key = DocumentType.Citizenship, Value = "Гражданство" });
                result.Add(new MiscEnumViewModel { Key = DocumentType.MigrationRegistration, Value = "Миграционный учёт" });
                result.Add(new MiscEnumViewModel { Key = DocumentType.TemporaryResidencePermit, Value = "Разрешение на временное проживание" });
                result.Add(new MiscEnumViewModel { Key = DocumentType.Residence, Value = "Вид на жительство" });
                return Ok(new DictioanaryMiscEnumViewModel { Dictionary = result.OrderBy(r => r.Value).ToList() });
            }



            int dicId;
            IQueryable<Misc> q;
            if (int.TryParse(query.Name, out dicId))
            {
                q = from m in _repository.GetAll()
                    where m.MiscId == dicId
                    orderby m.MiscValue
                    select m;
            }
            else
            {
                q = from m in _repository.GetAll()
                    join mn in _repositoryNames.GetAll() on m.MiscId equals mn.Id
                    where mn.Name == query.Name && mn.DocType == query.DocType && mn.PersonCategory == query.Category
                    select m;

            }

            var res = q.Select(m => new MiscViewModel { Key = m.Id, Value = m.MiscValue }).OrderBy(m => m.Value).ToList();

            return Ok(new DictioanaryMiscViewModel { Dictionary = res });
        }
    }
}
