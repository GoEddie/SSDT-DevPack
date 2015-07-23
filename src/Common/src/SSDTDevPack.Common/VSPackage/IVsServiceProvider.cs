using System;

namespace SSDTDevPack.Common.VSPackage
{
    public interface IVsServiceProvider
    {
        object GetVsService(Type type);
    }
}