using Aki.Scripts.Entities;
using GameFramework;
using GameFramework.Fsm;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<Aki.Scripts.Entities.PlayerLogic>;

namespace Aki.Scripts.FSM
{
    public class PlayerMoveState : FsmState<PlayerLogic>, IReference
    {
        private PlayerLogic owner;

        private KeyCode moveCommand;
        private static readonly float EXIT_TIME = 1f;
        private float exitTimer;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            owner = procedureOwner.Owner;
            //进入移动状态时，获取移动指令数据
            moveCommand = (KeyCode)(int)procedureOwner.GetData<VarInt>("MoveCommand");
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            //计时器累计时间
            exitTimer += elapseSeconds;
            //达到指定时间后
            if (exitTimer > EXIT_TIME)
            {
                //切换回空闲状态
                ChangeState<PlayerIdleState>(procedureOwner);
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }
        public static PlayerMoveState Create()
        {
            PlayerMoveState state = ReferencePool.Acquire<PlayerMoveState>();
            return state;
        }

        public void Clear()
        {
            owner = null;
        }
    }
}