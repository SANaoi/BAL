using Aki.Scripts.Base;
using Aki.Scripts.DataTable;
using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.Entities
{
    public class CameraData : EntityData
    {
        private DRCamera m_GroupData;
        private int m_Id;
        private int m_TypeId;

        public Vector3 DefaultLocalPosition
        {
            get
            {
                return m_GroupData.DefaultLocalPosition;
            }
        }
        public int entityId
        {
            get
            {
                return m_Id;
            }
        }
        public int typeId
        {
            get
            {
                return m_TypeId;
            }
        }
        public CameraData(int entityId, int typeId) : base(entityId, typeId)
        {
            IDataTable<DRCamera> m_GroupTable = GameEntry.DataTable.GetDataTable<DRCamera>();
            DRCamera m_GroupData = m_GroupTable.GetDataRow(typeId);
            this.m_GroupData = m_GroupData;
            AssetName = m_GroupData.AssetName;
            GroupName = m_GroupData.GroupName;
            this.m_Id = entityId;
            this.m_TypeId = typeId;
            Log.Info("CameraData: " + entityId + " " + typeId);
        }

    }
}