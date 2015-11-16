using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.Common.Settings
{
    public class Settings
    {
        public Settings()
        {
            GeneratorOptions = new SqlScriptGeneratorOptions();
        }

        public SqlScriptGeneratorOptions GeneratorOptions { get; set; }
        public string PrimaryKeyName { get; set; }

    }



    public class SavedSettings
    {
        private const string primaryKeyNameTemplate = "PK_%TABLENAME%";
        
        public static Settings Get()
        {
           XmlSerializer serializer = new XmlSerializer(typeof(Settings));


           var settings = GetSettings(serializer);


            if (String.IsNullOrEmpty(settings.PrimaryKeyName))
                settings.PrimaryKeyName = primaryKeyNameTemplate;

           return settings;    
        }

        private static Settings GetSettings(XmlSerializer serializer)
        {
            if (File.Exists(GetPath("config.xml")))
            {
                StreamReader reader = new StreamReader(GetPath("config.xml"));
                var settings = (Settings) serializer.Deserialize(reader);
                reader.Close();
                return settings;
            }

            return new Settings();
        }

        private static string GetPath(string file)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Path.Combine("SSDTDevPack", file));
        }
    }

}
