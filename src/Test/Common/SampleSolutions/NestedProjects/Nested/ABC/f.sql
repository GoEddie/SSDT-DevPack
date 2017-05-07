CREATE FUNCTION [dbo].[f]()
	RETURNS TABLE
AS
	RETURN  SELECT Id, Name FROM dbo.TheTable WHERE Id != Id
		AND Name != Name;

GO


create index ix_a on dbo.thetable (id)
go
create index ix_a2 on dbo.thetable (id)
go
create index ix_a3 on dbo.thetable (id)
go