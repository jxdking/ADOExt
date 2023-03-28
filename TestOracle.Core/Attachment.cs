using MagicEastern.ADOExt;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Text;

namespace TestOracle.Core
{
    [DBTable("attachment")]
    internal class Attachment
    {
        [DBColumn(NoInsert = true)] public int ID { get; set; }
        [DBColumn] public string FILENAME { get; set; }
        [DBColumn] public string CONTENT { get; set; }
    }
}
