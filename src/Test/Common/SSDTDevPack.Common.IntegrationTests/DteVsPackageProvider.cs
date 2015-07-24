using System;
using EnvDTE;
using SSDTDevPack.Common.VSPackage;

namespace SSDTDevPack.Common.IntegrationTests
{
    class DteVsPackageProvider : IVsServiceProvider
    {
        private readonly DTE _dte;

        public DteVsPackageProvider(DTE dte)
        {
            _dte = dte;
        }

        public object GetVsService(Type type)
        {
            return _dte;
        }
    }
}