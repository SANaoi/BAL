using Aki.Scripts.Base;
using Aki.Scripts.DataTable;
using GameFramework.DataTable;
using UnityEngine;

namespace Aki.Scripts.Entities
{
    public class CameraData : EntityData
    {
        private DRCamera m_GroupData;

        public Vector3 DefaultLocalPosition
        {
            get
            {
                return m_GroupData.DefaultLocalPosition;
            }
        }
        public CameraData(int entityId, int typeId) : base(entityId, typeId)
        {
            IDataTable<DRCamera> m_GroupTable = GameEntry.DataTable.GetDataTable<DRCamera>();
            DRCamera m_GroupData = m_GroupTable.GetDataRow(typeId);
            this.m_GroupData = m_GroupData;
        }

    }
}