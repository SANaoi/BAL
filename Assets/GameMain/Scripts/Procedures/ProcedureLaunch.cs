using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using Aki.Scripts.DataTable;
using Aki.Scripts.Definition.Constant;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;
using System;
using Aki.Scripts.Utility;

namespace Aki.Procedures
{
    /// <summary>
    /// 启动流程,开始初始化各种资源表
    /// 此类关键在设置m_LoadedFlag字典
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
    {
        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            //设置下一场景名称
            GameEntry.DataNode.GetOrAddNode(Constant.ProcedureRunningData.NextSceneName).SetData<VarString>("Menu");
            //设置过渡界面文字
            GameEntry.DataNode.GetOrAddNode(Constant.ProcedureRunningData.TransitionalMessage).SetData<VarString>("Loading.");
            //设置不能改变流程
            GameEntry.DataNode.GetOrAddNode(Constant.ProcedureRunningData.CanChangeProcedure).SetData<VarBoolean>(false);
            m_LoadedFlag.Clear();
            PreloadResources();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            IEnumerator<bool> iter = m_LoadedFlag.Values.GetEnumerator();
            while (iter.MoveNext())
            {
                if (!iter.Current)
                {
                    Log.Info("Data table loading...");
                    return;
                }
            }
            Log.Info("All data table loaded.");
            //所有资源加载就绪，进入场景切换流程
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        /// <summary>
        /// 资源表的预读取
        /// </summary>
        private void PreloadResources()
        {
            // LoadDataTable("SkillMessages");
            // LoadDataTable("Bullect");
            // LoadDataTable("EnemyTank");
            // LoadDataTable("Entity");
            // LoadDataTable("Sound");
            // LoadDataTable("Tools");
            LoadDataTable("Scene");
            LoadDataTable("UIForm");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTableName"></param>
        private void LoadDataTable(string dataTableName)
        {
            string dataTableAssetName = AssetUtility.GetDataTableAsset(dataTableName, false);
            m_LoadedFlag.Add(dataTableAssetName, false);
            Log.Info("Start load data table '{0}'...", dataTableName);
            Log.Info("DataTableAssetName: {0}", dataTableAssetName);
            GameEntry.DataTable.LoadDataTable(dataTableName, dataTableAssetName, this);
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs) e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName, ne.DataTableAssetName, ne.ErrorMessage);
        }
        /// <summary>
        /// 如果匹配，它会将资源表名称在 m_LoadedFlag 字典中的状态设置为 true，并输出成功加载的日志信息。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs) e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.DataTableAssetName] = true;

            Log.Info("Load dictionary '{0}' OK.", ne.DataTableAssetName);
        }
    }
}