create procedure [dbo].[DocumentsOnControl]
AS
begin	
	set nocount on;

	declare @daysShift tinyint; set @daysShift = 5;
	declare @today date; set @today = dateadd(dd, @daysShift, getdate());
	declare @adm tinyint; set @adm = 1;
	declare @rvp tinyint; set @rvp = 2;
	declare @vng tinyint; set @vng = 3;
	declare @ctz tinyint; set @ctz = 4;

	declare @desitionTable table(DocumentId int)
	insert into @desitionTable(DocumentId)
	select sdp.DocumentId
	from ParameterNames as spn 
	inner join DocumentParameters as sdp on sdp.ParameterId = spn.Id
	where spn.Name = 'Дата решения'
	and   spn.[Type] = 3
	and   sdp.DateValue is not null
	

	declare @res table(DocType tinyint, PersonId int, DocId int, DocNo nvarchar(255), DaysCount int, Note nvarchar(255));

	--административная практика
	insert into @res(DocType, PersonId, DocId, DocNo, DaysCount, Note)
	select d.[Type], d.ApplicantPersonId, d.Id, d.Number, 0, null
	from DocumentParameters as dp
	inner join ParameterNames as pn on dp.ParameterId = pn.Id
	inner join Documents as d on d.Id = dp.DocumentId
	where pn.Name = 'Сумма(оплачено)' 
	and   pn.DocType = @adm 
	and   pn.[Type] = 4	
	and   (dp.FloatValue is null or dp.FloatValue = 0)

	
	--гражданство
	declare @daysSpan int; set @daysSpan = 30 * 2 + 15;

	insert into @res(DocType, PersonId, DocId, DocNo, DaysCount, Note)
	select d.[Type], d.ApplicantPersonId, d.Id, d.Number, datediff(dd, @today, dp.DateValue) + @daysSpan + @daysShift, null
	from DocumentParameters as dp	
	inner join ParameterNames as pn on dp.ParameterId = pn.Id
	inner join Documents as d on d.Id = dp.DocumentId	
	where pn.Name = 'Дата приема' 
	and   pn.DocType = @ctz 
	and   pn.[Type] = 3	
	and   dateadd(dd, @daysSpan, dp.DateValue) <= @today
	and   not exists (select DocumentId from @desitionTable as de where de.DocumentId = dp.DocumentId)


	

	--рвп
	declare @rvptable table(DocumentId int, Base nvarchar(255))
	insert into @rvptable(DocumentId, Base)
	select sdp.DocumentId, m.MiscValue
	from DocumentParameters as sdp
	inner join ParameterNames as spn on sdp.ParameterId = spn.Id
	inner join Miscs as m on sdp.IntValue = m.Id and spn.MiscParentId = m.MiscId	
	where spn.Name = 'Основание для приема'
	and   spn.DocType = @rvp
	and   spn.[Type] = 1
	and   not exists (select DocumentId from @desitionTable as de where de.DocumentId = sdp.DocumentId)

	--рвп безвизовый въезд
	set @daysSpan = 40;

	insert into @res(DocType, PersonId, DocId, DocNo, DaysCount, Note)
	select d.[Type], d.ApplicantPersonId, d.Id, d.Number, datediff(dd, @today, dp.DateValue) + @daysSpan + @daysShift, N'(безвизовый въезд)'
	from DocumentParameters as dp
	inner join (
		 select DocumentId
		 from @rvptable as r		 
		 where r.Base like '%(безвизовый въезд)%'
	) as sq on sq.DocumentId = dp.DocumentId
	inner join ParameterNames as pn on dp.ParameterId = pn.Id
	inner join Documents as d on d.Id = dp.DocumentId	
	where pn.Name = 'Дата приема заявления' 
	and   pn.DocType = @rvp 
	and   pn.[Type] = 3	
	and   dateadd(dd, @daysSpan, dp.DateValue) <= @today	

	--рвп визовый въезд
	set @daysSpan = 30 * 4;

	insert into @res(DocType, PersonId, DocId, DocNo, DaysCount, Note)
	select d.[Type], d.ApplicantPersonId, d.Id, d.Number, datediff(dd, @today, dp.DateValue) + @daysSpan + @daysShift, N'(визовый въезд)'
	from DocumentParameters as dp
	inner join (
		 select DocumentId
		 from @rvptable as r		 
		 where r.Base like '%(визовый въезд)%'
	) as sq on sq.DocumentId = dp.DocumentId
	inner join ParameterNames as pn on dp.ParameterId = pn.Id
	inner join Documents as d on d.Id = dp.DocumentId
	where pn.Name = 'Дата приема заявления' 
	and   pn.DocType = @rvp 
	and   pn.[Type] = 3	
	and   dateadd(dd, @daysSpan, dp.DateValue) <= @today

	--внж
	declare @vngtable table(DocumentId int, Base nvarchar(255))		 
	insert into @vngtable(DocumentId, Base)
	select sdp.DocumentId, sbase.MiscValue
	from DocumentParameters as sdp
	inner join ParameterNames as spn on sdp.ParameterId = spn.Id
	inner join Miscs as m on sdp.IntValue = m.Id and spn.MiscParentId = m.MiscId
	inner join (
		 select sdp.DocumentId, m.MiscValue
		 from DocumentParameters as sdp
		 inner join ParameterNames as spn on sdp.ParameterId = spn.Id
		 inner join Miscs as m on sdp.IntValue = m.Id and spn.MiscParentId = m.MiscId
		 where spn.Name = 'Основание дела'
		 and   spn.DocType = @vng
		 and   spn.[Type] = 1
	) as sbase on sbase.DocumentId = sdp.DocumentId	
	where spn.Name = 'Тип дела'
	and   spn.DocType = @vng
	and   spn.[Type] = 1
	and   m.MiscValue like '%Выдача вида на жительство%'
	and   not exists (select DocumentId from @desitionTable as de where de.DocumentId = sdp.DocumentId)



	--внж Международное соглашение
	set @daysSpan = 30 * 2;

	insert into @res(DocType, PersonId, DocId, DocNo, DaysCount, Note)
	select d.[Type], d.ApplicantPersonId, d.Id, d.Number, datediff(dd, @today, dp.DateValue) + @daysSpan + @daysShift, N'(международное соглашение)'
	from DocumentParameters as dp
	inner join (
		 select DocumentId
		 from @vngtable as r		 
		 where r.Base like '%Международное соглашение%'
	) as sq on sq.DocumentId = dp.DocumentId
	inner join ParameterNames as pn on dp.ParameterId = pn.Id
	inner join Documents as d on d.Id = dp.DocumentId
	where pn.Name = 'Дата приема заявления' 
	and   pn.DocType = @vng 
	and   pn.[Type] = 3	
	and   dateadd(dd, @daysSpan, dp.DateValue) <= @today

	--внж высококвалифицированный специалист
	set @daysSpan = 30 * 2;

	insert into @res(DocType, PersonId, DocId, DocNo, DaysCount, Note)
	select d.[Type], d.ApplicantPersonId, d.Id, d.Number, datediff(dd, @today, dp.DateValue) + @daysSpan + @daysShift, N'(высококвалифицированный специалист)'
	from DocumentParameters as dp
	inner join (
		 select DocumentId
		 from @vngtable as r		 
		 where r.Base like '%квалифицир%специалист%'
	) as sq on sq.DocumentId = dp.DocumentId
	inner join ParameterNames as pn on dp.ParameterId = pn.Id
	inner join Documents as d on d.Id = dp.DocumentId
	where pn.Name = 'Дата приема заявления' 
	and   pn.DocType = @vng 
	and   pn.[Type] = 3	
	and   dateadd(dd, @daysSpan, dp.DateValue) <= @today

	--внж носитель русского языка
	set @daysSpan = 30 * 2;

	insert into @res(DocType, PersonId, DocId, DocNo, DaysCount, Note)
	select d.[Type], d.ApplicantPersonId, d.Id, d.Number, datediff(dd, @today, dp.DateValue) + @daysSpan + @daysShift, N'(носитель русского языка)'
	from DocumentParameters as dp
	inner join (
		 select DocumentId
		 from @vngtable as r		 
		 where r.Base like '%русск%язык%'
	) as sq on sq.DocumentId = dp.DocumentId
	inner join ParameterNames as pn on dp.ParameterId = pn.Id
	inner join Documents as d on d.Id = dp.DocumentId
	where pn.Name = 'Дата приема заявления' 
	and   pn.DocType = @vng 
	and   pn.[Type] = 3	
	and   dateadd(dd, @daysSpan, dp.DateValue) <= @today

	--внж остальные
	set @daysSpan = 30 * 4;

	insert into @res(DocType, PersonId, DocId, DocNo, DaysCount, Note)
	select d.[Type], d.ApplicantPersonId, d.Id, d.Number, datediff(dd, @today, dp.DateValue) + @daysSpan + @daysShift, null
	from DocumentParameters as dp
	inner join @vngtable as sq on sq.DocumentId = dp.DocumentId
	inner join ParameterNames as pn on dp.ParameterId = pn.Id
	inner join Documents as d on d.Id = dp.DocumentId
	where pn.Name = 'Дата приема заявления' 
	and   pn.DocType = @vng 
	and   pn.[Type] = 3	
	and   dateadd(dd, @daysSpan, dp.DateValue) <= @today
	and   not exists (select DocId from @res as r where r.DocId = dp.DocumentId)
	
	select r.*, p.Name
	from @res as r
	inner join People as p on p.Id = r.PersonId
end;