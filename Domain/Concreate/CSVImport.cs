﻿using Domain.Abstract;
using Domain.Concrete;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

        public async Task<UploadingProgress> CreateProgressAsync(string fileId, string userID)
        {
            var progress = new UploadingProgress()
            {
                StartDate = DateTime.Now,
                UserId = Guid.Parse(userID),
                FileId = Guid.Parse(fileId),
                Percent = 0,
                IsCompleted = false
            };

            _db.UploadProgress.Add(progress);
            await _db.SaveChangesAsync();

            return progress;
        }
        public async Task<UploadingProgress> GetProgressByIdAsync(int ID, string userId)
        {
            var uid = Guid.Parse(userId);
            return await _db.UploadProgress.SingleOrDefaultAsync(p => p.Id == ID && p.UserId == uid);
        }
        public async Task<UploadingProgress> GetProgressByFileNameAsync(string fileId, string userId)
        {
            var uid = Guid.Parse(userId);
            var fid = Guid.Parse(fileId);
            return await _db.UploadProgress.SingleOrDefaultAsync(p => p.FileId == fid && p.UserId == uid);
        }
        public async Task<UploadingProgress> GetCurrentUserProgressAsync(string userId)
        {
            var uid = Guid.Parse(userId);
            var result = from p in _db.UploadProgress
                         where p.UserId == uid && !p.IsCompleted
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
                progress.ExceptionMessage = ex.ToString();
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

            var prmNames = _db.ParameterNames.Where(p => p.DocType == DocumentType.AdministrativePractice || p.PersonCategory != null).ToDictionary(k => k.Name, v => v);

            int i = 0;
            progress.TotalRows = table.Count;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["ФИО"]) && !string.IsNullOrWhiteSpace(row["Дата рождения"])))
            {
                DateTime date;
                float sum;

                if (DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    var m1 = GetOrCreateMisc(prmNames["Гражданство"].MiscParentId, row["Гражданство"]);
                    var m2 = GetOrCreateMisc(prmNames["Статья"].MiscParentId, row["Статья"]);
                    var m3 = GetOrCreateMisc(prmNames["Вид правонарушения"].MiscParentId, row["Вид правонарушения"]);
                    var m4 = GetOrCreateMisc(prmNames["Орган рассмотрения"].MiscParentId, row["Орган рассмотрения"]);
                    var m5 = GetOrCreateMisc(prmNames["Статус дела"].MiscParentId, row["Статус дела"]);
                    var m6 = GetOrCreateMisc(prmNames["Принятое решение"].MiscParentId, row["Принятое решение"]);
                    var m7 = GetOrCreateMisc(prmNames["Тип взыскания"].MiscParentId, row["Тип взыскания"]);

                    var person = GetOrCreatePerson(row["ФИО"], date,
                        row["Категория лица"] == "Физическое" ? PersonCategory.Individual : PersonCategory.Legal,
                        row["Гражданство"] == "Россия" ? PersonType.Host : PersonType.Applicant);

                    var document = UpdateOrCreateDocumentByNumber(person, DocumentType.AdministrativePractice, row["Номер протокола"], userid);

                    UpdateOrCreatePersonFact(person, prmNames["Гражданство"], null, m1.Id);
                    UpdateOrCreatePersonFact(person, prmNames["Адрес"], row["Адрес регистрации"]);

                    UpdateOrCreateDocumentParameter(document, prmNames["Статья"], null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Вид правонарушения"], null, m3.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Орган рассмотрения"], null, m4.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Статус дела"], null, m5.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Принятое решение"], null, m6.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Тип взыскания"], null, m7.Id);

                    if (DateTime.TryParseExact(row["Дата составления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата составления"], null, null, null, date);
                    }
                    else
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата составления"]);
                    }

                    if (DateTime.TryParseExact(row["Дата постановления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата постановления"], null, null, null, date);
                    }
                    else
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата постановления"]);
                    }

                    if (float.TryParse(row["Сумма(начислено)"], out sum))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Сумма(начислено)"], null, null, sum);
                    }
                    else
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Сумма(начислено)"]);
                    }

                    if (float.TryParse(row["Сумма(оплачено)"], out sum))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Сумма(оплачено)"], null, null, sum);
                    }
                    else
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Сумма(оплачено)"]);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    progress.CurrentRow = i;
                    _db.SaveChanges();
                }
            }



        }
        private void ImportСitizenship(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "Рег.номер", "Тип дела", "Дата приема", "Основание для приема",
                "ФИО", "Дата рождения", "Гражданство", "Тип документа", "Номер документа",
                "Адрес", "Решение", "Основание решения", "Дата решения", "Номер решения"});


            var prmNames = _db.ParameterNames.Where(p => p.DocType == DocumentType.Citizenship || p.PersonCategory != null).ToDictionary(k => k.Name, v => v);

            int i = 0;
            progress.TotalRows = table.Count;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["ФИО"]) && !string.IsNullOrWhiteSpace(row["Дата рождения"])))
            {
                DateTime date;
                if (DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    var m1 = GetOrCreateMisc(prmNames["Тип дела"].MiscParentId, row["Тип дела"]);
                    var m2 = GetOrCreateMisc(prmNames["Основание для приема"].MiscParentId, row["Основание для приема"]);
                    var m3 = GetOrCreateMisc(prmNames["Гражданство"].MiscParentId, row["Гражданство"]);
                    var m4 = GetOrCreateMisc(prmNames["Личный документ"].MiscParentId, row["Тип документа"]);
                    var m5 = GetOrCreateMisc(prmNames["Решение"].MiscParentId, row["Решение"]);
                    var m6 = GetOrCreateMisc(prmNames["Основание решения"].MiscParentId, row["Основание решения"]);

                    var person = GetOrCreatePerson(row["ФИО"], date, PersonCategory.Individual, PersonType.Applicant);
                    var document = UpdateOrCreateDocumentByNumber(person, DocumentType.Citizenship, row["Рег.номер"], userid);

                    UpdateOrCreatePersonFact(person, prmNames["Гражданство"], null, m3.Id);
                    UpdateOrCreatePersonFact(person, prmNames["Личный документ"], row["Номер документа"], m4.Id);
                    UpdateOrCreatePersonFact(person, prmNames["Адрес"], row["Адрес"]);

                    UpdateOrCreateDocumentParameter(document, prmNames["Тип дела"], null, m1.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Основание для приема"], null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Решение"], null, m5.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Основание решения"], null, m6.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Номер решения"], row["Номер решения"], null);

                    if (DateTime.TryParseExact(row["Дата приема"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата приема"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата решения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата решения"], null, null, null, date);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    progress.CurrentRow = i;
                    _db.SaveChanges();
                }
            }



        }
        private void ImportTemporaryResidencePermit(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "Номер дела", "ФИО", "Дата рождения", "Гражданство", "Номер документа", "Основание для приема",
                "Дата приема заявления", "Дата решения", "Номер решения", "Основание решения", "Пользователь решения", "Номер РВП", "Дата печати",
                "Дата факт.выдачи", "Предполагаемый адрес" });

            var prmNames = _db.ParameterNames.Where(p => p.DocType == DocumentType.TemporaryResidencePermit || p.PersonCategory != null).ToDictionary(k => k.Name, v => v);
            var foreignPassportMisc = GetOrCreateMisc(prmNames["Личный документ"].MiscParentId, "Иностранный паспорт");

            int i = 0;
            progress.TotalRows = table.Count;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["ФИО"]) && !string.IsNullOrWhiteSpace(row["Дата рождения"])))
            {
                DateTime date, birthday;
                if (DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday) &&
                    DateTime.TryParseExact(row["Дата приема заявления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {

                    var m1 = GetOrCreateMisc(prmNames["Гражданство"].MiscParentId, row["Гражданство"]);
                    var m2 = GetOrCreateMisc(prmNames["Основание для приема"].MiscParentId, row["Основание для приема"]);
                    var m3 = GetOrCreateMisc(prmNames["Основание решения"].MiscParentId, row["Основание решения"]);
                    var m4 = GetOrCreateMisc(prmNames["Пользователь решения"].MiscParentId, row["Пользователь решения"]);

                    var person = GetOrCreatePerson(row["ФИО"], birthday, PersonCategory.Individual, PersonType.Applicant);
                    var document = UpdateOrCreateDocumentByApplicantDateNumber(person, DocumentType.TemporaryResidencePermit, date, row["Номер дела"], userid);

                    UpdateOrCreatePersonFact(person, prmNames["Личный документ"], row["Номер документа"], foreignPassportMisc.Id);
                    UpdateOrCreatePersonFact(person, prmNames["Гражданство"], null, m1.Id);
                    UpdateOrCreatePersonFact(person, prmNames["Адрес"], row["Предполагаемый адрес"]);

                    UpdateOrCreateDocumentParameter(document, prmNames["Основание для приема"], null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Основание решения"], null, m3.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Пользователь решения"], null, m4.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Номер решения"], row["Номер решения"]);
                    UpdateOrCreateDocumentParameter(document, prmNames["Номер РВП"], row["Номер РВП"]);

                    if (DateTime.TryParseExact(row["Дата приема заявления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата приема заявления"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата решения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата решения"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата печати"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата печати"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата факт.выдачи"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата фактической выдачи"], null, null, null, date);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    progress.CurrentRow = i;
                    _db.SaveChanges();
                }
            }



        }
        private void ImportResidencePermit(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "Идентификатор дела", "Номер дела", "Тип дела", "ФИО",
                "Дата рождения", "Гражданство", "Серия", "Номер", "Основание дела", "Дата приема заявления", "Дата решения", "Тип решения", "Серия ВНЖ", "Номер ВНЖ",
                "Дата выдачи", "Действителен С", "Действителен ПО", "Дата фактической выдачи", "Адрес" });
            
            var prmNames = _db.ParameterNames.Where(p => p.DocType == DocumentType.Residence || p.PersonCategory != null).ToDictionary(k => k.Name, v => v);
            var foreignPassportMisc = GetOrCreateMisc(prmNames["Личный документ"].MiscParentId, "Иностранный паспорт");

            int i = 0;
            progress.TotalRows = table.Count;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["ФИО"]) && !string.IsNullOrWhiteSpace(row["Дата рождения"])))
            {
                DateTime date, birthday;
                if (DateTime.TryParseExact(row["Дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday))
                {

                    var m1 = GetOrCreateMisc(prmNames["Гражданство"].MiscParentId, row["Гражданство"]);
                    var m2 = GetOrCreateMisc(prmNames["Тип дела"].MiscParentId, row["Тип дела"]);
                    var m3 = GetOrCreateMisc(prmNames["Основание дела"].MiscParentId, row["Основание дела"]);
                    var m4 = GetOrCreateMisc(prmNames["Тип решения"].MiscParentId, row["Тип решения"]);
                    var m5 = GetOrCreateMisc(prmNames["Серия ВНЖ"].MiscParentId, row["Серия ВНЖ"]);

                    var person = GetOrCreatePerson(row["ФИО"], birthday, PersonCategory.Individual, PersonType.Applicant);
                    var document = UpdateOrCreateDocumentByNumber(person, DocumentType.Residence, row["Идентификатор дела"], userid);

                    UpdateOrCreatePersonFact(person, prmNames["Личный документ"], string.Concat(row["Серия"], row["Номер"]), foreignPassportMisc.Id);
                    UpdateOrCreatePersonFact(person, prmNames["Гражданство"], null, m1.Id);
                    UpdateOrCreatePersonFact(person, prmNames["Адрес"], row["Адрес"]);

                    UpdateOrCreateDocumentParameter(document, prmNames["Номер дела"], row["Номер дела"]);
                    UpdateOrCreateDocumentParameter(document, prmNames["Тип дела"], null, m2.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Основание дела"], null, m3.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Тип решения"], null, m4.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Серия ВНЖ"], null, m5.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Номер ВНЖ"], row["Номер ВНЖ"]);

                    if (DateTime.TryParseExact(row["Дата приема заявления"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата приема заявления"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата решения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата решения"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата выдачи"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата выдачи"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Действителен С"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Действителен С"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Действителен ПО"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Действителен ПО"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["Дата фактической выдачи"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата фактической выдачи"], null, null, null, date);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    progress.CurrentRow = i;
                    _db.SaveChanges();
                }
            }



        }
        private void ImportMigrationRegistration(UploadingProgress progress, string filePath, string userid)
        {
            var table = ReadCSVTable(filePath, ';', new string[] { "номер уведомления", "фио иг рус", "дата рождения", "гражданство",
                "серия", "номер", "дата выдачи", "отметка проставлена", "дата въезда", "дата рег. с", "дата рег. до", "цель въезда", "первично/продлено",
                "КПП въезда", "принимающая сторона", "гражданство прин.стороны", "адрес регистрации иг" });

         
            var prmNames = _db.ParameterNames.Where(p => p.DocType == DocumentType.MigrationRegistration || p.PersonCategory != null).ToDictionary(k => k.Name, v => v);
            var foreignPassportMisc = GetOrCreateMisc(prmNames["Личный документ"].MiscParentId, "Иностранный паспорт");
            var ruCtz = GetOrCreateMisc(prmNames["Гражданство"].MiscParentId, "Россия");


            int i = 0;
            progress.TotalRows = table.Count;
            foreach (var row in table.Where(row => !string.IsNullOrWhiteSpace(row["фио иг рус"]) && !string.IsNullOrWhiteSpace(row["дата рождения"])))
            {
                DateTime date, birthday;
                if (DateTime.TryParseExact(row["дата рождения"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out birthday))
                {
                    var m1 = GetOrCreateMisc(prmNames["Гражданство"].MiscParentId, row["гражданство"]);
                    var m2 = GetOrCreateMisc(prmNames["Отметка проставлена"].MiscParentId, row["отметка проставлена"]);
                    var m3 = GetOrCreateMisc(prmNames["Цель въезда"].MiscParentId, row["цель въезда"]);
                    var m4 = GetOrCreateMisc(prmNames["Первично/Продлено"].MiscParentId, row["первично/продлено"]);
                    var m5 = GetOrCreateMisc(prmNames["КПП въезда"].MiscParentId, row["КПП въезда"]);

                    var applicant = GetOrCreatePerson(row["фио иг рус"], birthday, PersonCategory.Individual, PersonType.Applicant);

                    UpdateOrCreatePersonFact(applicant, prmNames["Гражданство"], null, m1.Id);
                    UpdateOrCreatePersonFact(applicant, prmNames["Личный документ"], string.Concat(row["серия"], row["номер"]), foreignPassportMisc.Id);
                    UpdateOrCreatePersonFact(applicant, prmNames["Адрес"], row["адрес регистрации иг"]);


                    Document document = null;

                    var split = row["принимающая сторона"].Split(',');
                    if (split.Length > 1)
                    {
                        string name = split[0].Trim();
                        var ctz = row["гражданство прин.стороны"];

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
                                UpdateOrCreatePersonFact(host, prmNames["Гражданство"], null, ruCtz.Id);
                            }
                            else if (!string.IsNullOrWhiteSpace(ctz))
                            {
                                var mctz = GetOrCreateMisc(prmNames["Гражданство"].MiscParentId, ctz);
                                UpdateOrCreatePersonFact(host, prmNames["Гражданство"], null, mctz.Id);
                            }
                        }

                        document = UpdateOrCreateDocumentByNumber(applicant, host, DocumentType.MigrationRegistration, row["номер уведомления"], userid);
                    }
                    else
                    {
                        document = UpdateOrCreateDocumentByNumber(applicant, DocumentType.MigrationRegistration, row["номер уведомления"], userid);
                    }



                    UpdateOrCreateDocumentParameter(document, prmNames["Цель въезда"], null, m3.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Первично/Продлено"], null, m4.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["КПП въезда"], null, m5.Id);
                    UpdateOrCreateDocumentParameter(document, prmNames["Отметка проставлена"], null, m2.Id);

                    if (DateTime.TryParseExact(row["дата выдачи"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата выдачи"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["дата въезда"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата въезда"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["дата рег. с"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата регистрации С"], null, null, null, date);
                    }

                    if (DateTime.TryParseExact(row["дата рег. до"], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        UpdateOrCreateDocumentParameter(document, prmNames["Дата регистрации ДО"], null, null, null, date);
                    }

                    i++;
                    progress.Percent = (float)i / table.Count * 100;
                    progress.CurrentRow = i;
                    _db.SaveChanges();
                }
            }
        }

        #region Вспомогательные методы    
        private Misc GetOrCreateMisc(int? miskParentId, string value)
        {
            value = NormalizeString(value, false);

            var misc = _db.Misc.SingleOrDefault(m => m.MiscId == miskParentId && m.MiscValue == value);
            if (misc == null)
            {
                misc = new Misc
                {
                    MiscId = (int)miskParentId,
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
            number = NormalizeString(number, false);

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
            number = NormalizeString(number, false);

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
            number = NormalizeString(number, false);

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
        private PersonFact UpdateOrCreatePersonFact(Person person, ParameterName factName, string strValue = null, int? intValue = null, float? floatValue = null, DateTime? dateValue = null)
        {
            strValue = NormalizeString(strValue, true);

            var fact = _db.PersonFacts.FirstOrDefault(f => f.PersonId == person.Id && f.FactId == factName.Id && f.StringValue == strValue
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
        private DocumentParameter UpdateOrCreateDocumentParameter(Document doc, ParameterName paramName, string strValue = null, int? intValue = null, float? floatValue = null, DateTime? dateValue = null)
        {
            strValue = NormalizeString(strValue, true);

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
        private string NormalizeString(string value, bool retainEmpty = true)
        {
            if (value != null && string.IsNullOrWhiteSpace(value.Replace("-", "")))
            {
                value = retainEmpty ? null : "[пустое значение]";
            }
            return value;
        }
        #endregion

    }
}
