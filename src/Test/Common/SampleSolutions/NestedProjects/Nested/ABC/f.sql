CREATE FUNCTION [dbo].[f]()
	RETURNS table
as
	return  select Id, Name from dbo.TheTable where Id != Id
		and Name != Name;