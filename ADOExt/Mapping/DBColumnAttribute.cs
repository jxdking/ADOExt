namespace MagicEastern.ADOExt
{
    public class DBColumnAttribute : System.Attribute
    {
        public string ColumnName { get; set; }
        public bool NoInsert { get; set; }
        public bool Required { get; set; }
    }
}