using UnityEngine;
using UnityGameFramework.Runtime;
using UnityEngine.InputSystem;

using System.Collections.Generic;

using GameFramework.Event;
using GameFramework.Fsm;

using Aki.Plugins;
using Aki.Scripts.FSM;
using Aki.Scripts.Definition.AnimationName;
using Aki.Scripts.Camera;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.Entities
{
    public class PlayerLogic : DefaultEntityLogic
    {
        // 输入值
        public bool isRunning = false; 
        public Vector2 playerMoveInput;

        // Entity挂载
        public Rigidbody rb;
        public Animator animator;
        public Transform CameraParent;
        protected PlayerInputAction inputActions;
        public CharacterController characterController;
        protected PlayerInputAction.PlayerActions moveActions;

        // 玩家数据
        public UnityEngine.Camera m_Camera;
        private CameraControl cameraControl;
        public CameraData cameraData;
        public PlayerData playerData;
        protected IFsm<PlayerLogic> fsm;
        protected List<FsmState<PlayerLogic>> stateList;
        public PlayerAnimationName playerAnimationName;
        public Vector3 playerMovement = Vector3.zero;
        public float currentSpeed = 0;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_Camera            = UnityEngine.Camera.main;
            playerData          = userData as PlayerData;

            playerAnimationName = new PlayerAnimationName();
            stateList           = new List<FsmState<PlayerLogic>>();
            inputActions        = new PlayerInputAction();
            moveActions         = inputActions.Player;

            animator            = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            rb                  = GetComponent<Rigidbody>();
        }
        
        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            if (playerData == null)
            {
                Log.Error("Player data is invalid.");
                return;
            }
            ShowVirtualCamera();
            CreateFsm();

            inputActions.Enable();
            playerAnimationName.InitializeData();
            AddInputActionsCallbacks(); // 添加输入回调
            
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            CaculateInputDirection();
            SetUpAnimator();
        }

        /// <summary>
        /// 继承自MonoBehaviour的FixedUpdate方法
        /// </summary>
        private void FixedUpdate()
        {
        }

        private void LateUpdate()
        {
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            base.OnHide(isShutdown, userData);
            inputActions.Disable();
            RemoveInputActionsCallbacks(); // 移除输入回调
        }

        private void OnDestroy()
        {
            GameEntry.Fsm.DestroyFsm(fsm);
        }

        protected void AddFsmState()
        {
            stateList.Add(PlayerIdleState.Create());
            stateList.Add(PlayerMoveState.Create());
            stateList.Add(PlayerRunState.Create());
        }

        protected void StartState()
        {
            fsm.Start<PlayerIdleState>();
        }

        /// <summary>
        /// 创建状态机
        /// </summary>
        protected void CreateFsm()
        {
            AddFsmState();
            fsm = GameEntry.Fsm.CreateFsm<PlayerLogic>(gameObject.name, this, stateList.ToArray());
            StartState();
        }
        /// <summary>
        /// 显示虚拟相机成功
        /// </summary>
        private void OnShowVirtualCameraSuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs) e;
            if (ne.EntityLogicType != typeof(CameraControl))
            {
                return;
            }
            // GameEntry.Entity.AttachEntity(GameEntry.Entity.GetEntity(cameraData.entityId), this.Entity, CameraParent, cameraData);
            cameraControl = GameEntry.Entity.GetEntity(cameraData.entityId).Logic as CameraControl;
            cameraControl.SetTarget(CameraParent);
        }

        private void ShowVirtualCamera()
        {
            GameEntry.Event.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowVirtualCameraSuccess);
            CameraParent = GameObject.Find("CameraRoot").transform;
            cameraData = new CameraData(GameEntry.Entity.GenerateSerialId(), (int)EnumCamera.CameraPlayer);
            
            GameEntry.Entity.ShowEntity(cameraData, typeof(CameraControl));
        }

        // private void OnAnimatorMove()
        // {
        //     transform.position += animator.deltaPosition;
        //     transform.rotation *= animator.deltaRotation;
        // }
        #region Move Function
        
        /// <summary>
        /// 计算玩家输入相对相机的方向
        /// </summary>
        void CaculateInputDirection()
        {
            Vector3 camForwardProjection = new Vector3(m_Camera.transform.forward.x, 0, m_Camera.transform.forward.z).normalized;
            playerMovement = camForwardProjection * playerMoveInput.y + m_Camera.transform.right * playerMoveInput.x; 
            playerMovement = transform.InverseTransformDirection(playerMovement);
        }

        void SetUpAnimator()
        {
            if (isRunning)
            {
                StartAnimation(playerAnimationName.isRunningParameterHash);
            }
            else
            {
                StopAnimation(playerAnimationName.isRunningParameterHash);
            }

            float rad = Mathf.Atan2(playerMovement.x, playerMovement.z);
            PlayAnimation(playerAnimationName.playerVerticalVelocityHash, rad);
            transform.Rotate(0, rad * 400 * Time.deltaTime, 0f);
        }
        
        #endregion

        # region Input Function
        protected virtual void AddInputActionsCallbacks()
        {
            moveActions.Move.canceled += OnMovementCanceled;
            moveActions.Move.performed += GetplayerMoveInput;
            moveActions.Run.performed += GetRunInput;
        }

        protected virtual void RemoveInputActionsCallbacks()
        {
            moveActions.Move.canceled -= OnMovementCanceled;
            moveActions.Move.performed -= GetplayerMoveInput;
            moveActions.Run.performed -= GetRunInput;
        }

        void GetplayerMoveInput(InputAction.CallbackContext context)
        {
            playerMoveInput = context.ReadValue<Vector2>();
        }
        void OnMovementCanceled(InputAction.CallbackContext context)
        {
            playerMoveInput = Vector2.zero;
        }
        
        void GetRunInput(InputAction.CallbackContext ctx)
        {
            isRunning = !isRunning;
        }

        #endregion

        #region Animation Function
        public void PlayAnimation(int animationHash, float value, float dampvale = 0.1f)
        {
            animator.SetFloat(animationHash, value, dampvale, Time.deltaTime);
        }

        public void PlayAnimation(int animationHash)
        {
            animator.Play(animationHash);
        }

        public void StartAnimation(int animationHash)
        {
            animator.SetBool(animationHash, true);
        }

        public void StopAnimation(int animationHash)
        {
            animator.SetBool(animationHash, false);
        }

        # endregion
    }
}
