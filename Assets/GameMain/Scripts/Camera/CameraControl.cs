using UnityEngine;
using Cinemachine;
using Aki.Scripts.Entities;
using Unity.IO.LowLevel.Unsafe;

namespace Aki.Scripts.Camera
{
    public class CameraControl : DefaultEntityLogic
    {
        private CameraData cameraData;
        public Transform target;
        private CinemachineVirtualCamera virtualCamera;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            cameraData      = userData as CameraData;
            virtualCamera   = GetComponent<CinemachineVirtualCamera>();
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
            target.position = cameraData.DefaultLocalPosition;

        }
    }
}
