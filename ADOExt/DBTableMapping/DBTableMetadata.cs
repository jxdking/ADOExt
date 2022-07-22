using System;

namespace MagicEastern.ADOExt
{
    [Serializable]
    public class DBTableMetadata
    {
        [DBColumn]
        public string TABLE_SCHEMA { get; set; }

        [DBColumn]
        public string TABLE_NAME { get; set; }

        [DBColumn]
        public string COLUMN_NAME { get; set; }

        [DBColumn]
        public string DATA_TYPE { get; set; }

        [DBColumn]
        public int? CHAR_LENGTH { get; set; }

        [DBColumn]
        public int? DATA_PRECISION { get; set; }

        [DBColumn]
        public int? DATA_SCALE { get; set; }

        [DBColumn]
        public string NULLABLE { get; set; }

        [DBColumn]
        public string PK { get; set; }
    }
}