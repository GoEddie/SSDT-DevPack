CREATE TABLE [dbo].[TableOne]
(
	[Id] INT NOT NULL PRIMARY KEY --always have one of these
/*this is another one*/)
go
create table TableTwo(
	ID int not null,
	constraint [pk_TableTwo] primary key clustered ([ID])
)