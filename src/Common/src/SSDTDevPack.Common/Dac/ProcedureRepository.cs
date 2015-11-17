using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;

namespace SSDTDevPack.Common.Dac
{
    public class ProcedureRepository : IDisposable, IEnumerable<TSqlProcedure>
    {
        public ProcedureRepository(string path)
        {
            _model = Model.Get(path);

            _procedures = _model.GetObjects<TSqlProcedure>(DacQueryScopes.UserDefined);
            
        }

        public void Dispose()
        {
            Model.Close(_model);
        }

        private readonly IEnumerable<TSqlProcedure> _procedures;
        private readonly TSqlTypedModel _model;

        public IEnumerator<TSqlProcedure> GetEnumerator()
        {
            return _procedures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}