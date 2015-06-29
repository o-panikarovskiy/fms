using Domain.Abstract;
using Domain.Concrete;
using Domain.Models;
using Domain.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Concreate
{
    public class CSVImport : IFileImport
    {
        private readonly ApplicationDbContext _db;
        public CSVImport()
        {
            _db = new ApplicationDbContext();

        }

        public async Task<UploadingProgress> CreateProgressAsync(string fileName, string userID)
        {
            var progress = new UploadingProgress()
            {
                StartDate = DateTime.Now,
                UserId = userID,
                FileName = Path.GetFileNameWithoutExtension(fileName),
                Percent = 0,
                IsCompleted = false
            };

            _db.UploadProgress.Add(progress);
            await _db.SaveChangesAsync();

            return progress;
        }
        public async Task<UploadingProgress> GetProgressByIdAsync(int ID, string userID)
        {
            return await _db.UploadProgress.SingleOrDefaultAsync(p => p.Id == ID && p.UserId == userID);
        }
        public async Task<UploadingProgress> GetProgressByFileNameAsync(string fileName, string userID)
        {
            return await _db.UploadProgress.SingleOrDefaultAsync(p => p.FileName == fileName && p.UserId == userID);
        }
        public async Task<UploadingProgress> GetCurrentUserProgressAsync(string userID)
        {
            var result = from p in _db.UploadProgress
                         where p.UserId == userID && !p.IsCompleted
                         orderby p.StartDate descending
                         select p;

            return await result.FirstOrDefaultAsync();
        }
        public void StartImport(UploadingProgress progress, DocumentType type, string filePath, string userID)
        {
            try
            {
                switch (type)
                {
                    case DocumentType.AdministrativePractice:
                        ImportAdministrativePractice(progress, filePath, userID);
                        break;
                    case DocumentType.TemporaryResidencePermit:
                        break;
                    case DocumentType.Residence:
                        break;
                    case DocumentType.Citizenship:
                        ImportСitizenship(progress, filePath, userID);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                progress.HasErrors = true;
                progress.ExceptionMessage = ex.Message;
                throw;
            }
            finally
            {
                progress.IsCompleted = true;
                progress.EndDate = DateTime.Now;
                _db.SaveChanges();
            }
        }


        private void ImportAdministrativePractice(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "ФИО", "Дата рождения", "Гражданство", "Категория лица",
                    "Адрес регистрации", "Номер протокола", "Дата составления", "Статья", "Вид правонарушения",
                    "Орган рассмотрения", "Статус дела", "Дата постановления", "Принятое решение", "Тип взыскания",
                    "Сумма(начислено)", "Сумма(оплачено)"});

            DateTime birthday, date1, date2;
            float sum1, sum2;

            var miscnames = _db.MiscNames.ToList();
            var parameterFactNames = _db.PrmFactNames.ToList();

            int i = 0;
            foreach (var row in table)
            {
                if (string.IsNullOrWhiteSpace(row["ФИО"]))
                {
                    continue;
                }

                if (!DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday))
                {
                    continue;
                }

                var m1 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Гражданство"), row["Гражданство"]);
                var m2 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Статья"), row["Статья"]);
                var m3 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Вид правонарушения"), row["Вид правонарушения"]);
                var m4 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Орган рассмотрения"), row["Орган рассмотрения"]);
                var m5 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Статус дела"), row["Статус дела"]);
                var m6 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Принятое решение"), row["Принятое решение"]);
                var m7 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Тип взыскания"), row["Тип взыскания"]);

                var person = GetOrCreatePerson(row["ФИО"], birthday,
                  row["Категория лица"] == "Физическое" ? PersonCategory.Individual : PersonCategory.Legal,
                  row["Гражданство"] == "Россия" ? PersonType.Host : PersonType.Applicant);

                var document = GetOrCreateDocument(person, DocumentType.AdministrativePractice, NormalizeString(row["Номер протокола"]), userid);

                GetOrCreatePersonFact(false, person,
                    parameterFactNames.Single(f => f.NameRu == "Гражданство"), null, m1.Id);
                GetOrCreatePersonFact(false, person,
                    parameterFactNames.Single(f => f.NameRu == "Адрес"), NormalizeString(row["Адрес регистрации"]));

                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Статья"), null, m2.Id);
                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Вид правонарушения"), null, m3.Id);
                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Орган рассмотрения"), null, m4.Id);
                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Статус дела"), null, m5.Id);
                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Принятое решение"), null, m6.Id);
                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Тип взыскания"), null, m7.Id);

                if (DateTime.TryParseExact(row["Дата составления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date1))
                {
                    GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Дата составления"), null, null, null, date1);
                }

                if (DateTime.TryParseExact(row["Дата постановления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date2))
                {
                    GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Дата постановления"), null, null, null, date2);
                }

                if (float.TryParse(row["Сумма(начислено)"], out sum1))
                {
                    GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Сумма(начислено)"), null, null, sum1);
                }

                if (float.TryParse(row["Сумма(оплачено)"], out sum2))
                {
                    GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Сумма(оплачено)"), null, null, sum2);
                }

                i++;
                progress.Percent = (float)i / table.Count * 100;
                _db.SaveChanges();
            }



        }
        private void ImportСitizenship(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "Рег.номер", "Тип дела", "Дата приема", "Основание для приема",
                "ФИО", "Дата рождения", "Гражданство", "Тип документа", "Номер документа",
                "Адрес", "Решение", "Основание решения", "Дата решения", "Номер решения"});

            DateTime birthday, date1, date2;

            var miscnames = _db.MiscNames.ToList();
            var parameterFactNames = _db.PrmFactNames.ToList();

            int i = 0;
            foreach (var row in table)
            {
                if (string.IsNullOrWhiteSpace(row["ФИО"]))
                {
                    continue;
                }

                if (!DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday))
                {
                    continue;
                }

                var m1 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Тип дела"), row["Тип дела"]);
                var m2 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Основание для приема"), row["Основание для приема"]);
                var m3 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Гражданство"), row["Гражданство"]);
                var m4 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Тип документа"), row["Тип документа"]);
                var m5 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Решение"), row["Решение"]);
                var m6 = GetOrCreateMisc(miscnames.Single(m => m.NameRu == "Основание решения"),row["Основание решения"]);

                var person = GetOrCreatePerson(row["ФИО"], birthday, PersonCategory.Individual, PersonType.Applicant);
                var document = GetOrCreateDocument(person, DocumentType.Citizenship, NormalizeString(row["Рег.номер"]), userid);

                GetOrCreatePersonFact(false, person,
                    parameterFactNames.Single(f => f.NameRu == "Гражданство"), null, m3.Id);
                GetOrCreatePersonFact(false, person,
                  parameterFactNames.Single(f => f.NameRu == "Тип документа"), NormalizeString(row["Номер документа"]), m4.Id);
                GetOrCreatePersonFact(false, person,
                    parameterFactNames.Single(f => f.NameRu == "Адрес"), NormalizeString(row["Адрес"]));

                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Тип дела"), null, m1.Id);
                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Основание для приема"), null, m2.Id);
                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Решение"), null, m5.Id);
                GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Основание решения"), null, m6.Id);

                if (DateTime.TryParseExact(row["Дата приема"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date1))
                {
                    GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Дата приема"), null, null, null, date1);
                }

                if (DateTime.TryParseExact(row["Дата решения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date2))
                {
                    GetOrCreateDocumentParameter(false, document, parameterFactNames.Single(d => d.NameRu == "Дата решения"), null, null, null, date2);
                }

                i++;
                progress.Percent = (float)i / table.Count * 100;
                _db.SaveChanges();
            }



        }


        #region Вспомогательные методы    
        private Misc GetOrCreateMisc(MiscName miskName, string value)
        {
            var misc = _db.Misc.SingleOrDefault(m => m.MiscId == miskName.Id && m.MiscValue == value);
            if (misc == null)
            {
                misc = new Misc
                {
                    MiscId = miskName.Id,
                    MiscValue = value
                };
                _db.Misc.Add(misc);
                _db.SaveChanges();
            }
            return misc;
        }
        private Person GetOrCreatePerson(string name, DateTime birthday, PersonCategory category, PersonType type)
        {
            var person = _db.People.SingleOrDefault(p => p.Name == name && p.Birthday == birthday);

            if (person == null)
            {
                person = new Person
                {
                    Name = name,
                    Birthday = birthday,
                    Type = type,
                    Category = category
                };
                _db.People.Add(person);
                _db.SaveChanges();
            }

            return person;
        }
        private Document GetOrCreateDocument(Person person, DocumentType type, string number, string userid)
        {
            var doc = _db.Documents.SingleOrDefault(d => d.Number == number && d.Type == type);
            if (doc == null)
            {
                var now = DateTime.Now;
                doc = new Document()
                {
                    ApplicantPersonId = (person.Type == PersonType.Applicant) ? person.Id : (int?)null,
                    HostPersonId = (person.Type == PersonType.Host) ? person.Id : (int?)null,
                    Number = number,
                    CreatedById = userid,
                    UpdatedById = userid,
                    CreatedDate = now,
                    UpdatedDate = now,
                    Type = type
                };
                _db.Documents.Add(doc);
                _db.SaveChanges();
            }
            return doc;
        }
        private PersonFact GetOrCreatePersonFact(bool shouldSave, Person person, PrmFactName factName, string strValue = null, int? intValue = null, float? floatValue = null, DateTime? dateValue = null)
        {
            var fact = _db.PersonFacts.SingleOrDefault(f => f.PersonId == person.Id && f.FactId == factName.Id && f.StringValue == strValue
            && f.IntValue == intValue && f.FloatValue == floatValue && f.DateValue == dateValue);

            if (fact == null)
            {
                fact = new PersonFact()
                {
                    PersonId = person.Id,
                    FactId = factName.Id,
                    FactDate = DateTime.Now,
                    StringValue = strValue,
                    IntValue = intValue,
                    FloatValue = floatValue,
                    DateValue = dateValue
                };
                _db.PersonFacts.Add(fact);
            }
            else
            {
                fact.FactDate = DateTime.Now;
            }

            if (shouldSave)
            {
                _db.SaveChanges();
            }
            return fact;
        }
        private DocumentParameter GetOrCreateDocumentParameter(bool shouldSave, Document doc, PrmFactName paramName, string strValue = null, int? intValue = null, float? floatValue = null, DateTime? dateValue = null)
        {
            strValue = strValue ?? string.Empty;

            var docPrm = _db.DocumentParameters.SingleOrDefault(d => d.DocumentId == doc.Id && d.ParameterId == paramName.Id);
            if (docPrm == null)
            {
                docPrm = new DocumentParameter()
                {
                    DateValue = dateValue,
                    IntValue = intValue,
                    FloatValue = floatValue,
                    StringValue = strValue,
                    DocumentId = doc.Id,
                    ParameterId = paramName.Id
                };
                _db.DocumentParameters.Add(docPrm);
            }
            else
            {
                docPrm.DateValue = dateValue;
                docPrm.IntValue = intValue;
                docPrm.FloatValue = floatValue;
                docPrm.StringValue = strValue;
                docPrm.DocumentId = doc.Id;
                docPrm.ParameterId = paramName.Id;
            }

            if (shouldSave)
            {
                _db.SaveChanges();
            }
            return docPrm;
        }
        private List<Dictionary<string, string>> ReadCSVTable(string path, char separator, string[] columns)
        {
            string[] csvRows = File.ReadAllLines(path, Encoding.GetEncoding(1251));
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>(csvRows.Length);

            if (csvRows.Length > 0)
            {
                string[] fields = csvRows[0].Split(separator);


                Dictionary<string, int> dicIdx = new Dictionary<string, int>(csvRows.Length);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i].Trim();
                    if (!dicIdx.ContainsKey(field))
                    {
                        dicIdx[field] = i;
                    }
                }



                for (int i = 1; i < csvRows.Length; i++)
                {
                    fields = csvRows[i].Split(separator);
                    var dic = new Dictionary<string, string>(columns.Length);
                    foreach (var col in columns)
                    {
                        if (!dic.ContainsKey(col) && dicIdx.ContainsKey(col))
                        {
                            dic[col] = fields[dicIdx[col]].Trim();
                        }
                        else
                        {
                            dic[col] = string.Empty;
                        }
                    }
                    list.Add(dic);
                }
            }

            return list;
        }

        private string NormalizeString(string value)
        {
            if (value != null && string.IsNullOrWhiteSpace(value.Replace("-", "")))
            {
                value = null;
            }
            return value;
        }
        #endregion

    }
}
