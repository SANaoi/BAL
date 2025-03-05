using System.Collections;
using System.Collections.Generic;
using Aki.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Aki.AI
{
    public class ChatPrefab : UGuiForm
    {
        // Start is called before the first frame update
        [SerializeField]private Text m_Text;

        public void SetText(string _msg){
            m_Text.text=_msg;
        }
    }
}
