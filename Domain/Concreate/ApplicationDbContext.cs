using Domain.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.IO;

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

		public DbSet<ParameterName> ParameterNames { get; set; }

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

			#region Person

			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 1,
				CanRemove = false,
				Name = "Личный документ",
				Category = ParameterCategory.Person,
				PersonCategory = PersonCategory.Individual,
				Type = ParameterType.Misc,
				IsFact = true,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Личный документ",
					PersonCategory = PersonCategory.Individual
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 2,
				CanRemove = false,
				Name = "Гражданство",
				Category = ParameterCategory.Person,
				PersonCategory = PersonCategory.Individual,
				Type = ParameterType.Misc,
				IsFact = true,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Гражданство",
					PersonCategory = PersonCategory.Individual
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 3,
				CanRemove = false,
				Name = "Адрес",
				Category = ParameterCategory.Person,
				PersonCategory = PersonCategory.Individual | PersonCategory.Legal,
				Type = ParameterType.Str,
				IsFact = true
			});

			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 4,
				CanRemove = false,
				Name = "Телефон",
				Category = ParameterCategory.Person,
				PersonCategory = PersonCategory.Individual | PersonCategory.Legal,
				Type = ParameterType.Str,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 5,
				CanRemove = false,
				Name = "Электронная почта",
				Category = ParameterCategory.Person,
				PersonCategory = PersonCategory.Individual | PersonCategory.Legal,
				Type = ParameterType.Str,
				IsFact = false
			});
			#endregion

			#region Административная практика
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 5,
				CanRemove = false,
				Name = "Вид правонарушения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Вид правонарушения",
					DocType = DocumentType.AdministrativePractice
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 6,
				CanRemove = false,
				Name = "Дата постановления",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 7,
				CanRemove = false,
				Name = "Дата составления",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 8,
				CanRemove = false,
				Name = "Орган рассмотрения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Орган рассмотрения",
					DocType = DocumentType.AdministrativePractice
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 9,
				CanRemove = false,
				Name = "Принятое решение",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Принятое решение",
					DocType = DocumentType.AdministrativePractice
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 10,
				CanRemove = false,
				Name = "Тип взыскания",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Тип взыскания",
					DocType = DocumentType.AdministrativePractice
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 11,
				CanRemove = false,
				Name = "Статус дела",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Статус дела",
					DocType = DocumentType.AdministrativePractice
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 12,
				CanRemove = false,
				Name = "Статья",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Статья",
					DocType = DocumentType.AdministrativePractice
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 13,
				CanRemove = false,
				Name = "Сумма(начислено)",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Float,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 14,
				CanRemove = false,
				Name = "Сумма(оплачено)",
				Category = ParameterCategory.Document,
				DocType = DocumentType.AdministrativePractice,
				Type = ParameterType.Float,
				IsFact = false
			});
			#endregion

			#region Гражданство
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 15,
				CanRemove = false,
				Name = "Тип дела",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Citizenship,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Тип дела",
					DocType = DocumentType.Citizenship
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 16,
				CanRemove = false,
				Name = "Дата приема",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Citizenship,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 17,
				CanRemove = false,
				Name = "Дата решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Citizenship,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 18,
				CanRemove = false,
				Name = "Номер решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Citizenship,
				Type = ParameterType.Str,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 19,
				CanRemove = false,
				Name = "Основание для приема",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Citizenship,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Основание для приема",
					DocType = DocumentType.Citizenship
				})

			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 20,
				CanRemove = false,
				Name = "Основание решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Citizenship,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Основание решения",
					DocType = DocumentType.Citizenship
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 21,
				CanRemove = false,
				Name = "Решение",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Citizenship,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName { Name = "Решение", DocType = DocumentType.Citizenship })
			});
			#endregion

			#region РВП
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 22,
				CanRemove = false,
				Name = "Дата печати",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 23,
				CanRemove = false,
				Name = "Дата приема заявления",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 24,
				CanRemove = false,
				Name = "Дата решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 25,
				CanRemove = false,
				Name = "Дата фактической выдачи",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 26,
				CanRemove = false,
				Name = "Номер РВП",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Str,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 27,
				CanRemove = false,
				Name = "Номер решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Str,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 28,
				CanRemove = false,
				Name = "Пользователь решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Пользователь решения",
					DocType = DocumentType.TemporaryResidencePermit
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 29,
				CanRemove = false,
				Name = "Основание для приема",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Основание для приема",
					DocType = DocumentType.TemporaryResidencePermit
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 30,
				CanRemove = false,
				Name = "Основание решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.TemporaryResidencePermit,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Основание решения",
					DocType = DocumentType.TemporaryResidencePermit
				})
			});
			#endregion

			#region ВНЖ
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 31,
				CanRemove = false,
				Name = "Дата выдачи",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 32,
				CanRemove = false,
				Name = "Дата приема заявления",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 33,
				CanRemove = false,
				Name = "Дата решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 34,
				CanRemove = false,
				Name = "Дата фактической выдачи",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 35,
				CanRemove = false,
				Name = "Действителен С",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 36,
				CanRemove = false,
				Name = "Действителен ПО",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Date,
				IsFact = false
			});			
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 37,
				CanRemove = false,
				Name = "Номер ВНЖ",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Str,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 38,
				CanRemove = false,
				Name = "Номер дела",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Str,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 39,
				CanRemove = false,
				Name = "Основание дела",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Основание дела",
					DocType = DocumentType.Residence
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 40,
				CanRemove = false,
				Name = "Серия ВНЖ",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Серия",
					DocType = DocumentType.Residence
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 41,
				CanRemove = false,
				Name = "Тип дела",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Тип дела",
					DocType = DocumentType.Residence
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 42,
				CanRemove = false,
				Name = "Тип решения",
				Category = ParameterCategory.Document,
				DocType = DocumentType.Residence,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Тип решения",
					DocType = DocumentType.Residence
				})
			});
			#endregion

			#region  Миграционный учёт
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 43,
				CanRemove = false,
				Name = "Дата выдачи",
				Category = ParameterCategory.Document,
				DocType = DocumentType.MigrationRegistration,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 44,
				CanRemove = false,
				Name = "Дата въезда",
				Category = ParameterCategory.Document,
				DocType = DocumentType.MigrationRegistration,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 45,
				CanRemove = false,
				Name = "Дата регистрации ДО",
				Category = ParameterCategory.Document,
				DocType = DocumentType.MigrationRegistration,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 46,
				CanRemove = false,
				Name = "Дата регистрации С",
				Category = ParameterCategory.Document,
				DocType = DocumentType.MigrationRegistration,
				Type = ParameterType.Date,
				IsFact = false
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 47,
				CanRemove = false,
				Name = "КПП въезда",
				Category = ParameterCategory.Document,
				DocType = DocumentType.MigrationRegistration,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "КПП въезда",
					DocType = DocumentType.MigrationRegistration
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 48,
				CanRemove = false,
				Name = "Отметка проставлена",
				Category = ParameterCategory.Document,
				DocType = DocumentType.MigrationRegistration,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Отметка проставлена",
					DocType = DocumentType.MigrationRegistration
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 49,
				CanRemove = false,
				Name = "Первично/Продлено",
				Category = ParameterCategory.Document,
				DocType = DocumentType.MigrationRegistration,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Первично/Продлено",
					DocType = DocumentType.MigrationRegistration
				})
			});
			context.ParameterNames.Add(new ParameterName
			{
				OrderIndex = 50,
				CanRemove = false,
				Name = "Цель въезда",
				Category = ParameterCategory.Document,
				DocType = DocumentType.MigrationRegistration,
				Type = ParameterType.Misc,
				IsFact = false,
				MiscParent = context.MiscNames.Add(new MiscName
				{
					Name = "Цель въезда",
					DocType = DocumentType.MigrationRegistration
				})
			});
			#endregion



			var appDomain = System.AppDomain.CurrentDomain;
			var basePath = appDomain.RelativeSearchPath ?? appDomain.BaseDirectory;
			var path = Path.Combine(basePath, "SQL\\DocumentsOnControl.sql");
			var sqlScript = File.ReadAllText(path);

			context.Database.ExecuteSqlCommand(sqlScript);

			context.SaveChanges();
		}
	}


}
