using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;

namespace SSDTDevPack.Common.Dac
{
    public class TableRepository
    {
        private readonly string _path;

        public TableRepository(string path)
        {
            _path = path;
        }

        public List<TableDescriptor> Get()
        {
            var model = Model.Get(_path);
            var dacTables = model.GetObjects<TSqlTable>(DacQueryScopes.UserDefined);

            var tables = dacTables.Select(t => new TableDescriptor(t)).ToList();

            Model.Close(model);

            return tables;
        }
    }
}