namespace MagicEastern.ADOExt.Oracle
{
    public class ResolverProvider : ResolverProviderBase
    {
        public override IDBClassResolver DBClassResolver { get; }

        public override ISqlResolver SqlResolver { get; }

        public ResolverProvider(string connStr) : base(connStr)
        {
            DBClassResolver = new DBClassResolver(connStr);
            SqlResolver = new SqlResolver();
        }
    }
}