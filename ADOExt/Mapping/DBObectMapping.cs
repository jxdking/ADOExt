using System.Collections.Generic;

//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBObjectMapping<T>
    {
        public List<IDBColumnMapping<T>> ColumnMappingList { get; set; }

        private static DBObjectMapping<T> _Mapping;

        public static DBObjectMapping<T> Get()
        {
            if (_Mapping == null) { _Mapping = new DBObjectMapping<T>(); }
            return _Mapping;
        }

        private DBObjectMapping()
        {
            ColumnMappingList = new List<IDBColumnMapping<T>>();

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(i => i.CanWrite && i.CanRead);

            /*
            MetadataTypeAttribute metadata = type.GetCustomAttributes(typeof(MetadataTypeAttribute), false).FirstOrDefault() as MetadataTypeAttribute;
            if (metadata != null)
            {
                var ps = metadata.MetadataClassType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(i => i.CanWrite && i.CanRead);
                foreach (var mp in ps)
                {
                    var dbColAtt = mp.GetCustomAttributes(typeof(DBColumnAttribute), false).FirstOrDefault() as DBColumnAttribute;
                    if (dbColAtt != null)
                    {
                        var pi = properties.First(i => i.Name == mp.Name);
                        ColumnMappingList.Add(
                            new DBColumnMapping<T>(
                                pi,
                                string.IsNullOrEmpty(dbColAtt.ColumnName) ? mp.Name : dbColAtt.ColumnName,
                                dbColAtt.Required,
                                dbColAtt.NoInsert
                            )
                        );
                    }
                }
            }
            else
            {
            */
            var pis = properties.Where(i => i.GetCustomAttributes(typeof(DBColumnAttribute), true).Length > 0);
            foreach (var p in pis)
            {
                var dbColAtt = p.GetCustomAttributes(typeof(DBColumnAttribute), false).First() as DBColumnAttribute;

                ColumnMappingList.Add(
                    new DBColumnMapping<T>(
                        p,
                        string.IsNullOrEmpty(dbColAtt.ColumnName) ? p.Name : dbColAtt.ColumnName,
                        dbColAtt.Required,
                        dbColAtt.NoInsert
                    )
                );
            }
            //}
        }
    }
}