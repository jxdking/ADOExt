using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public interface ISql
    {
        int CommandTimeout { get; set; }
        List<Parameter> Parameters { get; set; }
        string Text { get; set; }

        string ToString();

        Sql Clone();
    }
}