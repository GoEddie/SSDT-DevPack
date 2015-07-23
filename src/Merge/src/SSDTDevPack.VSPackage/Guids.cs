// Guids.cs
// MUST match guids.h
using System;

namespace TheAgileSQLClub.SSDTDevPack_VSPackage
{
    static class GuidList
    {
        public const string guidSSDTDevPack_VSPackagePkgString = "354e4235-c369-4a8f-a855-0318891c0903";
        public const string guidSSDTDevPack_VSPackageCmdSetString = "a60f097a-ca84-43f0-b08d-a45b6ccd476a";
        public const string guidToolWindowPersistanceString = "978c49d6-2808-4a7a-bf32-274bec085942";

        public static readonly Guid guidSSDTDevPack_VSPackageCmdSet = new Guid(guidSSDTDevPack_VSPackageCmdSetString);
    };
}