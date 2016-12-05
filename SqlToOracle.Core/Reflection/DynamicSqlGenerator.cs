using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using SqlToOracle.Core.Interface;
using SqlToOracle.Core.Common;

namespace SqlToOracle.Core.Reflection
{
    public static class DynamicSqlGenerator
    {
        public static string ToDynamicSelect(this DataRow dr, string primaryKeyField, string primaryKeyValue)
        {
            var sql = "SELECT * FROM {0} WHERE {1}";
            var condition = CreateCondition(primaryKeyField, primaryKeyValue);
            return String.Format(sql, dr["TableName"].ToString(), condition);
        }

        public static string ToDynamicInsert(this DataRow dr, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            var command = "INSERT INTO {0} ({1}) VALUES ({2})";
            var columns = String.Join(",", dr.Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName.ToUpper().ConvertToStandardColumnName()).ToArray());
            var valueList = new List<string>();
            foreach (var item in dr.Table.Columns.Cast<DataColumn>())
            {
                if (dr[item] == DBNull.Value)
                {
                    valueList.Add("NULL");
                }
                else
                {
                    if (item.DataType == typeof(int))
                    {
                        valueList.Add(dr[item].ToString());
                    }
                    else if (item.DataType == typeof(bool))
                    {
                        valueList.Add(dr[item] == Boolean.FalseString ? "0" : "1");
                    }
                    else if (item.DataType == typeof(DateTime))
                    {
                        valueList.Add(String.Format("TO_DATE('{0}', 'MM/DD/YYYY HH:MI:SS AM')", dr[item]));
                    }
                    else
                    {
                        valueList.Add((dr[item].ToString()).ConvertToSafeString());
                    }
                }
            }
            return String.Format(command, tableName.ToUpper(), columns, String.Join(",", valueList));
        }

        public static string ToDynamicUpdate(this DataRow dr, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            var command = "UPDATE {0} SET {1} WHERE {2}";
            var valueList = new List<string>();
            foreach (var item in dr.Table.Columns.Cast<DataColumn>())
            {
                if (dr[item] == DBNull.Value)
                {
                    valueList.Add((item.ToString()).ConvertToStandardColumnName() + "= NULL");
                }
                else
                {
                    if (item.DataType == typeof(int))
                    {
                        valueList.Add((item.ToString()).ConvertToStandardColumnName() + "= " + dr[item].ToString());
                    }
                    else if (item.DataType == typeof(bool))
                    {
                        valueList.Add((item.ToString()).ConvertToStandardColumnName() + "= " + (dr[item] == Boolean.FalseString ? "0" : "1"));
                    }
                    else if (item.DataType == typeof(DateTime))
                    {
                        valueList.Add((item.ToString()).ConvertToStandardColumnName() + "= " + String.Format("TO_DATE('{0}', 'MM/DD/YYYY HH:MI:SS AM')", dr[item]));
                    }
                    else
                    {
                        valueList.Add((item.ToString()).ConvertToStandardColumnName() + "= " + (dr[item].ToString()).ConvertToSafeString());
                    }
                }
            }
            var condition = CreateCondition(primaryKeyField, primaryKeyValue);
            return String.Format(command, tableName.ToUpper(), String.Join(",", valueList), condition);
        }

        public static string ToDynamicDelete(this DataRow dr, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            var command = "DELETE FROM {0} WHERE {1}";
            var condition = CreateCondition(primaryKeyField, primaryKeyValue);
            return String.Format(command, tableName.ToUpper(), condition);
        }

        public static string ToDynamicMerge(this DataRow dr, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            var command = @"BEGIN
                                {0}; 
                                IF (SQL%ROWCOUNT = 0) THEN 
                                            {1};
                                END IF;
                            END;";
            primaryKeyValue = dr[primaryKeyField].ToString();
            var updateCommand = ToDynamicUpdate(dr, tableName, primaryKeyField, primaryKeyValue);
            var insertCommand = ToDynamicInsert(dr, tableName, primaryKeyField, primaryKeyValue);
            return String.Format(command, updateCommand, insertCommand);

        }

        private static string CreateCondition(string primaryKeyField, string primaryKeyValue)
        {
            var conditionalList = new List<String>();
            var determine = new char[] { ',' };
            var primaryKeys = primaryKeyField.Split(determine, StringSplitOptions.RemoveEmptyEntries);
            var primaryValues = primaryKeyValue.Split(determine, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < primaryKeys.Length; i++)
            {
                conditionalList.Add(String.Format("{0} = '{1}'", primaryKeys[i].ToUpper().ConvertToStandardColumnName(), primaryValues[i]));
            }
            var condition = String.Join(" AND ", conditionalList);
            return condition;
        }
    }


}
