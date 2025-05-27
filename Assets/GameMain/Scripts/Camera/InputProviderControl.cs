using UnityEngine;
using Cinemachine;
using Aki.Scripts.Entities;

namespace Aki.Scripts.Camera
{
    public class InputProviderControl : CinemachineInputProvider
    {
        public void DisableInputProvider()
        {
            enabled = false;
            gameObject.SetActive(false);
        }

        public void EnableInputProvider()
        {
            enabled = true;
            gameObject.SetActive(true);
        }
    }
}
