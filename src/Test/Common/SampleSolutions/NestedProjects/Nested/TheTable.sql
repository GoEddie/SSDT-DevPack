CREATE TABLE [dbo].[TheTable]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[name] varchar(max) null,
	[fun] int
)
GO
CREATE TABLE [dbo].[NoInlineTable]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[name] varchar(max) null,
	[fun] int
)
GO
CREATE TABLE [dbo].[NoUpdate]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[name] varchar(max) null,
	[fun] int
)
GO
CREATE TABLE [dbo].[NoInsert]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[name] varchar(max) null,
	[fun] int
)
GO
CREATE TABLE [dbo].[NoDelete]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[name] varchar(max) null,
	[fun] int
)
GO

CREATE TABLE [dbo].[NoSchema]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[name] varchar(max) null,
	[fun] int
)