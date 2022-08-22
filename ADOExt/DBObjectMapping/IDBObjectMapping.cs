using System;
using System.Collections.Generic;
using System.Data;

namespace MagicEastern.ADOExt
{
    public interface IDBObjectMapping<T>
    {
        List<IDBColumnMapping<T>> ColumnMappingList { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>These returned setters only set the values that are not DBNull. Thus, only use these setters on newly initialized object,
        /// so that untouched properties would have its default value.</returns>
        //Action<T, IDataRecord>[] GetDataReaderSetters(IDataRecord reader);

        /// <summary>
        /// The return value is cached with sqltxt as the key.
        /// </summary>
        /// <param name="sqltxt"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        Action<T, IDataRecord>[] GetDataReaderSetters(Sql sql, IDataRecord reader);
    }
}