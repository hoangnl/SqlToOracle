using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlToOracle.Core.Interface;
using SqlToOracle.Core.Common;
using System.Data;

namespace SqlToOracle.Core.Reflection
{
    public abstract class BaseCommand : ICommand
    {
        protected DataTable _data;
        protected string _tableName;
        protected string _primaryKeyField;
        protected string _primaryKeyValue;
        public BaseCommand(DataTable data, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            _data = data;
            _tableName = tableName;
            _primaryKeyField = primaryKeyField;
            _primaryKeyValue = primaryKeyValue;
        }

        public abstract List<string> Excute();
    }

    public static class ChangeFactory
    {
        private static DataTable _data;
        private static string _tableName;
        private static string _primaryKeyField;
        private static string _primaryKeyValue;

        private static readonly Dictionary<string, Func<ICommand>> DeclarationMap = new Dictionary<string, Func<ICommand>>
         {
            {ChangeType.INSERTED, () =>{return new InsertCommand(_data, _tableName, _primaryKeyField, _primaryKeyValue);}},
            {ChangeType.UPDATED , () =>{return new UpdateCommand(_data, _tableName, _primaryKeyField, _primaryKeyValue);}},
            {ChangeType.DELETED , () =>{return new DeleteCommand(_data, _tableName, _primaryKeyField, _primaryKeyValue);}},
            {ChangeType.MERGED , () =>{return new MergeCommand(_data, _tableName, _primaryKeyField, _primaryKeyValue);}}
         };

        public static ICommand CreateCommand(string type, DataTable data, string tableName, string primaryKeyField, string primaryKeyValue)
        {
            _data = data;
            _tableName = tableName;
            _primaryKeyField = primaryKeyField;
            _primaryKeyValue = primaryKeyValue;
            return DeclarationMap[type]();
        }
    }

    public class InsertCommand : BaseCommand
    {
        public InsertCommand(DataTable data, string tableName, string primaryKeyField, string primaryKeyValue)
            : base(data, tableName, primaryKeyField, primaryKeyValue)
        {

        }

        public override List<string> Excute()
        {
            var result = new List<string>();
            foreach (DataRow dr in _data.Rows)
            {
                result.Add(dr.ToDynamicInsert(_tableName, _primaryKeyField, _primaryKeyValue));
            }
            return result;
        }
    }

    public class UpdateCommand : BaseCommand
    {
        public UpdateCommand(DataTable data, string tableName, string primaryKeyField, string primaryKeyValue)
            : base(data, tableName, primaryKeyField, primaryKeyValue)
        {

        }

        public override List<string> Excute()
        {
            var result = new List<string>();
            foreach (DataRow dr in _data.Rows)
            {
                result.Add(dr.ToDynamicUpdate(_tableName, _primaryKeyField, _primaryKeyValue));
            }
            return result;
        }
    }

    public class DeleteCommand : BaseCommand
    {
        public DeleteCommand(DataTable data, string tableName, string primaryKeyField, string primaryKeyValue)
            : base(data, tableName, primaryKeyField, primaryKeyValue)
        {

        }
        public override List<string> Excute()
        {
            var result = new List<string>();
            if (_data.Rows.Count > 0)
            {
                foreach (DataRow dr in _data.Rows)
                {
                    result.Add(dr.ToDynamicDelete(_tableName, _primaryKeyField, _primaryKeyValue));
                }
            }
            else
            {
                result.Add(((DataRow)null).ToDynamicDelete(_tableName, _primaryKeyField, _primaryKeyValue));
            }
            return result;
        }
    }

    public class MergeCommand : BaseCommand
    {
        public MergeCommand(DataTable data, string tableName, string primaryKeyField, string primaryKeyValue)
            : base(data, tableName, primaryKeyField, primaryKeyValue)
        {

        }
        public override List<string> Excute()
        {
            var result = new List<string>();
            foreach (DataRow dr in _data.Rows)
            {
                result.Add(dr.ToDynamicMerge(_tableName, _primaryKeyField, _primaryKeyValue));
            }

            return result;
        }
    }
}
