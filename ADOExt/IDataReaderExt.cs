using System;
using System.Collections.Generic;
using System.Data;
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

        public static List<T> ToList<T>(this IDataReader reader) where T : new()
        {
            List<T> res = new List<T>();
            var mapping = DBObjectMapping<T>.Get().ColumnMappingList;
            int[] ordinalAry = new int[mapping.Count];
            var setterAry = new Action<T, IDataRecord, int>[mapping.Count];

            T obj;

            if (reader.Read())
            {
                obj = new T();
                string cName = null;
                try
                {
                    for (int i = 0; i < mapping.Count; i++)
                    {
                        cName = mapping[i].ColumnName;
                        ordinalAry[i] = reader.GetOrdinal(cName);
                        setterAry[i] = mapping[i].GetPropSetterForRecord(reader.GetFieldType(ordinalAry[i]));

                        reader.SetProperty(obj, mapping[i], setterAry[i], ordinalAry[i]);
                    }

                    res.Add(obj);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new IndexOutOfRangeException("Unable to find specified column[" + cName + "] in DataReader", ex);
                }
            }

            while (reader.Read())
            {
                obj = new T();
                for (int i = 0; i < mapping.Count; i++)
                {
                    reader.SetProperty(obj, mapping[i], setterAry[i], ordinalAry[i]);
                }
                res.Add(obj);
            }

            return res;
        }
    }
}
