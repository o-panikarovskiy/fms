﻿using Domain.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Person> People { get; set; }
        public DbSet<PersonParameter> PersonParameters { get; set; }
        public DbSet<PersonFact> PersonFacts { get; set; }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentParameter> DocumentParameters { get; set; }
        public DbSet<DocumentFact> DocumentFacts { get; set; }

        public DbSet<PrmFactName> PrmFactNames { get; set; }

        public DbSet<MiscName> MiscNames { get; set; }
        public DbSet<Misc> Misc { get; set; }

        public DbSet<UploadingProgress> UploadProgress { get; set; }
        public DbSet<SearchQuery> SearchQueries { get; set; }


        public ApplicationDbContext() : base("DefaultConnectionString")
        {
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDBInitializer());
        }
    }

    public class ApplicationDBInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var r1 = RoleManager.Create(new IdentityRole("Admin"));
            var r2 = RoleManager.Create(new IdentityRole("User"));

            var adminUser = new ApplicationUser { UserName = "admin" };
            if (UserManager.Create(adminUser, "admin12").Succeeded)
            {
                UserManager.AddToRole(adminUser.Id, "Admin");
                UserManager.AddToRole(adminUser.Id, "User");
            }

            var testUser = new ApplicationUser { UserName = "test" };
            if (UserManager.Create(testUser, "test12").Succeeded)
            {
                UserManager.AddToRole(testUser.Id, "User");
            }

            context.PrmFactNames.Add(new PrmFactName { NameRu = "Адрес", Name = "Address", Category = PrmFactCategory.Person, Type = PrmFactType.Str, IsFact=true });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Гражданство", Name = "Citizenship", Category = PrmFactCategory.Person, Type = PrmFactType.Misc, IsFact = true });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Телефон", Name = "Phone", Category = PrmFactCategory.Person, Type = PrmFactType.Str, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Тип документа", Name = "PrivateDoc", Category = PrmFactCategory.Person, Type = PrmFactType.Misc, IsFact = true });            
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Электронная почта", Name = "Email", Category = PrmFactCategory.Person, Type = PrmFactType.Str, IsFact = false });

            context.PrmFactNames.Add(new PrmFactName { NameRu = "Вид правонарушения", Name = "CrimeType", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата въезда", Name = "IncomeDate", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата выдачи", Name = "IssueDate", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата печати", Name = "PrintDate", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата постановления", Name = "DecreeDate", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата приема заявления", Name = "DateOfReceipt", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата приема", Name = "AdmissionDate", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата регистрации ДО", Name = "RegDateTo", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата регистрации С", Name = "RegDateFrom", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата решения", Name = "DecisionDate", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата составления", Name = "DateCreate", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Дата фактической выдачи", Name = "ActualDate", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Действителен ПО", Name = "ValidTo", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Действителен С", Name = "ValidFrom", Category = PrmFactCategory.Document, Type = PrmFactType.Date, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "КПП въезда", Name = "KPP", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Номер ВНЖ", Name = "VngNo", Category = PrmFactCategory.Document, Type = PrmFactType.Str, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Номер дела", Name = "DocNo2", Category = PrmFactCategory.Document, Type = PrmFactType.Str, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Номер РВП", Name = "RvpNo", Category = PrmFactCategory.Document, Type = PrmFactType.Str, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Номер решения", Name = "DecisionNo", Category = PrmFactCategory.Document, Type = PrmFactType.Str, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Орган рассмотрения", Name = "StateDepartment", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Основание дела", Name = "DocAdmission", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Основание для приема", Name = "AdmissionReason", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Основание решения", Name = "DecisionBase", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Отметка проставлена", Name = "CardMark", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Первично/Продлено", Name = "PrimaryExtend", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Пользователь решения", Name = "DecisionUser", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Принятое решение", Name = "DecreeStr", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Решение", Name = "Decision", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Серия ВНЖ", Name = "SeriesVng", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Статус дела", Name = "DocStatus", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Статья", Name = "Article", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Сумма(начислено)", Name = "SumAccrued", Category = PrmFactCategory.Document, Type = PrmFactType.Float, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Сумма(оплачено)", Name = "SumPaid", Category = PrmFactCategory.Document, Type = PrmFactType.Float, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Тип взыскания", Name = "PenaltyType", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Тип дела", Name = "DocActionType", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Тип решения", Name = "DecisionType", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });
            context.PrmFactNames.Add(new PrmFactName { NameRu = "Цель въезда", Name = "PurposeOfEntry", Category = PrmFactCategory.Document, Type = PrmFactType.Misc, IsFact = false });


            var miscPrivateDoc = new MiscName { NameRu = "Тип документа", Name = "PrivateDoc" };

            context.MiscNames.Add(miscPrivateDoc);
            context.MiscNames.Add(new MiscName { NameRu = "Вид правонарушения", Name = "CrimeType" });
            context.MiscNames.Add(new MiscName { NameRu = "Гражданство", Name = "Citizenship" });
            context.MiscNames.Add(new MiscName { NameRu = "КПП въезда", Name = "KPPMu" });
            context.MiscNames.Add(new MiscName { NameRu = "Орган рассмотрения", Name = "StateDepartment" });
            context.MiscNames.Add(new MiscName { NameRu = "Основание дела (ВНЖ)", Name = "DocAdmissionVng" });
            context.MiscNames.Add(new MiscName { NameRu = "Основание для приема (Гр.)", Name = "AdmissionReasonCitz" });
            context.MiscNames.Add(new MiscName { NameRu = "Основание для приема (РВП)", Name = "AdmissionReasonRvp" });
            context.MiscNames.Add(new MiscName { NameRu = "Основание решения (Гр.)", Name = "DecisionBaseCitz" });
            context.MiscNames.Add(new MiscName { NameRu = "Основание решения (РВП)", Name = "DecisionBaseRvp" });
            context.MiscNames.Add(new MiscName { NameRu = "Отметка проставлена (МУ)", Name = "CardMarkMu" });
            context.MiscNames.Add(new MiscName { NameRu = "Первично/Продлено", Name = "PrimaryExtendMu" });
            context.MiscNames.Add(new MiscName { NameRu = "Пользователь решения (РВП)", Name = "DecisionUserRVP" });
            context.MiscNames.Add(new MiscName { NameRu = "Принятое решение", Name = "DecreeStr" });
            context.MiscNames.Add(new MiscName { NameRu = "Решение (Гр.)", Name = "DecisionCitz" });
            context.MiscNames.Add(new MiscName { NameRu = "Серия (ВНЖ)", Name = "SeriesVng" });
            context.MiscNames.Add(new MiscName { NameRu = "Статус дела", Name = "DocStatus" });
            context.MiscNames.Add(new MiscName { NameRu = "Статья", Name = "Article" });
            context.MiscNames.Add(new MiscName { NameRu = "Тип взыскания", Name = "PenaltyType" });
            context.MiscNames.Add(new MiscName { NameRu = "Тип дела (ВНЖ)", Name = "DocActionTypeVng" });
            context.MiscNames.Add(new MiscName { NameRu = "Тип дела (Гр.)", Name = "DocActionTypeCtz" });
            context.MiscNames.Add(new MiscName { NameRu = "Тип решения (ВНЖ)", Name = "DecisionTypeVng" });
            context.MiscNames.Add(new MiscName { NameRu = "Цель въезда (МУ)", Name = "PurposeOfEntryMu" });

            context.SaveChanges();

            context.Misc.Add(new Misc { MiscId = miscPrivateDoc.Id, MiscValue = "Российский паспорт" });
            context.Misc.Add(new Misc { MiscId = miscPrivateDoc.Id, MiscValue = "ИНН" });

            context.SaveChanges();
        }
    }


}
