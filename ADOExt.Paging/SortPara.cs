using System;

namespace MagicEastern.ADOExt.Paging
{
    public class SortPara
    {
        public string PropertyName { get; set; }
        public bool IsDesc { get; set; }

        public override string ToString()
        {
            if (!IsDesc)
            {
                return PropertyName;
            }
            return PropertyName + " desc";
        }

        public static implicit operator SortPara(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { throw new ArgumentNullException("InputStr"); }
            str = str.Trim();
            SortPara ret = new SortPara();

            int i = str.LastIndexOf(' ');

            if (i == -1)
            {
                ret.PropertyName = str;
                return ret;
            }

            string ascOrDesc = str.Substring(i + 1);
            if (ascOrDesc.ToLower() == "asc")
            {
                ret.PropertyName = str.Substring(0, i).Trim();
                return ret;
            }
            if (ascOrDesc.ToLower() == "desc")
            {
                ret.IsDesc = true;
                ret.PropertyName = str.Substring(0, i).Trim();
                return ret;
            }

            ret.PropertyName = str;
            return ret;
        }
    }
}