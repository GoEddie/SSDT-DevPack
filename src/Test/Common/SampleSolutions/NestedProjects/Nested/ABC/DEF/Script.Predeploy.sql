print N'@@LOCK_TIMEOUT:  ' + CAST(@@LOCK_TIMEOUT as nvarchar);


go


--MERGE INTO dbo.TheTable
-- AS TARGET
--USING (VALUES ) AS SOURCE(Id, name, fun) ON [SOURCE].[Id] = [TARGET].[Id]
--WHEN NOT MATCHED BY TARGET THEN INSERT (Id, name, fun) VALUES (SOURCE.Id, SOURCE.name, SOURCE.fun)
--WHEN MATCHED AND (NULLIF (SOURCE.fun, TARGET.fun) IS NOT NULL
--                  OR NULLIF (SOURCE.name, TARGET.name) IS NOT NULL
--                  OR NULLIF (SOURCE.Id, TARGET.Id) IS NOT NULL) THEN UPDATE 
--SET TARGET.Id   = SOURCE.Id,
--    TARGET.name = SOURCE.name,
--    TARGET.fun  = SOURCE.fun
--WHEN NOT MATCHED BY SOURCE THEN DELETE;GO
MERGE INTO dbo.TheTable
 AS TARGET
USING (VALUES (1, 'Ed', 999), (6, 'hhhhhh', 0), (3, 'AARDVARK', 1), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (1, 'Ed', 999), (6, 'hhhhhh', 0), (3, 'AARDVARK', 1), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (1, 'Ed', 999), (6, 'hhhhhh', 0), (3, 'AARDVARK', 1), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (1, 'Ed', 999), (6, 'hhhhhh', 0), (3, 'AARDVARK', 1), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL),(1, 'Ed', 999), (6, 'hhhhhh', 0), (3, 'AARDVARK', 1), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL),(1, 'Ed', 999), (6, 'hhhhhh', 0), (3, 'AARDVARK', 1), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL),(1, 'Ed', 999), (6, 'hhhhhh', 0), (3, 'AARDVARK', 1), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL),(1, 'Ed', 999), (6, 'hhhhhh', 0), (3, 'AARDVARK', 1), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (ss, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL), (s, NULL, NULL)) AS SOURCE(Id, name, fun) ON [SOURCE].[Id] = [TARGET].[Id]
WHEN NOT MATCHED BY TARGET THEN INSERT ([Id], [name], [fun]) VALUES ([SOURCE].[Id], [SOURCE].[name], [SOURCE].[fun])
WHEN MATCHED AND (NULLIF ([SOURCE].[fun], [TARGET].[fun]) IS NOT NULL
                  OR NULLIF ([SOURCE].[name], [TARGET].[name]) IS NOT NULL
                  OR NULLIF ([SOURCE].[Id], [TARGET].[Id]) IS NOT NULL) THEN UPDATE 
SET [TARGET].[Id]   = [SOURCE].[Id],
    [TARGET].[name] = [SOURCE].[name],
    [TARGET].[fun]  = [SOURCE].[fun]
WHEN NOT MATCHED BY SOURCE THEN DELETE;GO
GO
MERGE INTO dbo.TheTable
 AS TARGET
USING (VALUES (1, 'Ed', 1), (6, 'hhdsh', 66), (3, 'AARDVARK', 2), (3, NULL, 3), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (33, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL), (33, NULL, NULL), (3, NULL, NULL), (3, NULL, NULL)) AS SOURCE(Id, name, fun) ON [SOURCE].[Id] = [TARGET].[Id]
WHEN NOT MATCHED BY TARGET THEN INSERT ([Id], [name], [fun]) VALUES ([SOURCE].[Id], [SOURCE].[name], [SOURCE].[fun])
WHEN MATCHED AND (NULLIF ([SOURCE].[fun], [TARGET].[fun]) IS NOT NULL
                  OR NULLIF ([SOURCE].[name], [TARGET].[name]) IS NOT NULL
                  OR NULLIF ([SOURCE].[Id], [TARGET].[Id]) IS NOT NULL) THEN UPDATE 
SET [TARGET].[Id]   = [SOURCE].[Id],
    [TARGET].[name] = [SOURCE].[name],
    [TARGET].[fun]  = [SOURCE].[fun]
WHEN NOT MATCHED BY SOURCE THEN DELETE;GO
MERGE INTO dbo.NoDelete
 AS TARGET
USING (VALUES (1, 'Ed', 1), (2, 'Ian', 0)) AS SOURCE(Id, name, fun) ON [SOURCE].[Id] = [TARGET].[Id]
WHEN NOT MATCHED BY TARGET THEN INSERT ([Id], [name], [fun]) VALUES ([SOURCE].[Id], [SOURCE].[name], [SOURCE].[fun])
WHEN MATCHED AND (NULLIF ([SOURCE].[fun], [TARGET].[fun]) IS NOT NULL
                  OR NULLIF ([SOURCE].[name], [TARGET].[name]) IS NOT NULL
                  OR NULLIF ([SOURCE].[Id], [TARGET].[Id]) IS NOT NULL) THEN UPDATE 
SET [TARGET].[Id]   = [SOURCE].[Id],
    [TARGET].[name] = [SOURCE].[name],
    [TARGET].[fun]  = [SOURCE].[fun]
WHEN NOT MATCHED BY SOURCE THEN DELETE;GO
MERGE INTO dbo.NoInlineTable
 AS TARGET
USING (VALUES (1, 'Ed', 1), (2, 'Ian', 0)) AS SOURCE(Id, name, fun) ON [SOURCE].[Id] = [TARGET].[Id]
WHEN NOT MATCHED BY TARGET THEN INSERT ([Id], [name], [fun]) VALUES ([SOURCE].[Id], [SOURCE].[name], [SOURCE].[fun])
WHEN MATCHED AND (NULLIF ([SOURCE].[fun], [TARGET].[fun]) IS NOT NULL
                  OR NULLIF ([SOURCE].[name], [TARGET].[name]) IS NOT NULL
                  OR NULLIF ([SOURCE].[Id], [TARGET].[Id]) IS NOT NULL) THEN UPDATE 
SET [TARGET].[Id]   = [SOURCE].[Id],
    [TARGET].[name] = [SOURCE].[name],
    [TARGET].[fun]  = [SOURCE].[fun]
WHEN NOT MATCHED BY SOURCE THEN DELETE;GO
MERGE INTO dbo.NoSchema
 AS TARGET
USING (VALUES (1, 'Ed', 1), (2, 'Ian', 0)) AS SOURCE(Id, name, fun) ON [SOURCE].[Id] = [TARGET].[Id]
WHEN NOT MATCHED BY TARGET THEN INSERT ([Id], [name], [fun]) VALUES ([SOURCE].[Id], [SOURCE].[name], [SOURCE].[fun])
WHEN MATCHED AND (NULLIF ([SOURCE].[fun], [TARGET].[fun]) IS NOT NULL
                  OR NULLIF ([SOURCE].[name], [TARGET].[name]) IS NOT NULL
                  OR NULLIF ([SOURCE].[Id], [TARGET].[Id]) IS NOT NULL) THEN UPDATE 
SET [TARGET].[Id]   = [SOURCE].[Id],
    [TARGET].[name] = [SOURCE].[name],
    [TARGET].[fun]  = [SOURCE].[fun]
WHEN NOT MATCHED BY SOURCE THEN DELETE;GO
MERGE INTO dbo.NoUpdate
 AS TARGET
USING (VALUES (1, 'Edsdsdsdsds', 1), (9999, 'HASTINGS', 1), (200000, 'Beer', 0)) AS SOURCE(Id, name, fun) ON [SOURCE].[Id] = [TARGET].[Id]
WHEN NOT MATCHED BY TARGET THEN INSERT ([Id], [name], [fun]) VALUES ([SOURCE].[Id], [SOURCE].[name], [SOURCE].[fun])
WHEN MATCHED AND (NULLIF ([SOURCE].[fun], [TARGET].[fun]) IS NOT NULL
                  OR NULLIF ([SOURCE].[name], [TARGET].[name]) IS NOT NULL
                  OR NULLIF ([SOURCE].[Id], [TARGET].[Id]) IS NOT NULL) THEN UPDATE 
SET [TARGET].[Id]   = [SOURCE].[Id],
    [TARGET].[name] = [SOURCE].[name],
    [TARGET].[fun]  = [SOURCE].[fun]
WHEN NOT MATCHED BY SOURCE THEN DELETE;