using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SqlToOracle.Core.Connect;
using SqlToOracle.Core.Interface;

namespace SqlToOracle.Core.Access
{
    public class SqlAccess : ISource, IDestionation
    {
        public object ExcuteRawQuery(string text)
        {
            var connectionString = ConnectionHelper.SqlConnectionString;
            var ds = SqlHelper.ExecuteDatasetByText(connectionString, text);
            DataTable dt = ds == null ? null : ds.Tables[0];
            return dt;
        }

        public int ExcuteRawNonQuery(string text)
        {
            var connectionString = ConnectionHelper.SqlConnectionString;
            return SqlHelper.ExecuteRawNonQuery(connectionString, text);
        }
    }
}
