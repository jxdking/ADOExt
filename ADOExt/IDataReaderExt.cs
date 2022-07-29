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

        internal static IEnumerable<T> AsEnumerable<T>(this IDataReader reader, Action<T, IDataRecord>[] setters) where T : new()
        {
            T obj;
            int i = 0;
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
                throw new Exception($"Error when parsing {i}(th) Property on Type[{typeof(T).FullName}] from IDataReader!", ex);
            }
        }
    }
}
