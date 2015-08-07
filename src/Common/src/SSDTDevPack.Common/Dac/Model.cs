using Microsoft.SqlServer.Dac.Extensions.Prototype;

namespace SSDTDevPack.Common.Dac
{
    public class Model
    {
        public TSqlTypedModel Get(string path)
        {
            return new TSqlTypedModel(path);
        }
    }
}