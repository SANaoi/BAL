using System;
using Aki.Procedures;
using Aki.Scripts.Entities;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.Games
{
    public class GameControl
    {
        /// <summary>
        /// 流程游戏
        /// </summary>
        public ProcedureGame ProcedureGame;

        public GameControl(Object userData)
        {
            ProcedureGame = userData as ProcedureGame;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            InitPlayer();
        }

        /// <summary>
        /// 更新
        /// </summary>
        void Update()
        {
        }

        private void InitPlayer()
        {
            PlayerData playerData = new PlayerData(GameEntry.Entity.GenerateSerialId(), (int)EnumEntity.Player);
        
            GameEntry.Entity.ShowEntity(playerData, typeof(PlayerLogic));
        }
    }
}
