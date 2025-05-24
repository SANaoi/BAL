using System;
using Aki.Procedures;
using Aki.Scripts.Definition.Constant;
using Aki.Scripts.Entities;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.Games
{
    public class GameControl
    {
        /// <summary>
        /// 流程游戏
        /// </summary>
        public ProcedureGame ProcedureGame;

        public GameControl(object userData)
        {
            ProcedureGame = userData as ProcedureGame;

        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            InitUI();
            InitEntity();
        }

        /// <summary>
        /// 更新
        /// </summary>
        void Update()
        {
        }

        private void InitEntity()
        {
            PlayerData playerData = new PlayerData(GameEntry.Entity.GenerateSerialId(), (int)EnumEntity.Player);
            CharacterData characterData = new CharacterData(GameEntry.Entity.GenerateSerialId(), (int)EnumEntity.Azusa_Swimsuit);


            GameEntry.Entity.ShowEntity(playerData, typeof(PlayerLogic));
            GameEntry.Entity.ShowEntity(characterData, typeof(CharacterInteractableLogic));
        }   

        private void InitUI()
        {
        }
    }
}
