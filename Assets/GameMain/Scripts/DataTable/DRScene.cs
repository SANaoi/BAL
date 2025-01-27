using GameFramework;
using UnityGameFramework.Runtime;

namespace Aki.DataTable
{
    /// <summary>
    /// 场景表
    /// </summary>
    public class DRScene : DataRowBase
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
        
        public override bool ParseDataRow(GameFrameworkSegment<string> dataRowSegment)
        {
            string[] text = DataTableExtension.SplitDataRow(dataRowSegment);
            int index = 0;
            index++;
            m_Id = int.Parse(text[index++]);
            index++;
            AssetName = text[index++];
            return true;
        }
    }
}