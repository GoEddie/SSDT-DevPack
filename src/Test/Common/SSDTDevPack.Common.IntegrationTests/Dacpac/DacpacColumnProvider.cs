using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using SSDTDevPack.Common.Dac;

namespace SSDTDevPack.Common.IntegrationTests.Columns
{

    [TestFixture]
    public class DacpacTableProvider
    {
        private TSqlTypedModel _model;

        [TestFixtureSetUp]
        public void Init()
        {
            _model =
                Model.Get(Path.Combine(Directories.GetSampleSolution(),
                    @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
        }

        [Test]
        public void gets_table_name()
        {
            var table = _model.GetObject<TSqlTable>(new ObjectIdentifier("dbo", "TheTable"), DacQueryScopes.UserDefined);
            Assert.IsNotNull(table);
            var descriptor = new TableDescriptor(table);
            Assert.AreEqual("dbo", descriptor.Name.GetSchema());
            Assert.AreEqual("TheTable", descriptor.Name.GetName());
            
        }
    }

    [TestFixture]
    public class DacpacColumnProvider
    {

        private TSqlTypedModel _model;

        [TestFixtureSetUp]
        public void Init()
        {
            _model =
                Model.Get(Path.Combine(Directories.GetSampleSolution(),
                    @"NestedProjects\Nested\bin\Debug\Nested.dacpac"));
        }

        [Test]
        public void uses_correct_literal_type()
        {
            var table = _model.GetObject<TSqlTable>(new ObjectIdentifier("dbo", "TheTable"), DacQueryScopes.UserDefined);
            Assert.IsNotNull(table);
            var descriptor = new TableDescriptor(table);
            Assert.AreEqual(LiteralType.Integer, descriptor.Columns.First().DataType);

        }


        [Test]
        public void uses_sets_key_on_key_columns()
        {
            var table = _model.GetObject<TSqlTable>(new ObjectIdentifier("dbo", "TheTable"), DacQueryScopes.UserDefined);
            Assert.IsNotNull(table);
            var descriptor = new TableDescriptor(table);
            Assert.IsTrue(descriptor.Columns.First().IsKey);
            Assert.IsFalse(descriptor.Columns.Last().IsKey);
        }

        [Test]
        public void uses_sets_identity_on_identity_columns()
        {
            var table = _model.GetObject<TSqlTable>(new ObjectIdentifier("dbo", "TheTable"), DacQueryScopes.UserDefined);
            Assert.IsNotNull(table);
            var descriptor = new TableDescriptor(table);
            Assert.IsTrue(descriptor.Columns.First().IsIdentity);
            Assert.IsFalse(descriptor.Columns.Last().IsIdentity);
        }



    }
}
