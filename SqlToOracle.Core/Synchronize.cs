using System;
using SqlToOracle.Core.Access;
using SqlToOracle.Core.Reflection;
using System.Data;
using System.Collections.Generic;
using SqlToOracle.Core.Common;
using SqlToOracle.Core.Interface;

namespace SqlToOracle.Core
{
    public class Synchronize
    {
        private ISource _sourceAccess;
        private IDestionation _destinationAccess;


        public Synchronize()
        {
            _sourceAccess = new SqlAccess();
            _destinationAccess = new OracleAccess();
        }

        public void Run()
        {
            // Lấy thông tin cập nhật thay đổi trong DB Sql dựa theo trigger
            var selectCommand = String.Format("SELECT TOP ({0}) * FROM [LogTable] WHERE Status = {1} ORDER BY LogId", Utility.GetTopQuantity(), (int)Status.NEW);
            var logs = _sourceAccess.ExcuteRawQuery(selectCommand) as DataTable;

            var dict = new Dictionary<String, List<String>>();
            var updateLogCommand = "UPDATE [LogTable] SET Status = {0} WHERE LogId = {1}";
            var deleteLogCommand = "DELETE FROM [LogTable] WHERE LogId = {0}";

            foreach (DataRow item in logs.Rows)
            {
                //Đánh dấu là trạng thái đang tiến hành đồng bộ
                var logId = item["LogId"].ToString();
                _sourceAccess.ExcuteRawNonQuery(String.Format(updateLogCommand, (int)Status.SYNCED, item["LogId"]));

                var primaryKeyField = item["PrimaryKeyField"].ToString();
                var primaryKeyValue = item["PrimaryKeyValue"].ToString();
                var tableName = item["TableName"].ToString().Replace(",", "");

                // Tạo câu truy vấn lấy dữ liệu từ CSDL Sql dựa theo khóa chính của từng bảng
                var selectQuery = item.ToDynamicSelect(primaryKeyField, primaryKeyValue);
                var dt = _sourceAccess.ExcuteRawQuery(selectQuery) as DataTable;

                // Với từng trường hợp Insert, Update, Delete tạo câu lệnh tương ứng để excute trên DB Oracle
                var type = item["Type"].ToString();
                var changeCommand = ChangeFactory.CreateCommand(type, dt, tableName, primaryKeyField, primaryKeyValue);
                var sqlList = changeCommand.Excute();
                dict.Add(logId, sqlList);
            }

            //Tiến hành đồng bộ sang CSDL Oracle dựa vào câu lệnh sinh ra
            foreach (var item in dict)
            {
                try
                {
                    foreach (var tmp in item.Value)
                    {
                        _destinationAccess.ExcuteRawNonQuery(tmp);
                    }
                    _sourceAccess.ExcuteRawNonQuery(String.Format(deleteLogCommand, item.Key));
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("Error sync with logId {0}: {1}", item.Key, ex));
                    _sourceAccess.ExcuteRawNonQuery(String.Format(updateLogCommand, (int)Status.ERROR, item.Key));
                }
            }
        }

        public void Run(string tableName,
                        string primaryKeyField,
                        bool isTruncated = true)
        {
            if (isTruncated)
            {
                var truncateCommand = String.Format("TRUNCATE TABLE {0}", tableName);
                _destinationAccess.ExcuteRawNonQuery(truncateCommand);
            }
            var selectCommand = String.Format("SELECT COUNT(1) Total FROM {0}", tableName);
            var total = _sourceAccess.ExcuteRawQuery(selectCommand) as DataTable;
            var pageSize = Utility.GetTopQuantity();
            var pageNumber = Convert.ToInt32(total.Rows[0]["Total"]) / pageSize + 1;
            var dict = new Dictionary<String, List<String>>();
            for (int index = 1; index <= pageNumber; index++)
            {
                var selectQuery = String.Format(@";WITH x AS (SELECT {1}, k = ROW_NUMBER() OVER (ORDER BY {1}) FROM {0})
                                                    SELECT e.*
                                                    FROM x INNER JOIN {0} AS e
                                                    ON x.{1} = e.{1}
                                                    WHERE x.k BETWEEN  {2} AND {3}",
                                                    tableName,
                                                    primaryKeyField,
                                                    (index - 1) * pageSize + 1,
                                                    index * pageSize);
                var dt = _sourceAccess.ExcuteRawQuery(selectQuery) as DataTable;
                var changeCommand = ChangeFactory.CreateCommand(ChangeType.INSERTED, dt, tableName, primaryKeyField, string.Empty);
                var sqlList = changeCommand.Excute();
                dict.Add(index.ToString(), sqlList);
            }
            foreach (var item in dict)
            {
                try
                {
                    foreach (var tmp in item.Value)
                    {
                        _destinationAccess.ExcuteRawNonQuery(tmp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("Error sync with logId: {0}", ex));
                }
            }
        }


        public void Run(string tableName,
                        string primaryKeyField,
                        string createdDateColumn,
                        string startDate,
                        string endDate)
        {
            var selectCommand = String.Format("SELECT COUNT(1) Total FROM {0} WHERE {1} >= '{2}' AND {1} < '{3}'", tableName, createdDateColumn, startDate, endDate);
            var total = _sourceAccess.ExcuteRawQuery(selectCommand) as DataTable;
            var pageSize = Utility.GetTopQuantity();
            var pageNumber = Convert.ToInt32(total.Rows[0]["Total"]) / pageSize + 1;
            var dict = new Dictionary<String, List<String>>();
            for (int index = 1; index <= pageNumber; index++)
            {
                var selectQuery = String.Format(@";WITH x AS (SELECT {1}, k = ROW_NUMBER() OVER (ORDER BY {1}) FROM {0} WHERE {4} >= '{5}' AND {4} < '{6}')
                                                    SELECT e.*
                                                    FROM x INNER JOIN {0} AS e
                                                    ON x.{1} = e.{1}
                                                    WHERE x.k BETWEEN  {2} AND {3}",
                                                    tableName,
                                                    primaryKeyField,
                                                    (index - 1) * pageSize + 1,
                                                    index * pageSize,
                                                    createdDateColumn,
                                                    startDate,
                                                    endDate);
                var dt = _sourceAccess.ExcuteRawQuery(selectQuery) as DataTable;
                var changeCommand = ChangeFactory.CreateCommand(ChangeType.MERGED, dt, tableName, primaryKeyField, string.Empty);
                var sqlList = changeCommand.Excute();
                dict.Add(index.ToString(), sqlList);
            }
            foreach (var item in dict)
            {
                try
                {
                    foreach (var tmp in item.Value)
                    {
                        _destinationAccess.ExcuteRawNonQuery(tmp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("Error sync with logId: {0}", ex));
                }
            }
        }


        public void Run(string childTableName,
                        string childPrimaryKeyField,
                        string parentTableName,
                        string parentPrimaryKeyField,
                        string createdDateColumn,
                        string startDate,
                        string endDate)
        {
            var command = @"select {7} from {2}
                                                where exists (select top 1 1 from  {0}
                                                where  {4} > '{5}' AND {4} < '{6}'
                                                and  {0}.{1} = {2}.{1}
                                                )";
            var selectCommand = String.Format(command,
                parentTableName,
                parentPrimaryKeyField,
                childTableName,
                childPrimaryKeyField,
                createdDateColumn,
                startDate,
                endDate,
                "count(1) as Total");
            var total = _sourceAccess.ExcuteRawQuery(selectCommand) as DataTable;
            var pageSize = Utility.GetTopQuantity();
            var pageNumber = Convert.ToInt32(total.Rows[0]["Total"]) / pageSize + 1;
            var dict = new Dictionary<String, List<String>>();
            for (int index = 1; index <= pageNumber; index++)
            {
                var pagingCommand = String.Format(command,
                 parentTableName,
                 parentPrimaryKeyField,
                 childTableName,
                 childPrimaryKeyField,
                 createdDateColumn,
                 startDate,
                 endDate,
                 "{1}, k = ROW_NUMBER() OVER (ORDER BY {1})");
                var selectQuery = String.Format(@";WITH x AS (" + pagingCommand + @")
                                                    SELECT e.*
                                                    FROM x INNER JOIN {0} AS e
                                                    ON x.{1} = e.{1}
                                                    WHERE x.k BETWEEN  {2} AND {3}",
                                                    childTableName,
                                                    childPrimaryKeyField,
                                                    (index - 1) * pageSize + 1,
                                                    index * pageSize);
                var dt = _sourceAccess.ExcuteRawQuery(selectQuery) as DataTable;
                var changeCommand = ChangeFactory.CreateCommand(ChangeType.MERGED, dt, childTableName, childPrimaryKeyField, string.Empty);
                var sqlList = changeCommand.Excute();
                dict.Add(index.ToString(), sqlList);
            }
            foreach (var item in dict)
            {
                try
                {
                    foreach (var tmp in item.Value)
                    {
                        _destinationAccess.ExcuteRawNonQuery(tmp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("Error sync with logId {0}: {1}", item.Key, ex));
                }
            }
        }

        public void Run(string source,
                        string tableName,
                        string primaryKeyField)
        {
            var determine = new char[] { ',' };
            var primaryKeyValues = source.Split(determine, StringSplitOptions.RemoveEmptyEntries);
            var dict = new Dictionary<String, List<String>>();
            for (int index = 0; index < primaryKeyValues.Length; index++)
            {
                var selectQuery = String.Format(@"SElECT * FROM {0} WHERE {1} = {2}", tableName, primaryKeyField, primaryKeyValues[index]);
                var dt = _sourceAccess.ExcuteRawQuery(selectQuery) as DataTable;
                var changeCommand = ChangeFactory.CreateCommand(ChangeType.INSERTED, dt, tableName, primaryKeyField, string.Empty);
                var sqlList = changeCommand.Excute();
                dict.Add(index.ToString(), sqlList);
            }
            foreach (var item in dict)
            {
                try
                {
                    foreach (var tmp in item.Value)
                    {
                        _destinationAccess.ExcuteRawNonQuery(tmp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(String.Format("Error sync with logId {0}: {1}", item.Key, ex));
                }
            }
        }
    }
}
