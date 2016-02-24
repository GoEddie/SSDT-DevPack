using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;

namespace SSDTDevPack.Common.Enumerators.tSQLt
{
    public class TestClassEnumerator
    {
        public TestClassEnumerator(string path)
        {
            var tSQLtName = new SchemaObjectName();
            tSQLtName.Identifiers.Add(new Identifier() {Value = "tSQLt"});
            tSQLtName.Identifiers.Add(new Identifier() { Value = "TestClass" });

            
            var model = Model.Get(path);
            var extendedProperties = model.GetObjects<TSqlExtendedProperty>(DacQueryScopes.UserDefined).Where(s => s.Name.EqualsName(tSQLtName));

            foreach (var prop in extendedProperties)
            {
                Console.WriteLine(prop.Name);
            }

            //_tables = dacTables.Select(t => new TableDescriptor(t)).ToList();



            Model.Close(path);
        }
    }
}
