create schema [fTests]
    authorization dbo
GO
execute sp_addextendedproperty @name = 'tSQLt.TestClass', @value = 1, @level0type = 'SCHEMA', @level0name = 'fTests'