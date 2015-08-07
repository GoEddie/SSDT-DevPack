using Microsoft.SqlServer.Dac.Extensions.Prototype;

namespace SSDTDevPack.Common.Dac
{
    public class Model
    {
        public static TSqlTypedModel Get(string path)
        {
            return new TSqlTypedModel(path);
        }

        public static void Close(TSqlTypedModel model)
        {
            model.Dispose();
        }
    }
}