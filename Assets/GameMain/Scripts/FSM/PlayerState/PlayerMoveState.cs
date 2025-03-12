using GameFramework;
using GameFramework.Fsm;
using Aki.Scripts.Entities;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<Aki.Scripts.Entities.PlayerLogic>;
using UnityEngine;

namespace Aki.Scripts.FSM
{
    public class PlayerMoveState : FsmState<PlayerLogic>, IReference
    {
        private PlayerLogic owner;

        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
        }
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            owner = procedureOwner.Owner;
            Log.Debug("进入Move状态");
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (owner.playerMoveContext.y < 0)
            {
                owner.targetSpeedModifier = -2f;
            }
            else if (owner.playerMoveContext.y >= 0)
            {
                owner.targetSpeedModifier = 2f;
            }
            else
            {
                owner.targetSpeedModifier = 5f;
            }
            Log.Debug(owner.targetSpeedModifier);
            owner.playerData.speedModifier = Mathf.Lerp(owner.playerData.speedModifier, owner.targetSpeedModifier, 3 * Time.deltaTime);

            //切换回空闲状态
            if (owner.playerMoveContext == Vector2.zero)
            {
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