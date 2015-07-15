using Domain.Models;
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

        public DbSet<ParameterName> PrmFactNames { get; set; }

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

            context.PrmFactNames.Add(new ParameterName { Name = "Адрес", Category = ParameterCategory.Person, Type = ParameterType.Str, IsFact = true });
            context.PrmFactNames.Add(new ParameterName { Name = "Гражданство", Category = ParameterCategory.Person, Type = ParameterType.Misc, IsFact = true });
            context.PrmFactNames.Add(new ParameterName { Name = "Телефон", Category = ParameterCategory.Person, Type = ParameterType.Str, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Тип документа", Category = ParameterCategory.Person, Type = ParameterType.Misc, IsFact = true });
            context.PrmFactNames.Add(new ParameterName { Name = "Электронная почта", Category = ParameterCategory.Person, Type = ParameterType.Str, IsFact = false });

            context.PrmFactNames.Add(new ParameterName { Name = "Вид правонарушения", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата въезда", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата выдачи", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата печати", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата постановления", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата приема заявления", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата приема", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата регистрации ДО", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата регистрации С", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата решения", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата составления", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Дата фактической выдачи", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Действителен ПО", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Действителен С", Category = ParameterCategory.Document, Type = ParameterType.Date, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "КПП въезда", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Номер ВНЖ", Category = ParameterCategory.Document, Type = ParameterType.Str, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Номер дела", Category = ParameterCategory.Document, Type = ParameterType.Str, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Номер РВП", Category = ParameterCategory.Document, Type = ParameterType.Str, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Номер решения", Category = ParameterCategory.Document, Type = ParameterType.Str, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Орган рассмотрения", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Основание дела", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Основание для приема", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Основание решения", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Отметка проставлена", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Первично/Продлено", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Пользователь решения", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Принятое решение", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Решение", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Серия ВНЖ", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Статус дела", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Статья", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Сумма(начислено)", Category = ParameterCategory.Document, Type = ParameterType.Float, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Сумма(оплачено)", Category = ParameterCategory.Document, Type = ParameterType.Float, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Тип взыскания", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Тип дела", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Тип решения", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });
            context.PrmFactNames.Add(new ParameterName { Name = "Цель въезда", Category = ParameterCategory.Document, Type = ParameterType.Misc, IsFact = false });


            var miscPrivateDoc = new MiscName { Name = "Тип документа" };

            context.MiscNames.Add(miscPrivateDoc);
            context.MiscNames.Add(new MiscName { Name = "Вид правонарушения" });
            context.MiscNames.Add(new MiscName { Name = "Гражданство" });
            context.MiscNames.Add(new MiscName { Name = "КПП въезда" });
            context.MiscNames.Add(new MiscName { Name = "Орган рассмотрения" });
            context.MiscNames.Add(new MiscName { Name = "Основание дела (ВНЖ)" });
            context.MiscNames.Add(new MiscName { Name = "Основание для приема (Гр.)" });
            context.MiscNames.Add(new MiscName { Name = "Основание для приема (РВП)" });
            context.MiscNames.Add(new MiscName { Name = "Основание решения (Гр.)" });
            context.MiscNames.Add(new MiscName { Name = "Основание решения (РВП)" });
            context.MiscNames.Add(new MiscName { Name = "Отметка проставлена (МУ)" });
            context.MiscNames.Add(new MiscName { Name = "Первично/Продлено" });
            context.MiscNames.Add(new MiscName { Name = "Пользователь решения (РВП)" });
            context.MiscNames.Add(new MiscName { Name = "Принятое решение" });
            context.MiscNames.Add(new MiscName { Name = "Решение (Гр.)" });
            context.MiscNames.Add(new MiscName { Name = "Серия (ВНЖ)" });
            context.MiscNames.Add(new MiscName { Name = "Статус дела" });
            context.MiscNames.Add(new MiscName { Name = "Статья" });
            context.MiscNames.Add(new MiscName { Name = "Тип взыскания" });
            context.MiscNames.Add(new MiscName { Name = "Тип дела (ВНЖ)" });
            context.MiscNames.Add(new MiscName { Name = "Тип дела (Гр.)" });
            context.MiscNames.Add(new MiscName { Name = "Тип решения (ВНЖ)" });
            context.MiscNames.Add(new MiscName { Name = "Цель въезда (МУ)" });

            context.SaveChanges();

            context.Misc.Add(new Misc { MiscId = miscPrivateDoc.Id, MiscValue = "Российский паспорт" });
            context.Misc.Add(new Misc { MiscId = miscPrivateDoc.Id, MiscValue = "Иностранный паспорт" });

            context.SaveChanges();
        }
    }


}
