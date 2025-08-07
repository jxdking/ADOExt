using System.Collections.Generic;

namespace MagicEastern.ADOExt.Paging
{
    public interface IPagingContext : IPagingParameters
    {
        int TotalLines { get; set; }
        int TotalPages { get; }

        List<SortPara> SortParaList { get; }
        List<string> SortableExpressions { get; }

        PagingParameters GetPagingParaCopy();
        List<PagingLink> GetPageLinks();
    }
}