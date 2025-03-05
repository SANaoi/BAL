using GameFramework;
using UnityGameFramework.Runtime;

namespace Aki.Scripts.DataTable
{
    /// <summary>
    /// 实体表。
    /// </summary>
    public class DREntity : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 实体编号。
        /// </summary>
        public override int Id
        {
            get { return m_Id; }
        }

        /// <summary>
        /// 资源名称。
        /// </summary>
        public string AssetName { get; private set; }

        /// <summary>
        /// 资源组名称
        /// </summary>
        public string GroupName { get; private set; }

        /// <summary>
        /// tag标识
        /// </summary>
        public string tag { get; set; }

        public override bool ParseDataRow(GameFrameworkSegment<string> dataRowSegment)
        {
            string[] text = DataTableExtension.SplitDataRow(dataRowSegment);
            int index = 0;
            index++;
            m_Id = int.Parse(text[index++]);
            index++;
            AssetName = text[index++];
            GroupName = text[index++];
            tag = text[index++];
            return true;
        }
    }

}