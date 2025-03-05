
using System.Collections.Generic;
using Aki.Scripts.Base;
using Aki.Scripts.FSM;
using GameFramework.Fsm;
using UnityEngine;

namespace Aki.Scripts.Entities
{
    public class PlayerLogic : DefaultEntityLogic
    {
        private Camera m_Camera;

        public int MOVE_COMMANDS;

        protected IFsm<PlayerLogic> fsm;

        protected List<FsmState<PlayerLogic>> stateList;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_Camera = Camera.main;
            stateList = new List<FsmState<PlayerLogic>>();
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
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

        protected virtual void CreateFsm()
        {
            AddFsmState();
            fsm = GameEntry.Fsm.CreateFsm<PlayerLogic>(gameObject.name, this, stateList.ToArray());
            StartState();
        }
    }
}
