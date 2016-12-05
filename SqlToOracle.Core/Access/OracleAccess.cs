using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Client;
using SqlToOracle.Core.Connect;
using SqlToOracle.Core.Interface;

namespace SqlToOracle.Core.Access
{
    public class OracleAccess : ISource, IDestionation
    {
        public object ExcuteRawQuery(string text)
        {
            var connectionString = ConnectionHelper.OracleConnectionString;
            var ds = OracleHelper.ExecuteDataset(connectionString, text, null);
            DataTable dt = ds == null ? null : ds.Tables[0];
            return dt;
        }

        public int ExcuteRawNonQuery(string spName)
        {
            var connectionString = ConnectionHelper.OracleConnectionString;
            return OracleHelper.ExecuteNonQueryByText(connectionString, spName);
        }
    }
}
