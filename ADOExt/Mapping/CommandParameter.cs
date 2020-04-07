using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class CommandParameter<T>
    {
        public IDBColumnMapping<T> Column;
        public ParameterDirection Direction = ParameterDirection.Input;
    }
}
