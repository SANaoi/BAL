using System;
using Aki.Scripts.DataTable;
using Aki.Scripts.Definition.Constant;
using Aki.Scripts.Utility;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.Entities
{
    public static class EntityExtension
    {
        private static int s_SerialId = 0;

        public static void ShowEntity<T>(this EntityComponent entityComponent, int serialId, EnumEntity enumEntity, object userData = null)
        {
            entityComponent.ShowEntity(serialId, enumEntity, typeof(T), userData);
        }
        public static void ShowEntity(this EntityComponent entityComponent, int serialId, EnumEntity enumEntity, Type logicType, object userData = null)
        {
            entityComponent.ShowEntity(serialId, (int)enumEntity, logicType, userData);
        }
        public static void ShowEntity<T>(this EntityComponent entityComponent, int serialId, int entityId, object userData = null)
        {
            entityComponent.ShowEntity(serialId, entityId, typeof(T), userData);
        }

        /// <summary>
        /// 显示实体
        /// </summary>
        /// <param name="entityComponent"> </param>
        /// <param name="serialId"></param>
        /// <param name="entityId"></param>
        /// <param name="logicType"></param>
        /// <param name="userData"></param>
        public static void ShowEntity(this EntityComponent entityComponent, int serialId, int entityId, Type logicType, object userData = null)
        {
        }

        public static void ShowEntity(this EntityComponent entityComponent, EntityData entityData, Type logicType)
        {
            int id = entityData.Id;
            string assetName = AssetUtility.GetEntityAsset(entityData.AssetName);
            string groupName = entityData.GroupName;

            entityComponent.ShowEntity(id, logicType, assetName, groupName, entityData);
        }

        /// <summary>
        /// 生成自增的实体序列号
        /// </summary>
        /// <param name="entityComponent">实体组件</param>
        /// <returns>新生成的唯一序列号</returns>
        /// <remarks>
        /// 注意：该序列号在应用程序域内全局唯一且单调递增，
        /// 但不保证跨程序或持久化存储的唯一性
        /// </remarks>
        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            return ++s_SerialId;
        }
    }
}
