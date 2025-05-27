using UnityEngine;
using Cinemachine;
using Aki.Scripts.Entities;

namespace Aki.Scripts.Camera
{
    public class CameraControl : DefaultEntityLogic
    {
        public Transform target;

        CameraData cameraData;
        public UnityEngine.Camera m_Camera;
        public CinemachineVirtualCamera virtualCamera;
        CinemachineFramingTransposer framingTransposer;
        public InputProviderControl inputProvider;
        float targetDistance;

        [SerializeField][Range(0f, 10f)] private float defaultDistance = 2f;
        [SerializeField][Range(0f, 10f)] private float minimumDistance = 1f;
        [SerializeField][Range(0f, 10f)] private float maximumDistance = 4f;
        [SerializeField][Range(0f, 10f)] private float smoothing = 4f;
        [SerializeField][Range(0f, 10f)] private float zoomSensitivity = 0.5f;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            cameraData = userData as CameraData;
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            framingTransposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
            inputProvider = GetComponent<InputProviderControl>();

            targetDistance = defaultDistance;
            m_Camera = UnityEngine.Camera.main;
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            ScrollWheel();
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
            target.position = cameraData.DefaultLocalPosition;
        }

        public void OnInteract(Transform target)
        {
            inputProvider.enabled = false;
            virtualCamera.enabled = false;
            m_Camera.transform.position = target.position + new Vector3(0, 0, 0.7f);
            m_Camera.transform.LookAt(target.position);
        }

        void ScrollWheel()
        {
            var scrollValue = inputProvider.GetAxisValue(2) * zoomSensitivity;
            float currentDistance = framingTransposer.m_CameraDistance;

            targetDistance = Mathf.Clamp(targetDistance + scrollValue, minimumDistance, maximumDistance);

            if (currentDistance == targetDistance)
            {
                return;
            }

            float lerpedZoomValue = Mathf.Lerp(currentDistance, targetDistance, smoothing * Time.deltaTime);
    
            framingTransposer.m_CameraDistance = lerpedZoomValue;
        }
    }
}
