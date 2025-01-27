using GameFramework.Event;

namespace Aki.GameArgs
{
    /// <summary>
    /// 下一场景已初始化完毕事件
    /// </summary>
    public class LoadNextResourcesSuccessArgs : GameEventArgs
    {
        /// <summary>
        /// 事件Id
        /// </summary>
        public static readonly int EventId = typeof(LoadNextResourcesSuccessArgs).GetHashCode();
        /// <summary>
        /// 将要展现的资源已加载完毕
        /// </summary>
        public bool LoadNextResourcesSuccess { get; set; }
        public LoadNextResourcesSuccessArgs Fill(bool loadSuccessflag)
        {
            LoadNextResourcesSuccess = loadSuccessflag;
            return this;
        }

        public override void Clear()
        {
            LoadNextResourcesSuccess = false;
        }

        public override int Id => EventId;
    }
}