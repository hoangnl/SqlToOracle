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
            var condition = CreateCondition(primaryKeyField, primaryKeyValue, ChangeType.SELECTED);
            return String.Format(sql, dr["TableName"].ToString(), condition);
        }

        public static string ToDynamicInsert(this DataRow dr, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            var command = @"DECLARE
                                {3}
                            BEGIN
                                {4}
                                INSERT INTO {0} ({1}) VALUES ({2});
                            END;";
            var columns = String.Join(",", dr.Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName.ToUpper().ConvertToStandardName()).ToArray());
            var declareCommand = string.Empty;
            var setCommand = string.Empty;
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
                    else if (item.DataType == typeof(Guid))
                    {
                        valueList.Add("'" + (dr[item].ToString()).ToUpper() + "'");
                    }
                    else if (item.DataType == typeof(byte[]))
                    {
                        var hex = Utility.ConvertByteArrayToString(((byte[])dr[item]));
                        declareCommand += String.Format("p{0} RAW(100000); ", item.ToString());
                        setCommand += String.Format("p{0} := hextoraw('{1}');", item.ToString(), hex);
                        valueList.Add(String.Format("p{0}", item.ToString()));
                    }
                    else
                    {
                        var tmp = dr[item].ToString();
                        if (tmp.Length > 2000)
                        {
                            declareCommand += String.Format("p{0} varchar2(32767); ", item.ToString());
                            setCommand += String.Format("p{0} := {1};", item.ToString(), tmp.ConvertToSafeString());
                            valueList.Add(String.Format("p{0}", item.ToString()));
                        }
                        else
                        {
                            valueList.Add((dr[item].ToString()).ConvertToSafeString());
                        }
                    }
                }
            }
            return String.Format(command, tableName.ConvertToStandardName(), columns, String.Join(",", valueList), declareCommand, setCommand);
        }

        public static string ToDynamicUpdate(this DataRow dr, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            var command = @"DECLARE
                                {3}
                            BEGIN
                                {4}
                                UPDATE {0} SET {1} WHERE {2};
                            END;";
            var valueList = new List<string>();
            var declareCommand = string.Empty;
            var setCommand = string.Empty;
            foreach (var item in dr.Table.Columns.Cast<DataColumn>())
            {
                if (dr[item] == DBNull.Value)
                {
                    valueList.Add((item.ToString()).ConvertToStandardName() + "= NULL");
                }
                else
                {
                    if (item.DataType == typeof(int))
                    {
                        valueList.Add((item.ToString()).ConvertToStandardName() + "= " + dr[item].ToString());
                    }
                    else if (item.DataType == typeof(bool))
                    {
                        valueList.Add((item.ToString()).ConvertToStandardName() + "= " + (dr[item] == Boolean.FalseString ? "0" : "1"));
                    }
                    else if (item.DataType == typeof(DateTime))
                    {
                        valueList.Add((item.ToString()).ConvertToStandardName() + "= " + String.Format("TO_DATE('{0}', 'MM/DD/YYYY HH:MI:SS AM')", dr[item]));
                    }
                    else if (item.DataType == typeof(Guid))
                    {
                        valueList.Add((item.ToString()).ConvertToStandardName() + "= '" + (dr[item].ToString()).ToUpper() + "'");
                    }
                    else if (item.DataType == typeof(byte[]))
                    {
                        var hex = Utility.ConvertByteArrayToString(((byte[])dr[item]));
                        declareCommand += String.Format("p{0} RAW(100000); ", item.ToString());
                        setCommand += String.Format("p{0} := hextoraw('{1}');", item.ToString(), hex);
                        valueList.Add(String.Format("{0} = p{1}", (item.ToString()).ConvertToStandardName(), item.ToString()));
                    }
                    else
                    {
                        var tmp = dr[item].ToString();
                        if (tmp.Length > 2000)
                        {
                            declareCommand += String.Format("p{0} varchar2(32767); ", item.ToString());
                            setCommand += String.Format("p{0} := {1};", item.ToString(), tmp.ConvertToSafeString());
                            valueList.Add(String.Format("{0} = p{1}", (item.ToString()).ConvertToStandardName(), item.ToString()));
                        }
                        else
                        {
                            valueList.Add((item.ToString()).ConvertToStandardName() + "= " + (dr[item].ToString()).ConvertToSafeString());
                        }

                    }
                }
            }
            var condition = CreateCondition(primaryKeyField, primaryKeyValue, ChangeType.UPDATED);
            return String.Format(command, tableName.ConvertToStandardName(), String.Join(",", valueList), condition, declareCommand, setCommand);
        }

        public static string ToDynamicDelete(this DataRow dr, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            var command = "DELETE FROM {0} WHERE {1}";
            var condition = CreateCondition(primaryKeyField, primaryKeyValue, ChangeType.DELETED);
            return String.Format(command, tableName.ConvertToStandardName(), condition);
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

        private static string CreateCondition(string primaryKeyField, string primaryKeyValue, string changeType = ChangeType.SELECTED)
        {
            var conditionalList = new List<String>();
            var determine = new char[] { ',' };
            var primaryKeys = primaryKeyField.Split(determine, StringSplitOptions.RemoveEmptyEntries);
            var primaryValues = primaryKeyValue.Split(determine, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < primaryKeys.Length; i++)
            {
                conditionalList.Add(String.Format("{0} = '{1}'",
                    changeType != ChangeType.SELECTED ? primaryKeys[i].ToUpper().ConvertToStandardName() : primaryKeys[i].ToUpper(),
                    primaryValues[i]));
            }
            var condition = String.Join(" AND ", conditionalList);
            return condition;
        }
    }


}
