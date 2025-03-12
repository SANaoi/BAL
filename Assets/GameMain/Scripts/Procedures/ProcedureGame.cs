using Aki.Scripts.GameArgs;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;
using Aki.Scripts.Games;
using GameEntry = Aki.Scripts.Base.GameEntry;
using Aki.Scripts.Definition.Constant;

namespace Aki.Procedures
{
    public class ProcedureGame : ProcedureBase
    {
        private GameControl GameControl;
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.DataNode.SetData<VarBoolean>(Constant.ProcedureRunningData.CanChangeProcedure, false);

            // 调用订阅事件
            GameEntry.Event.Fire(this, ReferencePool.Acquire<LoadNextResourcesSuccessArgs>().Fill(true));

            if (GameControl == null)
            {
                GameControl = new GameControl(this);
            }
            GameControl?.Init();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            // CanChangeProcedure 为 true 时，进入下一个流程
            if (GameEntry.DataNode.GetData<VarBoolean>(Constant.ProcedureRunningData.CanChangeProcedure))
            {
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }
    }
}