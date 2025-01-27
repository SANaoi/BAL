using System;
using Aki.Utility;
using GameFramework;
using UnityGameFramework.Runtime;

namespace Aki.DataTable
{
    public static class DataTableExtension
    {
        private const string DataRowClassPrefixName = "Aki.DataTable.DR";
        private static readonly string[] ColumnSplit = new string[] {"\t"};

        public static void LoadDataTable(this DataTableComponent dataTableComponent, string dataTableName,
            LoadType loadType, object userData = null)
        {
            if (string.IsNullOrEmpty(dataTableName))
            {
                Log.Warning("Data table name is invalid.");
                return;
            }

            string[] splitNames = dataTableName.Split('_');
            if (splitNames.Length > 2)
            {
                Log.Warning("Data table name is invalid.");
                return;
            }

            string dataRowClassName = DataRowClassPrefixName + splitNames[0];

            Type dataRowType = Type.GetType(dataRowClassName);
            if (dataRowType == null)
            {
                Log.Warning("Can not get data row type with class name '{0}'.", dataRowClassName);
                return;
            }
            string dataTableNameInType = splitNames.Length > 1 ? splitNames[1] : null;
            dataTableComponent.LoadDataTable(dataRowType, dataTableName, dataTableNameInType,
                AssetUtility.GetDataTableAsset(dataTableName, loadType), loadType, userData);
        }

        public static string[] SplitDataRow(GameFrameworkSegment<string> dataRowSegment)
        {
            return dataRowSegment.Source.Substring(dataRowSegment.Offset, dataRowSegment.Length)
                .Split(ColumnSplit, StringSplitOptions.None);
        }
    }
}