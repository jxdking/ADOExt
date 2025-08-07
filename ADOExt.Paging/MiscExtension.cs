using System.Data;

namespace MagicEastern.ADOExt.Paging
{
    public static partial class MiscExtension
    {
        public static bool TryRemoveColumn(this DataColumnCollection cols, string columnName)
        {
            int idx = cols.IndexOf(columnName);
            if (idx >= 0)
            {
                cols.Remove(columnName);
                return true;
            }
            return false;
        }
    }
}
