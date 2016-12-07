using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlToOracle.Core.Common
{
    public enum Status
    {
        NEW = 0,
        SCANED = 1,
        SYNCED = 2,
        PENDING = 3,
        ERROR = 4
    }

    public class ChangeType
    {
        public const string SELECTED = "S";
        public const string INSERTED = "I";
        public const string UPDATED = "U";
        public const string DELETED = "D";
        public const string MERGED = "M";
    }

    public class Table
    {
        public const string DOCUMENT = "Document";
        public const string GOODDECLARATION = "GoodDeclaration";
        public const string HOUSEBILL = "HOUSEBILL";
        public const string CREW = "CREW";
    }
}
