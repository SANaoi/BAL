using UnityEngine;

namespace Aki.Scripts.Entities.EntityData
{
    public abstract class EntityData
    {
        /// <summary>
        /// 实体编号
        /// </summary>
        private int m_Id = 0;

        /// <summary>
        /// 种类编号
        /// </summary>
        private int m_TypeId = 0;

        /// <summary>
        /// 对应资源名称
        /// </summary>
        public string AssetName;

        /// <summary>
        /// 所处资源组名称
        /// </summary>
        public string GroupName;

        /// <summary>
        /// 实体的位置信息
        /// </summary>
        private Vector3 m_Position = Vector3.zero;

        /// <summary>
        /// 实体的旋转信息
        /// </summary>
        private Quaternion m_Rotation = Quaternion.identity;

        /// <summary>
        /// 实体编号。
        /// </summary>
        public int Id => m_Id;

        /// <summary>
        /// 种类编号
        /// </summary>
        public int TypeId => m_TypeId;

        /// <summary>
        /// 实体tag
        /// </summary>
        public string m_tag;

        /// <summary>
        /// 构造函数,用于初始化ID
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="typeId"></param>
        public EntityData(int entityId, int typeId)
        {
            m_Id = entityId;
            m_TypeId = typeId;
        }

        /// <summary>
        /// 实体位置。
        /// </summary>
        public Vector3 Position
        {
            get => m_Position;
            set => m_Position = value;
        }

        /// <summary>
        /// 实体朝向。
        /// </summary>
        public Quaternion Rotation
        {
            get => m_Rotation;
            set => m_Rotation = value;
        }
    }
}
