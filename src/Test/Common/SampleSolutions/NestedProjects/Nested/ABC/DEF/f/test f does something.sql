CREATE PROCEDURE [f].[test f does something]
AS
    EXECUTE tSQLt.FakeTable 'dbo', 'TheTable';
    SELECT *
    FROM   [dbo].[f]();
    EXECUTE tSQLt.AssertEquals 'TRUE', 'FALSE', N'Error Not Implemented';