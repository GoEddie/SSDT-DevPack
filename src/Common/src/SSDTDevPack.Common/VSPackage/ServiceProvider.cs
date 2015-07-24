using System;

namespace SSDTDevPack.Common.VSPackage
{
    public static class VsServiceProvider
    {
        private static IVsServiceProvider _provider;

        public static void Register(IVsServiceProvider provider)
        {
            _provider = provider;
        }

        public static object Get(Type type)
        {
            return _provider.GetVsService(type);
        }
    }
}