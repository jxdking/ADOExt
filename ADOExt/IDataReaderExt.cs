using System;
using System.Collections.Generic;
using System.Data;

namespace MagicEastern.ADOExt
{
    public static class IDataReaderExt
    {
        /// <summary>
        /// yield return the reader one record by one record. reader.Close() is called at the end of enumeration.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<IDataRecord> AsEnumerable(this IDataReader reader)
        {
            return _AsEnumerable(reader).AsSingleUse();
        }

        private static IEnumerable<IDataRecord> _AsEnumerable(IDataReader reader)
        {
            try
            {
                while (reader.Read())
                {
                    yield return reader;
                }
            }
            finally
            {
                reader.Close();
            }
        }

        internal static IEnumerable<T> AsEnumerable<T>(this IDataReader reader, DBObjectMapping<T> objectMapping) where T : new()
        {
            var mapping = objectMapping.ColumnMappingList;
            Action<T, IDataRecord>[] setters = new Action<T, IDataRecord>[mapping.Count];

            int i = 0;
            try
            {
                for (i = 0; i < mapping.Count; i++)
                {
                    var colMapping = mapping[i];
                    var cName = colMapping.ColumnName;
                    var ordinal = reader.GetOrdinal(cName);
                    var setter = colMapping.GetPropSetterForRecord(reader.GetFieldType(ordinal));
                    setters[i] = (o, r) => { setter(o, r, ordinal); };
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException("Unable to find specified column[" + mapping[i].ColumnName + "] in DataReader", ex);
            }

            T obj;
            try
            {
                return reader.AsEnumerable().Transform(rdr =>
                {
                    obj = new T();
                    for (i = 0; i < setters.Length; i++)
                    {
                        var setter = setters[i];
                        setter(obj, rdr);
                    }
                    return obj;
                });
            }
            catch (Exception ex)
            {
                var col = mapping[i];
                throw new Exception($"Error when parsing Property[{typeof(T).FullName}.{col.ObjectProperty.Name}] from IDataReader!", ex);
            }
        }
    }
}
