using GameFramework.DataTable;
using Aki.Scripts.DataTable;
using Aki.Scripts.Utility;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.UI
{
    public static class UIExtension
    {
        public static int? OpenUIForm(this UIComponent uiComponent, int uiFormId, object userData = null)
        {
            IDataTable<DRUIForm> dtUIForm = GameEntry.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            string assetName = AssetUtility.GetUIFormAsset(drUIForm.AssetName);
            return uiComponent.OpenUIForm(assetName, drUIForm.UIGroupName,
                drUIForm.PauseCoveredUIForm, userData);
        }

        public static int? OpenUIForm(this UIComponent uiComponent, EnumUIForm uiFormId, object userData = null)
        {
            return uiComponent.OpenUIForm((int) uiFormId, userData);
        }


        public static void OpenDialog(this UIComponent uiComponent, DialogParams dialogParams)
        {
            uiComponent.OpenUIForm(EnumUIForm.DialogForm, dialogParams);
        }
    }

}