using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt
{
    public static class IDataReaderExt
    {
        internal static void SetProperty<T>(this IDataRecord reader, T obj, IDBColumnMapping<T> mapping, Action<T, IDataRecord, int> setter, int ordinal)
        {
            try
            {
                setter(obj, reader, ordinal);
            }
            catch (Exception ex)
            {
                string colName = mapping.ColumnName;
                throw new FormatException("Failed to Parse [" + typeof(T).Name + "." + colName + "] from DataReader!", ex);
            }
        }

        /// <summary>
        /// yield return the reader one record by one record. reader.Close() is called at the end of enumeration.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<IDataRecord> AsEnumerable(this IDataReader reader)
        {
            return ToEnumerable(reader).AsSingleUse();
        }

        private static IEnumerable<IDataRecord> ToEnumerable(IDataReader reader)
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

        public static IEnumerable<T> AsEnumerable<T>(this IDataReader reader, IDBObjectMappingFactory dbObjectMappingFactory) where T : new()
        {
            var objectMapping = dbObjectMappingFactory.Get<T>();
            var mapping = objectMapping.ColumnMappingList;
            int[] ordinalAry = new int[mapping.Count];
            var setterAry = new Action<T, IDataRecord, int>[mapping.Count];

            int i;
            string cName = null;
            try
            {
                int ord;
                for (i = 0; i < mapping.Count; i++)
                {
                    cName = mapping[i].ColumnName;
                    ord = reader.GetOrdinal(cName);
                    ordinalAry[i] = ord;
                    setterAry[i] = mapping[i].GetPropSetterForRecord(reader.GetFieldType(ord));
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException("Unable to find specified column[" + cName + "] in DataReader", ex);
            }

            T obj;

            return reader.AsEnumerable().Transform(rdr =>
            {
                obj = new T();
                for (i = 0; i < mapping.Count; i++)
                {
                    rdr.SetProperty(obj, mapping[i], setterAry[i], ordinalAry[i]);
                }
                return obj;
            });
        }
    }
}
