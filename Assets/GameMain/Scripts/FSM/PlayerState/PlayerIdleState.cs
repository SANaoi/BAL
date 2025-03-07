using Aki.Scripts.Entities;
using GameFramework;
using GameFramework.Fsm;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<Aki.Scripts.Entities.PlayerLogic>;

namespace Aki.Scripts.FSM
{
    public class PlayerIdleState : FsmState<PlayerLogic>, IReference
    {
        private PlayerLogic owner;

        private static KeyCode[] MOVE_COMMANDS = { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Debug("进入Idle状态");
            owner = procedureOwner.Owner;
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (Input.anyKeyDown)
            {
                foreach (KeyCode key in MOVE_COMMANDS)
                {
                    if (Input.GetKeyDown(key))
                    {
                        //记录这个移动指令
                        procedureOwner.SetData<VarInt32>("MoveCommand", (int)key);
                        owner.MOVE_COMMANDS = (int)key;
                        //切换到移动状态
                        ChangeState<PlayerMoveState>(procedureOwner);
                        return;
                    }
                }
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        public static PlayerIdleState Create()
        {
            PlayerIdleState state = ReferencePool.Acquire<PlayerIdleState>();
            return state;
        }

        public void Clear()
        {
            owner = null;
        }
    }
}