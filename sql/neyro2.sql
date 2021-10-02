use uFlights
go
--select * from parse_info

alter table neyro
add tp int;

alter table neyro
add pk uniqueidentifier;

update neyro set tp = 0

create index ind_neyro_tp on neyro(tp);

CREATE TABLE [dbo].[neyro_tmp](
	[id] [int] NOT NULL,
	[dcname] [varchar](255) NULL,
	[usmartname] [varchar](255) NULL,
	[code] [int] NULL,
	[trg_len] [int] NULL,
	[tp] [int] NULL,
	[pk] [uniqueidentifier] NULL,
 CONSTRAINT [pk_neyro_tmp] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

/*
select * from neyro

delete from  parse_info where pi_table = 'neyro'

exec p_parse_info_add 'neyro_tmp', 'id,dcname,usmartname,code,tp,pk'
*/

create procedure p_update_neyro3
as

truncate table neyro3

declare @id int,
		 @str varchar(max)
		 
 declare c cursor local for
 select id, dcname from neyro		  	
 open c
 fetch next from c into @id, @str
 while @@FETCH_STATUS = 0
 begin
 insert into neyro3 (n3_id, trg)
 select @id, trg from dbo.get_trgm (@str)
 fetch next from c into @id, @str
 end
 close c
 deallocate c

 update neyro
 set neyro.trg_len = a.n
 from neyro inner join
 (select n3_id, count(*) n from neyro3
 group by n3_id) a on neyro.id = a.n3_id
 
 
go 

ALTER function [dbo].[get_trgm_id] (@str varchar(max), @tp int)
 returns int
 as
 begin
 declare @id int
 declare @tab  TABLE  ([trg] VARCHAR(3) not null PRIMARY KEY)
 insert into @tab (trg)
 select trg from dbo.get_trgm(@str)
 declare @n int
 select @n = count(*) from @tab
 
 select top 1
 @id = d.id /*, d.usmartname, d.code, */ 
 from neyro d(nolock)
 inner join
 (
 select n.n3_id, count(*) m
 from @tab t inner join neyro3 n(nolock)
 on t.trg = n.trg
 group by n.n3_id
 ) a
 on d.id = a.n3_id and d.tp = @tp
 order by (2.0 * a.m / (1.0 * d.trg_len + 1.0 * @n)) desc	
 return @id
 end
 

GO
insert into neyro
--select * from neyro_tmp

select 
 t.id,
 t.dc_name,
 n.dcname,
 n.pk
 from tariffs_import t(nolock)
 left join neyro n(nolock) on n.id = dbo.get_trgm_id (t.dc_name, 20)
 order by id


select * from tariffs_import
select * from parse_info