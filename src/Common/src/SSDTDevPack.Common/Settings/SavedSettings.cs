using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Logging;

namespace SSDTDevPack.Common.Settings
{
    public class Settings
    {
        public Settings()
        {
            GeneratorOptions = new SqlScriptGeneratorOptions();
            Costs = new CostThreshold {High = 1m, Medium = 0.2m};
        }

        public SqlScriptGeneratorOptions GeneratorOptions { get; set; }
        public string PrimaryKeyName { get; set; }
        public CostThreshold Costs { get; set; }
    }

    public class CostThreshold
    {
        public decimal High;
        public decimal Medium;
    }

    public class SavedSettings
    {
        private const string primaryKeyNameTemplate = "PK_%TABLENAME%";

        public static Settings Get()
        {
            var serializer = new XmlSerializer(typeof (Settings));

            var settings = GetSettings(serializer);


            if (string.IsNullOrEmpty(settings.PrimaryKeyName))
                settings.PrimaryKeyName = primaryKeyNameTemplate;

            //var b = new StringBuilder();
            //settings.Costs.High = 0.02;
            //serializer.Serialize(new StringWriter(b), settings);

            //Console.Write(b.ToString());
            return settings;
        }

        private static Settings GetSettings(XmlSerializer serializer)
        {
            try
            {
                if (File.Exists(GetPath("config.xml")))
                {
                    var reader = new StreamReader(GetPath("config.xml"));
                    var settings = (Settings) serializer.Deserialize(reader);
                    reader.Close();
                    return settings;
                }
            }
            catch (Exception e)
            {
                Log.WriteInfo("Error unable to parse settings file ({1}), error: {0}", GetPath("config.xml"), e.Message);
            }

            return new Settings();
        }

        private static string GetPath(string file)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Path.Combine("SSDTDevPack", file));
        }
    }
}