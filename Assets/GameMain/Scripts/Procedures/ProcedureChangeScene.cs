
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using Aki.Scripts.Definition.Constant;
using Aki.Scripts.Utility;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Procedures
{
    public class ProcedureChangeScene : ProcedureBase
    {
        /// <summary>
        /// 所有已加载的场景
        /// </summary>
        private string[] loadedSceneAssetNames;

        /// <summary>
        /// 拿到本身的流程拥有者引用，方便切换流程，不用再在Update轮询
        /// </summary>
        private IFsm<IProcedureManager> m_ProcedureOwner;

         protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GameEntry.Event.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GameEntry.Event.Subscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);

            // 初始化不能切换流程
            GameEntry.DataNode.SetData<VarBoolean>(Constant.ProcedureRunningData.CanChangeProcedure, false);

            m_ProcedureOwner = procedureOwner;

            // 获取所有已加载场景名称
            loadedSceneAssetNames = GameEntry.Scene.GetLoadedSceneAssetNames();

            // 加载场景
            GameEntry.Scene.LoadScene(
                AssetUtility.GetSceneAsset(
                    GameEntry.DataNode.GetData<VarString>(Constant.ProcedureRunningData.NextSceneName)), this);
        }

        private void OnLoadSceneUpdate(object sender, GameEventArgs e)
        {
            LoadSceneUpdateEventArgs ne = (LoadSceneUpdateEventArgs) e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' update, progress '{1}'.", ne.SceneAssetName, ne.Progress.ToString("P2"));
        }

        private void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs) e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Load scene '{0}' failure, error message '{1}'.", ne.SceneAssetName, ne.ErrorMessage);
        }

        private void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs) e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Info("Load scene '{0}' OK.", ne.SceneAssetName);

            foreach (string t in loadedSceneAssetNames)
            {
                GameEntry.Scene.UnloadScene(t);
            }

            switch (GameEntry.DataNode.GetData<VarString>(Constant.ProcedureRunningData.NextSceneName))
            {
                case "Menu":
                    ChangeState<ProcedureMenu>(m_ProcedureOwner);
                    break;
                case "Game":
                    // TODO 加载游戏场景
                    ChangeState<ProcedureGame>(m_ProcedureOwner);
                    break;
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            GameEntry.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GameEntry.Event.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            GameEntry.Event.Unsubscribe(LoadSceneUpdateEventArgs.EventId, OnLoadSceneUpdate);

            base.OnLeave(procedureOwner, isShutdown);
        }
    }
}