using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SSDTDevPack.Common.Dac;
using SSDTDevPack.Common.ScriptDom;
using SSDTDevPack.Common.UserMessages;
using SSDTDevPack.Merge.UI;

namespace SSDTDevPack.QuickDeploy
{
    public class QuickDeployer
    {
        private static string ConnectionString;

        public static void DeployFile(string newCode)
        {
            if (String.IsNullOrEmpty(ConnectionString))
            {
                var connectDialog = new ConnectDialog();
                connectDialog.ShowDialog();
                ConnectionString = connectDialog.ConnectionString;

                if (String.IsNullOrEmpty(ConnectionString))
                    return;
            }

            var procedures = ScriptDom.GetProcedures(newCode);
            var deployScripts = new List<string>();

            foreach (var procedure in procedures)
            {
                OutputPane.WriteMessage("Deploying {0}", procedure.ProcedureReference.Name.ToQuotedString());
                Deploy(BuildIfNotExistsStatements(procedure));
                Deploy(ChangeCreateToAlter(procedure, newCode));
                OutputPane.WriteMessage("Deploying {0}...Done", procedure.ProcedureReference.Name.ToQuotedString());
            }

            var functions = ScriptDom.GetFunctions(newCode);
            foreach (var function in functions)
            {
                OutputPane.WriteMessage("Deploying {0}", function.Name.ToQuotedString());
                if (function.ReturnType is SelectFunctionReturnType)
                {
                    Deploy(BuildIfNotExistsStatementsInlineFunction(function));
                }
                else
                {
                    Deploy(BuildIfNotExistsStatements(function));
                }

                Deploy(ChangeCreateToAlter(function, newCode));
                OutputPane.WriteMessage("Deploying {0}...Done", function.Name.ToQuotedString());
                
            }

            foreach (var statement in deployScripts)
            {
                Deploy(statement);
            }
            //Deploy();
        }

        private static string BuildIfNotExistsStatementsInlineFunction(CreateFunctionStatement function)
        {

            var generateIfExists =
                string.Format("if object_id('{0}') is null\r\nbegin\r\n execute sp_executeSql N' create function {0}() \r\n RETURNS TABLE as RETURN SELECT 1 as a' \r\nEND;",
                    function.Name.ToQuotedString());

            return generateIfExists;
        }

        private static void Deploy(string statement)
        {

            try
            {
                using (var con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = statement;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                OutputPane.WriteMessage("Error Deploying File: {0}\r\n", ex.Message);
            }

        }

        private static string BuildIfNotExistsStatements(CreateProcedureStatement procedure)
        {

            var generateIfExists =
                string.Format("if object_id('{0}') is null\r\nbegin\r\n execute sp_executeSql N' create procedure {0} as select 1;';\r\nend",
                    procedure.ProcedureReference.Name.ToQuotedString());

            return generateIfExists;
        }

        private static string BuildIfNotExistsStatements(CreateFunctionStatement function)
        {

            var generateIfExists =
                string.Format("if object_id('{0}') is null\r\nbegin\r\n execute sp_executeSql N' create function {0}() \r\n RETURNS @t TABLE (stringValue VARCHAR(128))\r\n as \r\nbegin\r\ninsert into @t values(''ee'')\r\n return\r\nend';\r\nend",
                    function.Name.ToQuotedString());

            return generateIfExists;
        }

        private static string ChangeCreateToAlter(CreateProcedureStatement procedure, string wholeScript)
        {
            //get part of script we are interested in...
            var subScript = wholeScript.Substring(procedure.StartOffset, procedure.FragmentLength);

            IList<ParseError> errors;
            var fragment = new TSql130Parser(false).Parse(new StringReader(subScript), out errors);

            bool haveCreate = false;
            var output = new StringBuilder();

            foreach (var token in fragment.ScriptTokenStream)
            {
                if (!haveCreate && token.TokenType == TSqlTokenType.Create)
                {
                    var alterToken = new TSqlParserToken(TSqlTokenType.Alter, "alter");
                    output.Append(alterToken.Text);
                    haveCreate = true;
                    continue;
                }

                output.Append(token.Text);
            }

            return output.ToString();
        }


        private static string ChangeCreateToAlter(CreateFunctionStatement function, string wholeScript)
        {
            //get part of script we are interested in...
            var subScript = wholeScript.Substring(function.StartOffset, function.FragmentLength);

            IList<ParseError> errors;
            var fragment = new TSql130Parser(false).Parse(new StringReader(subScript), out errors);

            bool haveCreate = false;
            var output = new StringBuilder();

            foreach (var token in fragment.ScriptTokenStream)
            {
                if (!haveCreate && token.TokenType == TSqlTokenType.Create)
                {
                    var alterToken = new TSqlParserToken(TSqlTokenType.Alter, "alter");
                    output.Append(alterToken.Text);
                    haveCreate = true;
                    continue;
                }

                output.Append(token.Text);
            }

            return output.ToString();
        }
    }




}
