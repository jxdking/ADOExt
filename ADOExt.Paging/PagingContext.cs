using System;
using System.Collections.Generic;

namespace MagicEastern.ADOExt.Paging
{
    class PagingContext<T> : IPagingContext where T : PagingParameters, new()
    {
        private T orgPP;

        public int PageSize => orgPP.PageSize;

        public int CurrentPage => orgPP.CurrentPage;

        public string SortParaStr => orgPP.SortParaStr;

        public string MultiSort => orgPP.MultiSort;

        public int TotalPages => (TotalLines - 1) / PageSize + 1;

        public int TotalLines { get; set; }

        private List<SortPara> sortParaList;

        public List<SortPara> SortParaList
        {
            get
            {
                if (sortParaList == null)
                {
                    sortParaList = new List<SortPara>();
                    if (!string.IsNullOrWhiteSpace(SortParaStr))
                    {
                        string[] sortStrs = SortParaStr.Split(new string[] { "--" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < sortStrs.Length; i++)
                        {
                            sortParaList.Add(sortStrs[i]);
                        }
                    }
                }
                return sortParaList;
            }
        }


        List<string> IPagingContext.SortableExpressions => orgPP.SortableExpressions;

        public PagingContext(T pp)
        {
            orgPP = (T)pp.ShallowCopy();
        }

        public PagingParameters GetPagingParaCopy()
        {
            var ret = (T)orgPP.ShallowCopy();
            return ret;
        }

        public List<PagingLink> GetPageLinks()
        {
            int window = orgPP.PaginationWidth;

            List<PagingLink> lks = new List<PagingLink>();
            PagingParameters pp;

            //first
            pp = GetPagingParaCopy();
            pp.CurrentPage = 1;
            lks.Add(new PagingLink { Text = "First", RoutValues = pp });

            //previous
            pp = GetPagingParaCopy();
            if (pp.CurrentPage > 1)
            {
                pp.CurrentPage--;
            }
            lks.Add(new PagingLink { Text = "Previous", RoutValues = pp });

            //pagination
            int endPage = Math.Min(TotalPages, (CurrentPage + window / 2));
            int startPage = Math.Max(1, endPage - window + 1);
            endPage = Math.Max(endPage, Math.Min(startPage + window - 1, TotalPages));

            //between first and start
            if (startPage / 2 > 1)
            {
                pp = GetPagingParaCopy();
                pp.CurrentPage = startPage / 2;
                lks.Add(new PagingLink { Text = "...", RoutValues = pp });
            }

            //pages
            for (int i = startPage; i <= endPage; i++)
            {
                pp = GetPagingParaCopy();
                pp.CurrentPage = i;
                lks.Add(new PagingLink { Text = i.ToString(), RoutValues = pp, Active = i == CurrentPage });
            }

            //between end and total
            if ((endPage + TotalPages) / 2 < TotalPages && (endPage + TotalPages) / 2 > endPage)
            {
                pp = GetPagingParaCopy();
                pp.CurrentPage = (endPage + TotalPages) / 2;
                lks.Add(new PagingLink { Text = "...", RoutValues = pp });
            }

            //next
            pp = GetPagingParaCopy();
            if (pp.CurrentPage < TotalPages)
            {
                pp.CurrentPage++;
            }
            lks.Add(new PagingLink { Text = "Next", RoutValues = pp });

            //last
            pp = GetPagingParaCopy();
            pp.CurrentPage = TotalPages;
            lks.Add(new PagingLink { Text = "Last", RoutValues = pp });

            return lks;
        }
    }
}