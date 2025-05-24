using UnityGameFramework.Runtime;

namespace Aki.Scripts.Entities
{
    public class DefaultEntityLogic : EntityLogic
    {
        protected EntityData m_userData;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_userData = userData as EntityData;
        }
    }
}