// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace TheAgileSQLClub.SSDTDevPack_VSPackage
{
    static class PkgCmdIDList
    {
        public const uint SSDTDevPack = 0x99;
        public const uint SSDTDevPackQuickDeploy = 0x100;
        public const uint SSDTDevPackMergeUi = 0x101;
        public const uint SSDTDevPackCodeCoverage = 0x98;
        public const uint SSDTDevPackNameConstraints = 0x102;
        public const uint SSDTDevPackCreatetSQLtSchema = 0x103;
        public const uint SSDTDevPackCreatetSQLtTestStub = 0x104;
        public const uint SSDTDevPackToggleQueryCosts = 0x105;
        public const uint SSDTDevPackClearQueryCosts = 0x106;
        public const uint SSDTDevPackLowerCase = 0x107;
        public const uint SSDTDevPackUpperCase = 0x108;
        public const uint SSDTDevPackExtractToTvf = 0x109;
        public const uint SSDTDevPackFindDuplicateIndexes = 0x111;
        public const uint SSDTNonSargableRewrites = 0x112;
        public const uint SSDTTSqlClippy = 0x113;
        public const uint SSDTDevPackCorrectCase = 0x114;
        public const uint SSDTDevPackToggleCodeCoverageDisplay = 0x115;
        public const uint SSDTDevPackQuickDeployToClipboard = 0x116;
        public const uint SSDTDevPackQuickDeployAppendToClipboard = 0x117;
        public const uint SSDTDevPackQuickDeployClearConnection = 0x118;



        public const uint SSDTDevPackDeprecatedWarning = 0x119;

        public const uint CasingGroup = 0x1010;
        public const uint tSQLtGroup = 0x1011;
        public const uint SSDTDevQueryCostGroup = 0x1012;
        public const uint tSQLtSubMenuGroup = 0x1013;
        public const uint ConstraintsRefactorGroup = 0x1014;
        public const uint SubMenu = 0x1020;

        public const uint DeprecatedGroup = 0x1015;


    };
}