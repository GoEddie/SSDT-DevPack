CREATE FUNCTION [dbo].[f]()
	RETURNS TABLE
AS
	RETURN  SELECT Id, Name FROM dbo.TheTable WHERE Id != Id
		AND Name != Name;

GO


CREATE INDEX ix_a ON dbo.thetable (id)
go
CREATE INDEX ix_a2 ON dbo.thetable (id)
go
CREATE INDEX ix_a3 ON dbo.thetable (id)
go

