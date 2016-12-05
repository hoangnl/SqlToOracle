using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlToOracle.Core.Interface
{
    public interface IDestionation
    {
        object ExcuteRawQuery(string text);
        int ExcuteRawNonQuery(string text);
    }
}
