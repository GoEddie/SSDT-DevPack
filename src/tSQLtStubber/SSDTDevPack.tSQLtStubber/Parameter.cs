

using Microsoft.SqlServer.Dac.Model;

namespace SSDTDevPack.tSQLtStubber
{
    public class Parameter
    {
        public string Name;
        public SqlDataType Type;

        public Parameter(string name, SqlDataType type)
        {
            Name = name;
            Type = type;
        }
    }
}