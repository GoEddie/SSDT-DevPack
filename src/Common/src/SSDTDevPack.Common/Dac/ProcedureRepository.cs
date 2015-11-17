using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;

namespace SSDTDevPack.Common.Dac
{
    public class ProcedureRepository : IDisposable, IEnumerable<TSqlProcedure>
    {
        private readonly string _path;

        public ProcedureRepository(string path)
        {
            _path = path;
            _model = Model.Get(path);

            _procedures = _model.GetObjects<TSqlProcedure>(DacQueryScopes.UserDefined);
            
        }

        public void Dispose()
        {
            Model.Close(_path);
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

    public class FunctionRepository : IDisposable, IEnumerable<TSqlTableValuedFunction>
    {
        private readonly string _path;

        public FunctionRepository(string path)
        {
            _path = path;
            _model = Model.Get(path);

            _procedures = _model.GetObjects<TSqlTableValuedFunction>(DacQueryScopes.UserDefined);

        }

        public void Dispose()
        {
            Model.Close(_path);
        }

        private readonly IEnumerable<TSqlTableValuedFunction> _procedures;
        private readonly TSqlTypedModel _model;

        public IEnumerator<TSqlTableValuedFunction> GetEnumerator()
        {
            return _procedures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}