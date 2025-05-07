namespace Aki.Scripts.Definition.Constant
{
    public  static partial class Constant
    {
        /// <summary>
        /// 流程运行时的数据
        /// </summary>
        public static class ProcedureRunningData
        {
            /// <summary>
            /// 需要加载的下一场景名称
            /// </summary>
            public const string NextSceneName = "NextSceneName";
            /// <summary>
            /// 当前关卡
            /// </summary>
            public const string CurrentLevel = "CurrentLevel";
            /// <summary>
            /// 是否可以改变游戏流程
            /// </summary>
            public const string CanChangeProcedure = "CanChangeProcedure";
            /// <summary>
            /// 过渡界面文字提示
            /// </summary>
            public const string TransitionalMessage = "TransitionalMessage";
            /// <summary>
            /// 交互UI管理序列号
            /// </summary>
            public const string InteractableUISerialId = "InteractableUISerialId";
        }
    }
}