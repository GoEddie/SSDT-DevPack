# SSDT-DevPack

[![Join the chat at https://gitter.im/GoEddie/SSDT-DevPack](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/GoEddie/SSDT-DevPack?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

It is basically a collection of tools that make developing in for SQL Server in SSDT better or easier. If anyone has any ideas for a tool that they would find useful please feel free to fork it on github and add it.

For full details of what this is see: <https://the.agilesql.club/Projects/SSDT-Dev-Pack>

For the latest release grab it from: <https://github.com/GoEddie/SSDT-DevPack/blob/master/release/SSDTDevPack.VSPackage.Latest.vsix>

For help open an issue or use the gitter room.


## Features

The first one is MergeUi which basically (as you might imagine) puts a gui around merger statements in post deployment scripts to make it simple to deploy static or reference data with an SSDT project

 - Re-write unnamed primary keys into named table constraints
 - Create tSQLt schemas (classes) including the extended property everyone always forgets
 - Take a stored procedure or tvf and create a tSQLt test for it including faking every take in the procedure and creating the parameters needed for the procedure and calling it
 - When developing a stored procedure (or tvf) configure a connection to a database with realistic statistics and have high costing queries hightlighted so it is easy to see queries that will cause issues (not a 100% catch all obviously)


## License
MIT
