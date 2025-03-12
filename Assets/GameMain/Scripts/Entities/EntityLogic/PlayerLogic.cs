using System.Collections.Generic;
using Aki.Scripts.FSM;
using GameFramework.Fsm;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;


namespace Aki.Scripts.Entities
{
    public class PlayerLogic : DefaultEntityLogic
    {
        private Camera m_Camera;

        public  PlayerData playerData;

        public int MOVE_COMMANDS;

        protected IFsm<PlayerLogic> fsm;

        protected List<FsmState<PlayerLogic>> stateList;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_Camera = Camera.main;
            playerData = userData as PlayerData;
            stateList = new List<FsmState<PlayerLogic>>();
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            if (playerData == null)
            {
                Log.Error("Player data is invalid.");
                return;
            }
            CreateFsm();
        }

        private void OnDestroy()
        {
            GameEntry.Fsm.DestroyFsm(fsm);
        }

        protected virtual void AddFsmState()
        {
            stateList.Add(PlayerIdleState.Create());
            stateList.Add(PlayerMoveState.Create());
        }

        protected virtual void StartState()
        {
            fsm.Start<PlayerIdleState>();
        }

        /// <summary>
        /// 创建状态机
        /// </summary>
        protected virtual void CreateFsm()
        {
            AddFsmState();
            fsm = GameEntry.Fsm.CreateFsm<PlayerLogic>(gameObject.name, this, stateList.ToArray());
            StartState();
        }
    }
}
