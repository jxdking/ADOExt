using Newtonsoft.Json;

namespace MagicEastern.ADOExt
{
    public static class SqlExt
    {
        public static bool AppendIfNotNull(this Sql sql, string txt, Parameter para)
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

        public static string ToJsonString(this Sql sql)
        {
            return JsonConvert.SerializeObject(sql, Formatting.Indented);
        }
    }
}