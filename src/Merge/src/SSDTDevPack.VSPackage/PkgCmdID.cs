// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace TheAgileSQLClub.SSDTDevPack_VSPackage
{
    static class PkgCmdIDList
    {
        public const uint tSQLtGroup = 0x1011;
        public const uint SSDTDevQueryCostGroup = 0x1012;
        public const uint tSQLtSubMenuGroup = 0x1013;
        public const uint ConstraintsRefactorGroup = 0x1014;

        public const uint SubMenu = 0x1020;
        public const uint SSDTDevPack = 0x99;
        
        public const uint SSDTDevPackQuickDeploy = 0x100;
        
        public const uint SSDTDevPackMergeUi = 0x101;
        
        public const uint SSDTDevPackNameConstraints = 0x102;
        
        public const uint SSDTDevPackCreatetSQLtSchema = 0x103;
        public const uint SSDTDevPackCreatetSQLtTestStub = 0x104;

        public const uint SSDTDevPackToggleQueryCosts = 0x105;
        public const uint SSDTDevPackClearQueryCosts = 0x106;
        

    };
}