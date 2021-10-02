use uFlights
go
--drop table neyro
CREATE TABLE neyro
(
  id integer NOT NULL,
  dcname varchar(255),
  usmartname varchar(255),
  code integer,
  CONSTRAINT pk_neyro PRIMARY KEY (id)
)
go

/*
exec p_parse_info_add 'neyro', 'id,dcname,usmartname,code'

select * from parse_info

update parse_info
set pi_type = 'numeric'
where pi_nn in (28, 29, 30)


exec p_parse_info_add 'tariffs_import', 'id,nn,dc_name,SV_NN,mtow,rm_name,tf_tariffmain,tf_payexec,tf_paymin,tf_round,tf_reportname,tf_comment,tf_lowlimit,st_code,tf_fldirection,tf_fltype,tf_flcategory'

select * from tariffs_import
select * from neyro
*/
/*
declare @str varchar(max)
set @str = '			       ass           ssde		
		 rterr		rterr
		 
		 	           yyyy		 '
		 	           
select * from dbo.get_trgm(@str)
*/
create function get_trgm (@str varchar(max))
returns @trgm_tab TABLE  ([trg] VARCHAR(3) not null PRIMARY KEY)
AS
begin
set @str = replace(@str, '	', ' ')
set @str = replace(@str, '
', ' ')


while (CHARINDEX('  ', @str) > 0)
begin
	set @str = replace(@str, '  ', ' ')
end
set @str = ltrim(rtrim(@str))
set @str = ' ' + replace(@str, ' ', '  ') + ' '

declare @i int
declare @n int
declare @trgm varchar(3)
set @n = len(@str)
set @i = 1
while (@i <= @n - 2)
begin
if not (SUBSTRING(@str, @i + 1, 1) = ' ' and  SUBSTRING(@str, @i + 2, 1) = ' ')
	begin
		set  @trgm = SUBSTRING(@str, @i, 1) + SUBSTRING(@str, @i + 1, 1) + SUBSTRING(@str, @i + 2, 1)
		if (not exists(select trg from @trgm_tab where trg = @trgm))
			insert into @trgm_tab  (trg) values(@trgm)
	end  
	
set @i = @i + 1	                  
end 

return
end
go


create table neyro3
(n3_pk uniqueidentifier not null default(newid()) primary key,
 n3_id int not null,
 trg varchar(3)
 )
 go
 
 create index ind_neyro3_id on neyro3(n3_id)
 
 go
 
 create index ind_neyro3_trg on neyro3(trg)
 
 go
 
 /*
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
 */
 
 alter table dbo.neyro
 add trg_len int
 go
 /*
 update neyro
 set neyro.trg_len = a.n
 from neyro inner join
 (select n3_id, count(*) n from neyro3
 group by n3_id) a on neyro.id = a.n3_id
 */
 create function get_trgm_id (@str varchar(max))
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
 on d.id = a.n3_id
 order by (2.0 * a.m / (1.0 * d.trg_len + 1.0 * @n)) desc	
 return @id
 end
 
 go
 
 
 select 
 t.id,
 t.dc_name,
 n.usmartname,
 n.code
 from tariffs_import t(nolock)
 left join neyro n(nolock) on n.id = dbo.get_trgm_id (t.dc_name)
 order by id
 /*
 declare @str varchar(max)
 set @str = 'Стремянка "Технический трап"'
 */
 