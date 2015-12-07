using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSDTDevPack.NameConstraints;

namespace NameConstraints.IntegrationTests
{
    [TestFixture]
    public class Names
    {
        [Test]
        public void PrimaryKey()
        {
            var scriptPath = ".\\tableOne.sql";
            if(File.Exists(scriptPath))
                File.Delete(scriptPath);

            File.Copy("..\\..\\..\\ProjectWithConstraints\\TableOne.sql", scriptPath);

            
            //var namer = new ConstraintNamer(scriptPath,
            //    "..\\..\\..\\ProjectWithConstraints\\bin\\Release\\ProjectWithConstraints.dacpac");

            var namer = new ConstraintNamer(File.ReadAllText(scriptPath));

            var changedText = namer.Go();
            
            Assert.LessOrEqual(-1, changedText.IndexOf("[Id] INT NOT NULL PRIMARY KEY"), changedText);
            Assert.Greater( changedText.IndexOf("CONSTRAINT [PK_TableOne] PRIMARY KEY ([Id])", StringComparison.OrdinalIgnoreCase), -1, changedText);
        }
    }
}
