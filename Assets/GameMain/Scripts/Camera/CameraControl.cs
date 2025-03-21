using UnityEngine;
using Cinemachine;
using Aki.Scripts.Entities;

namespace Aki.Scripts.Camera
{
    public class CameraControl : MonoBehaviour
    {
        private CameraData cameraData;
        public Transform parentCamera;
        public Transform target;
        private CinemachineVirtualCamera virtualCamera;

        private void Awake()
        {
            
        }
    }
}
