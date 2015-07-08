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
                        ImportTemporaryResidencePermit(progress, filePath, userID);
                        break;
                    case DocumentType.Residence:
                        ImportResidencePermit(progress, filePath, userID);
                        break;
                    case DocumentType.Citizenship:
                        ImportСitizenship(progress, filePath, userID);
                        break;
                    default:
                        ImportMigrationRegistration(progress, filePath, userID);
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


            var mscNames = _db.MiscNames.ToList();
            var prmNames = _db.PrmFactNames.ToList();

            int i = 0;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["ФИО"]) && !string.IsNullOrWhiteSpace(row["Дата рождения"])))
            {
                DateTime date;
                float sum;

                if (DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    var m1 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Гражданство"), row["Гражданство"]);
                    var m2 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Статья"), row["Статья"]);
                    var m3 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Вид правонарушения"), row["Вид правонарушения"]);
                    var m4 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Орган рассмотрения"), row["Орган рассмотрения"]);
                    var m5 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Статус дела"), row["Статус дела"]);
                    var m6 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Принятое решение"), row["Принятое решение"]);
                    var m7 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Тип взыскания"), row["Тип взыскания"]);

                    var person = GetOrCreatePerson(row["ФИО"], date,
                      row["Категория лица"] == "Физическое" ? PersonCategory.Individual : PersonCategory.Legal,
                      row["Гражданство"] == "Россия" ? PersonType.Host : PersonType.Applicant);

                    var document = UpdateOrCreateDocumentByNumber(person, DocumentType.AdministrativePractice, NormalizeString(row["Номер протокола"]), userid);

                    UpdateOrCreatePersonFact(person,
                        prmNames.Single(f => f.NameRu == "Гражданство"), null, m1.Id);
                    UpdateOrCreatePersonFact(person,
                        prmNames.Single(f => f.NameRu == "Адрес"), NormalizeString(row["Адрес регистрации"]));

                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Статья"), null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Вид правонарушения"), null, m3.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Орган рассмотрения"), null, m4.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Статус дела"), null, m5.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Принятое решение"), null, m6.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Тип взыскания"), null, m7.Id);

                    if (DateTime.TryParseExact(row["Дата составления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата составления"), null, null, null, date);
                    }
                    else
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата составления"));
                    }

                    if (DateTime.TryParseExact(row["Дата постановления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата постановления"), null, null, null, date);
                    }
                    else
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата постановления"));
                    }

                    if (float.TryParse(row["Сумма(начислено)"], out sum))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Сумма(начислено)"), null, null, sum);
                    }
                    else
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Сумма(начислено)"));
                    }

                    if (float.TryParse(row["Сумма(оплачено)"], out sum))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Сумма(оплачено)"), null, null, sum);
                    }
                    else
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Сумма(оплачено)"));
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    _db.SaveChanges();
                }
            }



        }
        private void ImportСitizenship(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "Рег.номер", "Тип дела", "Дата приема", "Основание для приема",
                "ФИО", "Дата рождения", "Гражданство", "Тип документа", "Номер документа",
                "Адрес", "Решение", "Основание решения", "Дата решения", "Номер решения"});

            var mscNames = _db.MiscNames.ToList();
            var prmNames = _db.PrmFactNames.ToList();

            int i = 0;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["ФИО"]) && !string.IsNullOrWhiteSpace(row["Дата рождения"])))
            {
                DateTime date;
                if (DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    var m1 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Тип дела (Гр.)"), row["Тип дела"]);
                    var m2 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Основание для приема (Гр.)"), row["Основание для приема"]);
                    var m3 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Гражданство"), row["Гражданство"]);
                    var m4 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Тип документа"), row["Тип документа"]);
                    var m5 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Решение (Гр.)"), row["Решение"]);
                    var m6 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Основание решения (Гр.)"), row["Основание решения"]);

                    var person = GetOrCreatePerson(row["ФИО"], date, PersonCategory.Individual, PersonType.Applicant);
                    var document = UpdateOrCreateDocumentByNumber(person, DocumentType.Citizenship, NormalizeString(row["Рег.номер"]), userid);

                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Гражданство"), null, m3.Id);
                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Тип документа"), NormalizeString(row["Номер документа"]), m4.Id);
                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Адрес"), NormalizeString(row["Адрес"]));

                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Тип дела"), null, m1.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Основание для приема"), null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Решение"), null, m5.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Основание решения"), null, m6.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Номер решения"), NormalizeString(row["Номер решения"]), null);

                    if (DateTime.TryParseExact(row["Дата приема"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата приема"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата решения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата решения"), null, null, null, date);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    _db.SaveChanges();
                }
            }



        }
        private void ImportTemporaryResidencePermit(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "Номер дела", "ФИО", "Дата рождения", "Гражданство", "Номер документа", "Основание для приема",
                "Дата приема заявления", "Дата решения", "Номер решения", "Основание решения", "Пользователь решения", "Номер РВП", "Дата печати",
                "Дата факт.выдачи", "Предполагаемый адрес" });

            var mscNames = _db.MiscNames.ToList();
            var prmNames = _db.PrmFactNames.ToList();

            var foreignPassportMisc = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Тип документа"), "Иностранный паспорт");

            int i = 0;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["ФИО"]) && !string.IsNullOrWhiteSpace(row["Дата рождения"])))
            {
                DateTime date, birthday;
                if (DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday) &&
                    DateTime.TryParseExact(row["Дата приема заявления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {

                    var m1 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Гражданство"), row["Гражданство"]);
                    var m2 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Основание для приема (РВП)"), row["Основание для приема"]);
                    var m3 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Основание решения (РВП)"), row["Основание решения"]);
                    var m4 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Пользователь решения (РВП)"), row["Пользователь решения"]);

                    var person = GetOrCreatePerson(row["ФИО"], birthday, PersonCategory.Individual, PersonType.Applicant);
                    var document = UpdateOrCreateDocumentByApplicantDateNumber(person, DocumentType.TemporaryResidencePermit, date, NormalizeString(row["Номер дела"]), userid);

                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Тип документа"), NormalizeString(row["Номер документа"]), foreignPassportMisc.Id);
                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Гражданство"), null, m1.Id);
                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Адрес"), NormalizeString(row["Предполагаемый адрес"]));

                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Основание для приема"), null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Основание решения"), null, m3.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Пользователь решения"), null, m4.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Номер решения"), NormalizeString(row["Номер решения"]));
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Номер РВП"), NormalizeString(row["Номер РВП"]));

                    if (DateTime.TryParseExact(row["Дата приема заявления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата приема заявления"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата решения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата решения"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата печати"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата печати"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата факт.выдачи"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата фактической выдачи"), null, null, null, date);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    _db.SaveChanges();
                }
            }



        }
        private void ImportResidencePermit(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "Идентификатор дела", "Номер дела", "Тип дела", "ФИО",
                "Дата рождения", "Гражданство", "Серия", "Номер", "Основание дела", "Дата приема заявления", "Дата решения", "Тип решения", "Серия ВНЖ", "Номер ВНЖ",
                "Дата выдачи", "Действителен С", "Действителен ПО", "Дата фактической выдачи", "Адрес" });

            var mscNames = _db.MiscNames.ToList();
            var prmNames = _db.PrmFactNames.ToList();
            var foreignPassportMisc = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Тип документа"), "Иностранный паспорт");

            int i = 0;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["ФИО"]) && !string.IsNullOrWhiteSpace(row["Дата рождения"])))
            {
                DateTime date, birthday;
                if (DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday))
                {

                    var m1 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Гражданство"), row["Гражданство"]);
                    var m2 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Тип дела (ВНЖ)"), row["Тип дела"]);
                    var m3 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Основание дела (ВНЖ)"), row["Основание дела"]);
                    var m4 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Тип решения (ВНЖ)"), row["Тип решения"]);
                    var m5 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Серия (ВНЖ)"), row["Серия ВНЖ"]);

                    var person = GetOrCreatePerson(row["ФИО"], birthday, PersonCategory.Individual, PersonType.Applicant);
                    var document = UpdateOrCreateDocumentByNumber(person, DocumentType.Residence, NormalizeString(row["Идентификатор дела"]), userid);

                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Тип документа"),
                        string.Concat(NormalizeString(row["Серия"]), NormalizeString(row["Номер"])), foreignPassportMisc.Id);
                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Гражданство"), null, m1.Id);
                    UpdateOrCreatePersonFact(person, prmNames.Single(f => f.NameRu == "Адрес"), NormalizeString(row["Адрес"]));

                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Номер дела"), NormalizeString(row["Номер дела"]));
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Тип дела"), null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Основание дела"), null, m3.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Тип решения"), null, m4.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Серия ВНЖ"), null, m5.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Номер ВНЖ"), NormalizeString(row["Номер ВНЖ"]));

                    if (DateTime.TryParseExact(row["Дата приема заявления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата приема заявления"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата решения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата решения"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата выдачи"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата выдачи"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Действителен С"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Действителен С"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Действителен ПО"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Действителен ПО"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата фактической выдачи"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата фактической выдачи"), null, null, null, date);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    _db.SaveChanges();
                }
            }



        }
        private void ImportMigrationRegistration(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "номер уведомления", "фио иг рус", "дата рождения", "гражданство",
                "серия", "номер", "дата выдачи", "отметка проставлена", "дата въезда", "дата рег. с", "дата рег. до", "цель въезда", "первично/продлено",
                "КПП въезда", "принимающая сторона", "гражданство прин.стороны", "адрес регистрации иг" });

            var mscNames = _db.MiscNames.ToList();
            var prmNames = _db.PrmFactNames.ToList();
            var foreignPassportMisc = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Тип документа"), "Иностранный паспорт");
            var ruCtz = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Гражданство"), "Россия");


            int i = 0;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["фио иг рус"]) && !string.IsNullOrWhiteSpace(row["дата рождения"])))
            {
                DateTime date, birthday;
                if (DateTime.TryParseExact(row["дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday))
                {
                    var m1 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Гражданство"), row["гражданство"]);
                    var m2 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Отметка проставлена (МУ)"), row["отметка проставлена"]);
                    var m3 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Цель въезда (МУ)"), row["цель въезда"]);
                    var m4 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Первично/Продлено"), row["первично/продлено"]);
                    var m5 = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "КПП въезда"), row["КПП въезда"]);

                    var applicant = GetOrCreatePerson(row["фио иг рус"], birthday, PersonCategory.Individual, PersonType.Applicant);

                    UpdateOrCreatePersonFact(applicant, prmNames.Single(f => f.NameRu == "Гражданство"), null, m1.Id);
                    UpdateOrCreatePersonFact(applicant, prmNames.Single(f => f.NameRu == "Тип документа"),
                      string.Concat(NormalizeString(row["серия"]), NormalizeString(row["номер"])), foreignPassportMisc.Id);
                    UpdateOrCreatePersonFact(applicant, prmNames.Single(f => f.NameRu == "Адрес"), NormalizeString(row["адрес регистрации иг"]));


                    Document document = null;

                    var split = row["принимающая сторона"].Split(',');
                    if (split.Length > 1)
                    {
                        string name = split[0].Trim();
                        var ctz = NormalizeString(row["гражданство прин.стороны"]);

                        Person host = null;
                        if (ctz == "Организация")
                        {
                            host = GetOrCreatePerson(name, split[1].Replace("инн:", "").Trim(), PersonCategory.Legal, PersonType.Host);
                        }
                        else
                        {
                            DateTime.TryParseExact(split[1].Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday);
                            host = GetOrCreatePerson(name, birthday, PersonCategory.Individual, PersonType.Host);
                            if (ctz == "Россия")
                            {
                                UpdateOrCreatePersonFact(host, prmNames.Single(f => f.NameRu == "Гражданство"), null, ruCtz.Id);
                            }
                            else if (!string.IsNullOrWhiteSpace(ctz))
                            {
                                var mctz = GetOrCreateMisc(mscNames.Single(m => m.NameRu == "Гражданство"), ctz);
                                UpdateOrCreatePersonFact(host, prmNames.Single(f => f.NameRu == "Гражданство"), null, mctz.Id);
                            }
                        }

                        document = UpdateOrCreateDocumentByNumber(applicant, host, DocumentType.MigrationRegistration, NormalizeString(row["номер уведомления"]), userid);
                    }
                    else
                    {
                        document = UpdateOrCreateDocumentByNumber(applicant, DocumentType.MigrationRegistration, NormalizeString(row["номер уведомления"]), userid);
                    }



                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Отметка проставлена"), null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Цель въезда"), null, m3.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Первично/Продлено"), null, m4.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "КПП въезда"), null, m5.Id);

                    if (DateTime.TryParseExact(row["дата выдачи"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата выдачи"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["дата въезда"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата въезда"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["дата рег. с"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата регистрации С"), null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["дата рег. до"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames.Single(d => d.NameRu == "Дата регистрации ДО"), null, null, null, date);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    _db.SaveChanges();
                }
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
        private Person GetOrCreatePerson(string name, string code, PersonCategory category, PersonType type)
        {
            var person = _db.People.SingleOrDefault(p => p.Name == name && p.Code == code);
            if (person == null)
            {
                person = new Person
                {
                    Name = name,
                    Type = type,
                    Category = category,
                    Code = code
                };
                _db.People.Add(person);
                _db.SaveChanges();
            }

            return person;
        }
        private Document UpdateOrCreateDocumentByNumber(Person applicant, DocumentType type, string number, string userid)
        {
            var doc = _db.Documents.SingleOrDefault(d => d.Number == number && d.Type == type);
            if (doc == null)
            {
                var now = DateTime.Now;
                doc = new Document()
                {
                    ApplicantPersonId = (applicant.Type == PersonType.Applicant) ? applicant.Id : (int?)null,
                    HostPersonId = (applicant.Type == PersonType.Host) ? applicant.Id : (int?)null,
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
        private Document UpdateOrCreateDocumentByNumber(Person applicant, Person host, DocumentType type, string number, string userid)
        {
            var doc = _db.Documents.SingleOrDefault(d => d.Number == number && d.Type == type);
            if (doc == null)
            {
                var now = DateTime.Now;
                doc = new Document()
                {
                    ApplicantPersonId = applicant.Id,
                    HostPersonId = host.Id,
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
        private Document UpdateOrCreateDocumentByApplicantDateNumber(Person person, DocumentType type, DateTime date, string number, string userid)
        {
            var doc = _db.Documents.SingleOrDefault(d => d.CreatedDate == date && d.Number == number && d.ApplicantPersonId == person.Id && d.Type == type);
            if (doc == null)
            {
                doc = new Document()
                {
                    ApplicantPersonId = person.Id,
                    HostPersonId = null,
                    Number = number,
                    CreatedById = userid,
                    UpdatedById = userid,
                    CreatedDate = date,
                    UpdatedDate = DateTime.Now,
                    Type = type
                };
                _db.Documents.Add(doc);
                _db.SaveChanges();
            }
            return doc;
        }
        private PersonFact UpdateOrCreatePersonFact(Person person, PrmFactName factName, string strValue = null, int? intValue = null, float? floatValue = null, DateTime? dateValue = null)
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
            };

            _db.SaveChanges();
            return fact;
        }
        private DocumentParameter UpdateOrCreateDocumentParameter(Document doc, PrmFactName paramName, string strValue = null, int? intValue = null, float? floatValue = null, DateTime? dateValue = null)
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
            };

            _db.SaveChanges();
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
                        else if (!dicIdx.ContainsKey(col))
                        {
                            throw new ArgumentException(string.Format("Колонка \"{0}\" отсутствует в импортируемом файле.\nВозможно вы пытаетесь импортировать не тот файл.", col));
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
