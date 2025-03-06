using GameFramework;

namespace Aki.Scripts.Utility
{
    public static class AssetUtility
    {
        public static string GetDataTableAsset(string assetName, bool fromBytes = false)
        {
            return GameFramework.Utility.Text.Format("Assets/GameMain/DataTables/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetSceneAsset(string assetName)
        {
            return GameFramework.Utility.Text.Format("Assets/GameMain/Scenes/{0}.unity", assetName);
        }

        public static string GetSoundAsset(string assetName)
        {
            return GameFramework.Utility.Text.Format("Assets/GameMain/Sounds/{0}.wav", assetName);
        }

        public static string GetEntityAsset(string assetName)
        {
            return GameFramework.Utility.Text.Format("Assets/GameMain/Entities/{0}.prefab", assetName);
        }

        public static string GetUIFormAsset(string assetName)
        {
            return GameFramework.Utility.Text.Format("Assets/GameMain/UI/UIForms/{0}.prefab", assetName);
        }

        public static string GetSpriteAsset()
        {
            return "Assets/GameMain/CostumAssets/SpritesAsset.asset";
        }

    }
}