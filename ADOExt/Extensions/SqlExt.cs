using System.Collections.Generic;
using System.Data;

namespace MagicEastern.ADOExt
{
    public static class SqlExt
    {
        public static bool AppendIfNotNull(this Sql sql, string txt, IDbDataParameter para)
        {
            var p = para.Value;
            if (p != null && (!string.IsNullOrWhiteSpace(p as string) || !(p is string)))
            {
                sql.Text += " " + txt;
                sql.Parameters.Add(para);
                return true;
            }
            return false;
        }

        public static void Add(this Dictionary<string, IDbDataParameter> dic, IDbDataParameter para)
        {
            dic[para.ParameterName] = para;
        }
    }
}