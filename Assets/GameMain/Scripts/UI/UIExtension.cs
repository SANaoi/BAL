using GameFramework.DataTable;
using Aki.DataTable;
using Aki.Utility;
using UnityGameFramework.Runtime;
using GameEntry = Aki.GameEntry;

namespace Aki.UI
{
    public static class UIExtension
    {
        public static int? OpenUIForm(this UIComponent uiComponent, int uiFormId, object userData = null)
        {
            IDataTable<DRUIForm> dtUIForm = GameEntry.DataTable.GetDataTable<DRUIForm>();
            DRUIForm drUIForm = dtUIForm.GetDataRow(uiFormId);
            string assetName = AssetUtility.GetUIFormAsset(drUIForm.AssetName);
            return uiComponent.OpenUIForm(assetName, drUIForm.GroupName,
                drUIForm.PauseCoveredUIForm, userData);
        }

        public static int? OpenUIForm(this UIComponent uiComponent, UIFormId uiFormId, object userData = null)
        {
            return uiComponent.OpenUIForm((int) uiFormId, userData);
        }


        public static void OpenDialog(this UIComponent uiComponent, DialogParams dialogParams)
        {
            uiComponent.OpenUIForm(UIFormId.DialogForm, dialogParams);
        }
    }

}