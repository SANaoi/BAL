using Aki.Scripts.GameArgs;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using Aki.Scripts.Games;
using GameEntry = Aki.Scripts.Base.GameEntry;
using Aki.Scripts.Entities;

namespace Aki.Procedures
{
    public class ProcedureGame : ProcedureBase
    {
        private GameControl gameControl;
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.Event.Fire(this, ReferencePool.Acquire<LoadNextResourcesSuccessArgs>().Fill(true));

            PlayerData playerData = new PlayerData(GameEntry.Entity.GenerateSerialId(), (int)EnumEntity.Player);
            GameEntry.Entity.ShowEntity<PlayerLogic>((EntityData)playerData, typeof(PlayerLogic), null);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }
    }
}