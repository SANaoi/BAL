using GameFramework;
using UnityGameFramework.Runtime;

namespace Aki.DataTable
{
    /// <summary>
    /// UI表
    /// </summary>
    public class DRUIForm : DataRowBase
    {
        private int m_Id;

        /// <summary>
        /// ID
        /// </summary>
        public override int Id => m_Id;

        /// <summary>
        /// 资源名称
        /// </summary>
        public string AssetName { get; set; }
        
        /// <summary>
        /// 资源组名称
        /// </summary>
        public string GroupName { get; set; }
        
        /// <summary>
        /// 是否允许多个界面同时存在
        /// </summary>
        public bool AllowMultiInstance { get; set; }
        
        /// <summary>
        /// 是否暂停被其覆盖的界面
        /// </summary>
        public bool PauseCoveredUIForm { get; set; }
        
        public override bool ParseDataRow(GameFrameworkSegment<string> dataRowSegment)
        {
            string[] text = DataTableExtension.SplitDataRow(dataRowSegment);
            int index = 0;
            index++;
            m_Id = int.Parse(text[index++]);
            index++;
            AssetName = text[index++];
            GroupName = text[index++];
            AllowMultiInstance = bool.Parse(text[index++]);
            PauseCoveredUIForm = bool.Parse(text[index++]);
            return true;
        }
    }
}