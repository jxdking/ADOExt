using System;
using System.Collections.Generic;
using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IDBObjectMapping<T>
    {
        IReadOnlyList<IDBColumnMapping<T>> ColumnMappingList { get; }

        Action<T, IDataRecord>[] GetDataReaderSetters(IDataRecord reader);

        /// <summary>
        /// The return value is cached with sqltxt.
        /// </summary>
        /// <param name="sqltxt"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        Action<T, IDataRecord>[] GetDataReaderSetters(string sqltxt, IDataRecord reader);
    }
}