using System;

namespace MagicEastern.ADOExt
{
    public interface IDBService
    {
        IServiceProvider DBServiceProvider { get; }
        IDBCommandBuilder DBClassResolver { get; }
        ISqlResolver SqlResolver { get; }

        DBConnectionWrapper OpenConnection();
        IDBObjectMapping<T> GetDBObjectMapping<T>();
        IDBTableAdapter<T> GetDBTableAdapter<T>() where T : new();
    }
}