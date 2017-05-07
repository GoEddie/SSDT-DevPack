using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ProjectVersion;
using SSDTDevPack.Common.Settings;

namespace SSDTDevPack.NameConstraints
{
    public  class ConstraintNamer
    {
        private readonly string _script;
        
        public ConstraintNamer(string script)
        {
            _script = script;
        }

        public string Go()
        {
            var statements = GetCreateTableStatements();

            var statementsToChange = new List<CreateTableStatement>();

            foreach (var statement in statements)
            {
                bool hasChanged = NamePrimaryKey(statement);


                if (hasChanged)
                {
                   statementsToChange.Add(statement);
                }
                
            }

            var modifiedScript = _script;

            foreach (var statement in statementsToChange)
            {
               modifiedScript = ModifyScript(statement,_script, modifiedScript);
            }

            return modifiedScript;

        }

        private string ModifyScript(CreateTableStatement statement, string originalScript, string modifiedScript)
        {
            var oldScriptBlock = originalScript.Substring(statement.StartOffset, statement.FragmentLength);

            var comments = GetComments(oldScriptBlock);

            var generator = VersionDetector.ScriptGeneratorFactory(SavedSettings.Get().GeneratorOptions);
            
            string newScriptBlock;
            generator.GenerateScript(statement, out newScriptBlock);

            if (string.IsNullOrEmpty(comments))
                modifiedScript = modifiedScript.Replace(oldScriptBlock, newScriptBlock);
            else
                modifiedScript = modifiedScript.Replace(oldScriptBlock, newScriptBlock + "\r\n--These comments were saved after refactoring this table...\r\n" + comments);

            return modifiedScript;
        }

        private string GetComments(string oldScriptBlock)
        {

            var comments = new StringBuilder();
            var parser = VersionDetector.ParserFactory(false);
            IList<ParseError> errors;
            var tokens = parser.GetTokenStream(new StringReader(oldScriptBlock), out errors);
            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case TSqlTokenType.MultilineComment:
                    case TSqlTokenType.SingleLineComment:

                        comments.AppendLine(token.Text);
                        break;
                }
            }

            return comments.ToString();
        }

        private static bool NamePrimaryKey(CreateTableStatement statement)
        {
            var columnWithPrimaryKey = statement.Definition.ColumnDefinitions.FirstOrDefault( c => c.Constraints.Any(p => p is UniqueConstraintDefinition));

            if (columnWithPrimaryKey == null)
            {
                return false;
            }

            var originalConstraint = columnWithPrimaryKey.Constraints.FirstOrDefault( c => c as UniqueConstraintDefinition != null && ((UniqueConstraintDefinition) c).IsPrimaryKey) as UniqueConstraintDefinition;

            if (originalConstraint == null)
            {
                return false;
            }

            var newConstraint = new UniqueConstraintDefinition();
            newConstraint.Clustered = originalConstraint.Clustered;
            newConstraint.IsPrimaryKey = true;

            foreach (var o in originalConstraint.IndexOptions)
                newConstraint.IndexOptions.Add(o);

            newConstraint.IndexType = originalConstraint.IndexType;

            var referencedColumn = new ColumnWithSortOrder();
            referencedColumn.Column = new ColumnReferenceExpression();
            referencedColumn.Column.MultiPartIdentifier = new SchemaObjectName();
            referencedColumn.Column.MultiPartIdentifier.Identifiers.Add(columnWithPrimaryKey.ColumnIdentifier);
            newConstraint.Columns.Add(referencedColumn);
            
            newConstraint.ConstraintIdentifier = new Identifier();
            newConstraint.ConstraintIdentifier.Value =
                ReplaceTemplateValues(SavedSettings.Get().PrimaryKeyName, statement.SchemaObjectName.BaseIdentifier.Value, referencedColumn.Column.MultiPartIdentifier.Identifiers.Last().Value);
            
            newConstraint.ConstraintIdentifier.QuoteType = QuoteType.SquareBracket;
            
            statement.Definition.TableConstraints.Add(newConstraint);
            columnWithPrimaryKey.Constraints.Remove(originalConstraint);

            return true;
        }

        private static string ReplaceTemplateValues(string template, string tableName, string columnName)
        {
            return template.Replace("%TABLENAME%", tableName).Replace("%COLUMNNAME%", columnName);
        }

        private List<CreateTableStatement> GetCreateTableStatements()
        {
            using (var script = new StringReader(_script))
            {
                var parser = VersionDetector.ParserFactory(false);

                IList<ParseError> errors;

                var fragment = parser.Parse(script, out errors);

                if (fragment != null)
                {
                    var visitor = new CreateTableVisitor();
                    fragment.Accept(visitor);
                    return visitor.Creates;
                }
            }

            return null;
        }
    }

    public class CreateTableVisitor : TSqlConcreteFragmentVisitor
    {
        public  List<CreateTableStatement> Creates = new List<CreateTableStatement>(); 

        public override void Visit(CreateTableStatement node)
        {
            Creates.Add(node);
        }

        
        //ALSO NEED ALTERTABLECREATECONSTRAINT BLAH as thjose can be unnamed
    }

}
