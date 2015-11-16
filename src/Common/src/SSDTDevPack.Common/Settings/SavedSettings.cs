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

           StreamReader reader = new StreamReader(GetPath("config.xml"));
           var settings = (Settings)serializer.Deserialize(reader);
           reader.Close();


           if (String.IsNullOrEmpty(settings.PrimaryKeyName))
                settings.PrimaryKeyName = primaryKeyNameTemplate;

           return settings;    
        }

        private static string GetPath(string file)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Path.Combine("SSDTDevPack", file));
        }
    }

}
