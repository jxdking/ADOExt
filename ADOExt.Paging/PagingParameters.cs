using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.Paging
{
    public class PagingParameters : IPagingParameters
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        /// <summary>
        /// "--" Separated sort expressions, if there are 2 or more sort expressions. Format of "sort expression": [column name] [(optional)desc].
        /// </summary>
        public string SortParaStr { get; set; }

        /// <summary>
        /// Y or N
        /// </summary>
        public string MultiSort { get; set; }

        public int PaginationWidth = 5;
        public List<string> SortableExpressions = new List<string>();

        private IPagingContext PagingCtx;

        public PagingParameters ShallowCopy()
        {
            return (PagingParameters)this.MemberwiseClone();
        }

        private IPagingContext GetPagingContext<T>(T pp) where T : PagingParameters, new()
        {
            return new PagingContext<T>(pp);
        }

        public IPagingContext GetPagingContext()
        {
            if (PagingCtx == null)
            {
                PagingCtx = GetPagingContext(this);
            }
            return PagingCtx;
        }

        public PagingParameters()
        {
            PageSize = 20;
            CurrentPage = 1;
        }

        public void SetSortParaStr(List<SortPara> sortParas)
        {
            SortParaStr = string.Join("--", sortParas.Select(i => i.IsDesc ? (i.PropertyName + " desc") : i.PropertyName)) + "--";
        }
    }
}