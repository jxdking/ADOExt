using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class Parameter : IDbDataParameter
    {
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        public string ParameterName { get; set; }
        public object Value { get; set; }

        public Parameter()
        {

        }

        public Parameter(string name, object val)
        {
            ParameterName = name;
            Value = val;
        }


        public byte Precision { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte Scale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Size { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DbType DbType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsNullable => throw new NotImplementedException();
        public string SourceColumn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DataRowVersion SourceVersion { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
