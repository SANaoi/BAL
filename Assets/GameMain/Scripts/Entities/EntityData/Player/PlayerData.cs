using Aki.Scripts.Base;
using Aki.Scripts.DataTable;
using Aki.Scripts.ProfileMessage;
using GameFramework.DataTable;
using UnityEngine;


namespace Aki.Scripts.Entities
{
    public class PlayerData : EntityData
    {
        //玩家不同状态的运动速度
        public float walkSpeed = 2.5f;
        public float runSpeed = 5.5f;
        public PlayerData(int entityId, int typeId) : base(entityId, typeId)
        {
            IDataTable<DREntity> m_GroupTable = GameEntry.DataTable.GetDataTable<DREntity>();
            DREntity m_GroupData = m_GroupTable.GetDataRow(typeId);
            AssetName = m_GroupData.AssetName;
            GroupName = m_GroupData.GroupName;
            m_tag = m_GroupData.Tag;
            // ProfileReader.SetPlayerData(this, typeId - 9999);
        }
    }
}
