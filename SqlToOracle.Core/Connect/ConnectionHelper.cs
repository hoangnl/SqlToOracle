using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
namespace SqlToOracle.Core.Connect
{
    public static class ConnectionHelper
    {
        public static String SqlConnectionString
        {
            get
            {
                var connection = ConfigurationManager.ConnectionStrings["NSWSQL"].ConnectionString;
                return connection;
            }
        }

        public static String OracleConnectionString
        {
            get
            {
                var connection = ConfigurationManager.ConnectionStrings["NSWOracle"].ConnectionString;
                return connection;
            }
        }


    }
}