using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Extensions.Prototype;

namespace SSDTDevPack.Common.Dac
{
    public class Model
    {
        static readonly Dictionary<string, ModelReference> Models = new Dictionary<string, ModelReference>(); 

        public static TSqlTypedModel Get(string path)
        {
            lock (Models)
            {
                if (Models.ContainsKey(path))
                {
                    var reference = Models[path];
                    reference.ReferenceCount++;
                    return reference.Model;
                }

                var newReference = new ModelReference();
                newReference.Model = new TSqlTypedModel(path);
                newReference.ReferenceCount = 1;
                Models.Add(path, newReference);
                return newReference.Model;
            }
        }

        public static void Close(string path)
        {
            lock (Models)
            {
                if (!Models.ContainsKey(path))
                    return;

                var reference = Models[path];
                reference.ReferenceCount--;

                if (reference.ReferenceCount <= 0)
                {
                    reference.Model.Dispose();
                    Models.Remove(path);
                }
            }
        }
    }

    class ModelReference
    {
        public int ReferenceCount;
        public TSqlTypedModel Model;
    }
}