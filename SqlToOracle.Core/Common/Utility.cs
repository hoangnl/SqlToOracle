using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Oracle.DataAccess.Client;

namespace SqlToOracle.Core.Common
{
    public static class Utility
    {
        public static int GetTopQuantity()
        {
            int value;
            try
            {
                var top = ConfigurationManager.AppSettings["Top"];
                value = Convert.ToInt32(top);
            }
            catch (Exception ex)
            {
                value = 10000;
            }
            return value;
        }

        public static int GetScheduleTime()
        {
            int value;
            try
            {
                var scheduleTime = ConfigurationManager.AppSettings["ScheduleTime"];
                value = Convert.ToInt32(scheduleTime);
            }
            catch (Exception ex)
            {
                value = 5000;
            }
            return value;
        }

        public static string ConvertToSafeString(this string input)
        {
            var result = input.Replace("'", "''");
            if (input.Length > 4000)
            {
                //result = String.Format("TO_CLOB('{0}')", result);
                result = input.Substring(0, 4000);
            }
            else
            {
                result = String.Format("'{0}'", result);
            }
            return result;
        }

        public static string ConvertToStandardColumnName(this string input)
        {
            if (input.Length > 30)
            {
                return input.Substring(0, 30);
            }
            if (String.Equals(input, "comment", StringComparison.OrdinalIgnoreCase))
            {
                input = "comment_";
            }
            return input;
        }
    }
}
