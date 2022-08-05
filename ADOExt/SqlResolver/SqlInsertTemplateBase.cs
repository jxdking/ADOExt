﻿using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public abstract class SqlInsertTemplateBase<T> where T : new()
    {
        public abstract Sql Generate(T obj, IEnumerable<IDBColumnMapping<T>> setCols);

        public abstract int Execute(T obj, IEnumerable<IDBColumnMapping<T>> setCols, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans);
    }
}
