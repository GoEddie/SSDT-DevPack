using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.SqlServer.Dac.Extensions;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Common.ProjectVersion
{
    public static class VersionDetector
    {
        static VersionDetector()
        {
            Platform = TSqlPlatforms.Sql130;
            
        }
        public static TSqlParser ParserFactory(bool quotedIdentifiers = true)
        {
            switch (Platform)
            {
                case TSqlPlatforms.Sql90:
                    return new TSql90Parser(quotedIdentifiers);
                case TSqlPlatforms.Sql100:
                    return new TSql100Parser(quotedIdentifiers);
                case TSqlPlatforms.SqlAzure:
                    return new TSql120Parser(quotedIdentifiers);
                case TSqlPlatforms.Sql110:
                    return new TSql110Parser(quotedIdentifiers);
                case TSqlPlatforms.Sql120:
                    return new TSql120Parser(quotedIdentifiers);
                case TSqlPlatforms.Sql130:
                    return new TSql130Parser(quotedIdentifiers);
                case TSqlPlatforms.SqlAzureV12:
                    return new TSql140Parser(quotedIdentifiers);

            }
            
            return new TSql140Parser(quotedIdentifiers);
        }

        public static SqlScriptGenerator ScriptGeneratorFactory()
        {
            switch (Platform)
            {
                case TSqlPlatforms.Sql90:
                    return new Sql90ScriptGenerator();
                case TSqlPlatforms.Sql100:
                    return new Sql100ScriptGenerator();
                case TSqlPlatforms.SqlAzure:
                    return new Sql120ScriptGenerator();
                case TSqlPlatforms.Sql110:
                    return new Sql110ScriptGenerator();
                case TSqlPlatforms.Sql120:
                    return new Sql120ScriptGenerator();
                case TSqlPlatforms.Sql130:
                    return new Sql130ScriptGenerator();
                case TSqlPlatforms.SqlAzureV12:
                    return new Sql140ScriptGenerator();

            }

            return new Sql140ScriptGenerator();
        }

        public static SqlScriptGenerator ScriptGeneratorFactory(SqlScriptGeneratorOptions options)
        {
            switch (Platform)
            {
                case TSqlPlatforms.Sql90:
                    return new Sql90ScriptGenerator(options);
                case TSqlPlatforms.Sql100:
                    return new Sql100ScriptGenerator(options);
                case TSqlPlatforms.SqlAzure:
                    return new Sql120ScriptGenerator(options);
                case TSqlPlatforms.Sql110:
                    return new Sql110ScriptGenerator(options);
                case TSqlPlatforms.Sql120:
                    return new Sql120ScriptGenerator(options);
                case TSqlPlatforms.Sql130:
                    return new Sql130ScriptGenerator(options);
                case TSqlPlatforms.SqlAzureV12:
                    return new Sql140ScriptGenerator(options);

            }

            return new Sql140ScriptGenerator(options);
        }


        public static void SetVersion(string version)
        {
            if (String.IsNullOrEmpty(version))
                return;

            var platform = TSqlPlatforms.Sql130;

            if (TSqlPlatforms.TryParse(version, true, out platform))
            {
                Platform = platform;
            }
        }

        public static TSqlPlatforms Platform { get; set; }

        private static void SetPlatform(TSqlPlatforms platform)
        {
            switch (platform)
            {
                case TSqlPlatforms.Sql90:
                    break;
                case TSqlPlatforms.Sql100:
                    break;
                case TSqlPlatforms.SqlAzure:
                    break;
                case TSqlPlatforms.Sql110:
                    break;
                case TSqlPlatforms.Sql120:
                    break;
                case TSqlPlatforms.Sql130:
                    break;
                case TSqlPlatforms.SqlAzureV12:
                    break;
                case TSqlPlatforms.OnPremises:
                    break;
                case TSqlPlatforms.Cloud:
                    break;
                case TSqlPlatforms.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
            }
        }
    }
}
