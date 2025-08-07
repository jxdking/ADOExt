namespace MagicEastern.ADOExt.Paging
{
    public interface IPagingParameters
    {
        int CurrentPage { get; }
        string MultiSort { get; }
        int PageSize { get; }
        string SortParaStr { get; }

    }
}