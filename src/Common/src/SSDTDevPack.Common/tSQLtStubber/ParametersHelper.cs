using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.tSQLtStubber
{
    public class ParametersHelper
    {
        public static ExecuteParameter CreateStoredProcedureParameter(string name, string value)
        {
            return new ExecuteParameter
            {
                Variable = new VariableReference
                {
                    Name = name
                },
                ParameterValue = new StringLiteral {Value = value}
            };
        }

        public static ExecuteParameter CreateStoredProcedureParameter(string name, int value)
        {
            return new ExecuteParameter
            {
                Variable = new VariableReference
                {
                    Name = name
                },
                ParameterValue = new IntegerLiteral {Value = value.ToString()}
            };
        }

        public static ExecuteParameter CreateStoredProcedureVariableParameter(string name)
        {
            return new ExecuteParameter
            {
                ParameterValue = new VariableReference
                {
                    Name = name
                }
            };
        }

        public static ExecuteParameter CreateStoredProcedureParameter(string value)
        {
            return new ExecuteParameter
            {
                ParameterValue = new StringLiteral
                {
                    Value = value
                }
            };
        }
    }
}