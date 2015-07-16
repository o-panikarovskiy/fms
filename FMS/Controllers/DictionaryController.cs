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

        [Route("api/dictionary/{name}/{type}")]
        public IHttpActionResult GetDictionary(string name, string type)
        {
            if (string.Compare(name, "PersonCategory", true) == 0)
            {
                var result = new List<MiscViewModel>(2);
                result.Add(new MiscViewModel { Key = (int)PersonCategory.Individual, Value = "Физическое лицо" });
                result.Add(new MiscViewModel { Key = (int)PersonCategory.Legal, Value = "Юридическое лицо" });
                return Ok(new DictioanryMiscViewModel { Dictionary = result.OrderBy(r => r.Value).ToList() });
            }

            if (string.Compare(name, "PersonType", true) == 0)
            {
                var result = new List<MiscViewModel>(2);
                result.Add(new MiscViewModel { Key = (int)PersonType.Applicant, Value = "Соискатель" });
                result.Add(new MiscViewModel { Key = (int)PersonType.Host, Value = "Принимающая сторона" });
                return Ok(new DictioanryMiscViewModel { Dictionary = result.OrderBy(r => r.Value).ToList() });
            }

            if (string.Compare(name, "DocumentType", true) == 0)
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
            var q = _repository.GetAll();
            if (int.TryParse(name, out dicId))
            {
                q = from m in _repository.GetAll()
                    where m.MiscId == dicId
                    orderby m.MiscValue
                    select m;
            }
            else
            {
                var sub = from m in q
                          join mn in _repositoryNames.GetAll() on m.MiscId equals mn.Id
                          where mn.Name == name
                          select new { mn = mn, m = m };

                PersonCategory category;
                DocumentType docType;
                if (Enum.TryParse<PersonCategory>(type, out category))
                {
                    sub = sub.Where(s => s.mn.PersonCategory == category);
                }

                if (Enum.TryParse<DocumentType>(type, out docType))
                {
                    sub = sub.Where(s => s.mn.DocType == docType);
                };

                q = sub.Select(s => s.m);

            }

            var res = q.Select(m => new MiscViewModel { Key = m.Id, Value = m.MiscValue }).ToList();

            return Ok(new DictioanryMiscViewModel { Dictionary = res });
        }
    }
}
