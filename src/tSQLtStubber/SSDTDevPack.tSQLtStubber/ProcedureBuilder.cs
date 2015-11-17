using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;

namespace SSDTDevPack.tSQLtStubber
{
    enum CodeType
    {
        Procedure,
        Function
    }
    //TODO I added function in a hurry, this should be split into two classes which inherit common code
    public class ProcedureBuilder : ScriptBuilder
    {
        private readonly ExecuteStatement _execProc = new ExecuteStatement();
        private readonly SelectStatement _functionSelect = new SelectStatement();
        private readonly List<Parameter> _parameters = new List<Parameter>();
        private readonly List<ObjectIdentifier> _tables = new List<ObjectIdentifier>();
        private readonly CreateProcedureStatement _testProcedure = new CreateProcedureStatement();
        private readonly CodeType CodeType;

        public ProcedureBuilder(string testSchema, string testName, TSqlProcedure procedureUnderTest)
        {
            CodeType = CodeType.Procedure;
            _testProcedure.StatementList = new StatementList();
            foreach (var t in procedureUnderTest.BodyDependencies)
            {
                if (t.ObjectType == ModelSchema.Table)
                {
                    //table to fake
                    var table = new TSqlTable(t.Element);
                    AddTable(table.Name);
                }
            }

            foreach (var p in procedureUnderTest.Parameters)
            {
                AddParameter(p.Name.GetName().UnQuote(), GetParameterType(p.DataType));
            }

            CreateTestProcedureDefinition(testSchema, testName);
            CreateExecForProcUnderTest(procedureUnderTest.Name);
        }

        public ProcedureBuilder(string testSchema, string testName, TSqlTableValuedFunction procedureUnderTest)
        {
            CodeType = CodeType.Function;
            _testProcedure.StatementList = new StatementList();
            foreach (var t in procedureUnderTest.BodyDependencies)
            {
                if (t.ObjectType == ModelSchema.Table)
                {
                    //table to fake
                    var table = new TSqlTable(t.Element);
                    AddTable(table.Name);
                }
            }

            foreach (var p in procedureUnderTest.Parameters)
            {
                AddParameter(p.Name.GetName().UnQuote(), GetParameterType(p.DataType));
            }

            CreateTestProcedureDefinition(testSchema, testName);
            CreateSelectForFunctionUnderTest(procedureUnderTest.Name);
        }

        
        private SqlDataType GetParameterType(IEnumerable<ISqlDataType> dataType)
        {
            foreach (var type in dataType)
            {
                SqlDataType returnType;
                if (Enum.TryParse(type.Name.GetName().UnQuote(), true, out returnType))
                    return returnType;
            }
            
            return SqlDataType.VarChar;
        }

        private void CreateTestProcedureDefinition(string testSchema, string testName)
        {
            var testProcedureReference = _testProcedure.ProcedureReference = new ProcedureReference();
            testProcedureReference.Name = new SchemaObjectName();
            testProcedureReference.Name.Identifiers.Add(testSchema.ToIdentifier());
            testProcedureReference.Name.Identifiers.Add(testName.ToIdentifier());
            testProcedureReference.Name.BaseIdentifier.QuoteType = QuoteType.SquareBracket;
            testProcedureReference.Name.SchemaIdentifier.QuoteType = QuoteType.SquareBracket;
        }

        private void CreateExecForProcUnderTest(ObjectIdentifier procUnderTest)
        {
            var calleeProcedure = new ProcedureReference();
            calleeProcedure.Name = new SchemaObjectName();

            _execProc.ExecuteSpecification = new ExecuteSpecification();
            var entity = new ExecutableProcedureReference();
            entity.ProcedureReference = new ProcedureReferenceName();
            entity.ProcedureReference.ProcedureReference = calleeProcedure;
            entity.ProcedureReference.ProcedureReference.Name = procUnderTest.ToSchemaObjectName();
            _execProc.ExecuteSpecification.ExecutableEntity = entity;
        }

        private void CreateSelectForFunctionUnderTest(ObjectIdentifier name)
        {
            var select = new QuerySpecification();
            select.SelectElements.Add(new SelectStarExpression());

            var from = new FromClause();
            var reference = new SchemaObjectFunctionTableReference();
            foreach (var p in _parameters)
            {
                reference.Parameters.Add(new VariableReference(){Name = p.Name});
            }
            
            reference.SchemaObject = name.ToSchemaObjectName();
            from.TableReferences.Add(reference);

            select.FromClause = from;
            _functionSelect.QueryExpression = select;
        }


        public void AddParameter(string name, SqlDataType type)
        {
            _parameters.Add(new Parameter(name, type));
        }

        public void AddTable(ObjectIdentifier name)
        {
            _tables.Add(name);
        }

        //We want it to look like, create proc as .. fake tables .. declares for proc .. proc .. assert
        private void Assemble()
        {
            _testProcedure.StatementList.Statements.Clear();


            foreach (var table in _tables)
            {
                CreateFakeTableDefinition(table);
            }
            
            foreach (var parameter in _parameters)
            {
                CreateDeclareVariableDefinitionForParmeter(parameter.Name, parameter.Type);

                if (CodeType == CodeType.Procedure)
                    CreateParameterForCalleeStoredProc(parameter);
            }
            
            if(CodeType == CodeType.Procedure)
                _testProcedure.StatementList.Statements.Add(_execProc);
            else
                _testProcedure.StatementList.Statements.Add(_functionSelect);


            CreateAssertDefinition();
        }

        private void CreateParameterForCalleeStoredProc(Parameter parameter)
        {
            _execProc.ExecuteSpecification.ExecutableEntity.Parameters.Add(
                ParametersHelper.CreateStoredProcedureVariableParameter(parameter.Name));
        }

        private void CreateFakeTableDefinition(ObjectIdentifier table)
        {
            var fakeTable = new ExecuteStatement();
            fakeTable.ExecuteSpecification = new ExecuteSpecification();

            var procedureReference = new ProcedureReference();
            procedureReference.Name = new SchemaObjectName();
            procedureReference.Name.Identifiers.Add("tSQLt".ToIdentifier());
            procedureReference.Name.Identifiers.Add("FakeTable".ToIdentifier());

            var entity = new ExecutableProcedureReference();
            entity.ProcedureReference = new ProcedureReferenceName();
            entity.ProcedureReference.ProcedureReference = procedureReference;

            entity.Parameters.Add(
                ParametersHelper.CreateStoredProcedureParameter(string.Format("{0}", table.GetSchema())));
            entity.Parameters.Add(
                ParametersHelper.CreateStoredProcedureParameter(string.Format("{0}", table.GetName())));

            fakeTable.ExecuteSpecification.ExecutableEntity = entity;

            _testProcedure.StatementList.Statements.Add(fakeTable);
        }

        private void CreateAssertDefinition()
        {
            var fakeTable = new ExecuteStatement();
            fakeTable.ExecuteSpecification = new ExecuteSpecification();

            var procedureReference = new ProcedureReference();
            procedureReference.Name = new SchemaObjectName();
            procedureReference.Name.Identifiers.Add("tSQLt".ToIdentifier());
            procedureReference.Name.Identifiers.Add("AssertEquals".ToIdentifier());

            var entity = new ExecutableProcedureReference();
            entity.ProcedureReference = new ProcedureReferenceName();
            entity.ProcedureReference.ProcedureReference = procedureReference;

            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("TRUE"));
            entity.Parameters.Add(ParametersHelper.CreateStoredProcedureParameter("FALSE"));

            var messageParameter = new ExecuteParameter();
            var messageValue = new StringLiteral { IsNational = true, Value = "Error Not Implemented" };
            messageParameter.ParameterValue = messageValue;
            entity.Parameters.Add(messageParameter);

            fakeTable.ExecuteSpecification.ExecutableEntity = entity;

            _testProcedure.StatementList.Statements.Add(fakeTable);
        }

        private void CreateDeclareVariableDefinitionForParmeter(string name, SqlDataType type)
        {
            var declare = new DeclareVariableStatement();
            var declareElement = new DeclareVariableElement();

            var dataType = GetDataType(type);
            declareElement.Value = GetDefaultValue(dataType);
            declareElement.DataType = dataType;
            declareElement.VariableName = name.ToIdentifier();
            declare.Declarations.Add(declareElement);

            _testProcedure.StatementList.Statements.Add(declare);
        }

        public string GetScript()
        {
            Assemble();

            var script = new StringBuilder();

            var tokens = GetTokens(_testProcedure);
            var previousNewLine = false;

            foreach (var token in tokens)
            {
                if (previousNewLine && (token.TokenType != TSqlTokenType.As && token.TokenType != TSqlTokenType.Create))
                {
                    script.Append("    ");
                }

                script.Append(token.Text);

                previousNewLine = token.Text == "\r\n";
            }

            return script.ToString();
        }

        private SqlDataTypeReference GetDataType(SqlDataType type)
        {
            var option = SqlDataTypeOption.BigInt;

            switch (type)
            {
                case SqlDataType.Unknown:
                    option = SqlDataTypeOption.VarChar;
                    break;
                case SqlDataType.BigInt:
                    option = SqlDataTypeOption.BigInt;
                    break;
                case SqlDataType.Int:
                    option = SqlDataTypeOption.Int;
                    break;
                case SqlDataType.SmallInt:
                    option = SqlDataTypeOption.SmallInt;
                    break;
                case SqlDataType.TinyInt:
                    option = SqlDataTypeOption.TinyInt;
                    break;
                case SqlDataType.Bit:
                    option = SqlDataTypeOption.Bit;
                    break;
                case SqlDataType.Decimal:
                    option = SqlDataTypeOption.Decimal;
                    break;
                case SqlDataType.Numeric:
                    option = SqlDataTypeOption.Numeric;
                    break;
                case SqlDataType.Money:
                    option = SqlDataTypeOption.Money;
                    break;
                case SqlDataType.SmallMoney:
                    option = SqlDataTypeOption.SmallMoney;
                    break;
                case SqlDataType.Float:
                    option = SqlDataTypeOption.Float;
                    break;
                case SqlDataType.Real:
                    option = SqlDataTypeOption.Real;
                    break;
                case SqlDataType.DateTime:
                    option = SqlDataTypeOption.DateTime;
                    break;
                case SqlDataType.SmallDateTime:
                    option = SqlDataTypeOption.SmallDateTime;
                    break;
                case SqlDataType.Char:
                    option = SqlDataTypeOption.Char;
                    break;
                case SqlDataType.VarChar:
                    option = SqlDataTypeOption.VarChar;
                    break;
                case SqlDataType.Text:
                    option = SqlDataTypeOption.Text;
                    break;
                case SqlDataType.NChar:
                    option = SqlDataTypeOption.NChar;
                    break;
                case SqlDataType.NVarChar:
                    option = SqlDataTypeOption.NVarChar;
                    break;
                case SqlDataType.NText:
                    option = SqlDataTypeOption.NText;
                    break;
                case SqlDataType.Binary:
                    option = SqlDataTypeOption.Binary;
                    break;
                case SqlDataType.VarBinary:
                    option = SqlDataTypeOption.VarBinary;
                    break;
                case SqlDataType.Image:
                    option = SqlDataTypeOption.Image;
                    break;
                case SqlDataType.Cursor:
                    option = SqlDataTypeOption.Cursor;
                    break;
                case SqlDataType.Variant:
                    option = SqlDataTypeOption.Sql_Variant;
                    break;
                case SqlDataType.Table:
                    option = SqlDataTypeOption.Table;
                    break;
                case SqlDataType.Timestamp:
                    option = SqlDataTypeOption.Timestamp;
                    break;
                case SqlDataType.UniqueIdentifier:
                    option = SqlDataTypeOption.UniqueIdentifier;
                    break;
                case SqlDataType.Xml:
                    //??
                    break;
                case SqlDataType.Date:
                    option = SqlDataTypeOption.Date;
                    break;
                case SqlDataType.Time:
                    option = SqlDataTypeOption.Time;
                    break;
                case SqlDataType.DateTime2:
                    option = SqlDataTypeOption.DateTime2;

                    break;
                case SqlDataType.DateTimeOffset:
                    option = SqlDataTypeOption.DateTimeOffset;
                    break;
                case SqlDataType.Rowversion:
                    option = SqlDataTypeOption.Rowversion;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            return new SqlDataTypeReference
            {
                SqlDataTypeOption = option
            };
        }

        private ScalarExpression GetDefaultValue(SqlDataTypeReference type)
        {
            switch (type.SqlDataTypeOption)
            {
                case SqlDataTypeOption.SmallInt:
                case SqlDataTypeOption.TinyInt:
                case SqlDataTypeOption.Bit:
                case SqlDataTypeOption.Decimal:
                case SqlDataTypeOption.Numeric:
                case SqlDataTypeOption.Money:
                case SqlDataTypeOption.SmallMoney:
                case SqlDataTypeOption.Float:
                case SqlDataTypeOption.Real:
                case SqlDataTypeOption.BigInt:
                case SqlDataTypeOption.Int:
                    return new IntegerLiteral { Value = "0" };

                case SqlDataTypeOption.Char:
                case SqlDataTypeOption.VarChar:
                case SqlDataTypeOption.Text:
                case SqlDataTypeOption.NChar:
                case SqlDataTypeOption.NVarChar:
                case SqlDataTypeOption.NText:

                    return new StringLiteral { Value = "" };

                case SqlDataTypeOption.Binary:
                case SqlDataTypeOption.VarBinary:
                case SqlDataTypeOption.Image:
                    return new BinaryLiteral { Value = "0" };


                case SqlDataTypeOption.UniqueIdentifier:
                    return new StringLiteral { Value = Guid.NewGuid().ToString() };


                case SqlDataTypeOption.DateTime:
                case SqlDataTypeOption.SmallDateTime:
                case SqlDataTypeOption.Date:
                case SqlDataTypeOption.DateTime2:
                    return new StringLiteral { Value = "1980-04-01" }; //yes i did do that

                case SqlDataTypeOption.Time:
                    return new StringLiteral { Value = "11:59:59" }; //yes i did do that
            }

            return new StringLiteral { Value = "0" };
        }
    }
}
