using Aki.Scripts.GameArgs;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Procedures
{
    public class ProcedureGame : ProcedureBase
    {
        
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.Event.Fire(this, ReferencePool.Acquire<LoadNextResourcesSuccessArgs>().Fill(true));

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