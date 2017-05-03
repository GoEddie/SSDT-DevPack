using System.Collections.Generic;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SSDTDevPack.tSQLtStubber
{
    public class ProcedureVisitor : TSqlFragmentVisitor
    {
        public readonly List<CreateProcedureStatement> Procedures = new List<CreateProcedureStatement>();
        public readonly List<CreateFunctionStatement> Functions = new List<CreateFunctionStatement>();

        public override void Visit(CreateProcedureStatement node)
        {
            Procedures.Add(node);
        }

        public override void Visit(CreateFunctionStatement node)
        {
            Functions.Add(node);
        }
    }
}