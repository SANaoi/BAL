using System.Collections.Generic;
using GameFramework.Fsm;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;
using UnityEngine.InputSystem;
using Aki.Plugins;
using Aki.Scripts.FSM;
using Aki.Scripts.Definition.AnimationName;
using Aki.Scripts.Camera;
using System.Linq;
using GameFramework.Event;



namespace Aki.Scripts.Entities
{
    public class PlayerLogic : DefaultEntityLogic
    {
        // 移动状态机变量
        public Vector2 movementInput;
        private Vector3 moveDirection = Vector3.zero;
        public Vector2 playerMoveContext;
        protected Vector3 currentTargetRotation; // 当前相机目标旋转
        protected Vector3 timeToReachTargetRotation;
        protected Vector3 dampedTargetRotationCurrentVelocity;
        protected Vector3 dampedTargetRotationPassedTime; // 相机旋转时间 阻尼
        public float targetSpeedModifier; // 目标速度
        private Vector3 moveVelocity; // 平滑速度向量

        // Entity挂载
        public Rigidbody rb;
        public Animator animator;
        public Transform CameraParent;
        protected PlayerInputAction inputActions;
        public CharacterController characterController;
        protected PlayerInputAction.PlayerActions moveActions;

        // 玩家数据
        public UnityEngine.Camera m_Camera;
        public CameraData cameraData;
        private CameraControl cameraControl;
        public PlayerData playerData;
        protected IFsm<PlayerLogic> fsm;
        protected List<FsmState<PlayerLogic>> stateList;
        private PlayerAnimationName playerAnimationName;

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
            
            if (stateList != null && animator != null)
            {
                playerData.speedModifier = animator.GetFloat(playerAnimationName.speedParameterHash);
            }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            PlayAnimation(playerAnimationName.speedParameterHash, playerData.speedModifier);
            ReadMovementInput();
        }

        /// <summary>
        /// 继承自MonoBehaviour的FixedUpdate方法
        /// </summary>
        private void FixedUpdate()
        {
            Move();
        }

        private void LateUpdate()
        {
            // m_Camera.transform.position = new Vector3(transform.position.x, m_Camera.transform.position.y, transform.position.z); // 相机跟随
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
        # region MoveState Function
        protected void ReadMovementInput()
        {
            movementInput = moveActions.Move.ReadValue<Vector2>();
        }
        protected void Move()
        {
            if (movementInput == Vector2.zero || playerData.speedModifier == 0f) { return; }
            Vector3 movementDirection = GetMovementInputDirection(); // 获取移动输入方向

            float targetRotationYAngle = UpdateTargetRotation(movementDirection); // 更新目标旋转

            Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle); // 获取目标旋转方向

            Vector3 currentPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();// 获取玩家水平速度

            Vector3 tep = targetRotationDirection * playerData.speedModifier - currentPlayerHorizontalVelocity; // 目标速度减去当前速度
            Vector3 targetMoveDirection = new Vector3(tep.x, moveDirection.y, tep.z); // 目标移动方向
            RotateTowardsTargetRotation(); // 旋转到目标旋转

            moveDirection = Vector3.SmoothDamp(moveDirection, targetMoveDirection, ref moveVelocity, Time.deltaTime); // 平滑移动

            characterController.Move(moveDirection * Time.deltaTime); // 移动
        }

        private Vector3 GetMovementInputDirection()
        {
            return new Vector3(movementInput.x, 0f, movementInput.y);
        }

        protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
        {
            // 根据输入方向更新目标旋转。如果指定，考虑相机旋转。
            float directionAngle = GetDirectionAngle(direction);

            if (shouldConsiderCameraRotation)
            {
                directionAngle = AddCameraRotationToAngle(directionAngle);
            }

            if (directionAngle != currentTargetRotation.y)
            {
                UpdateTargetRotationData(directionAngle);
            }

            return directionAngle;
        }
        private float GetDirectionAngle(Vector3 direction)
        {
            // Mathf.Atan2: 算出夹角弧度
            // Mathf.Rad2Deg：弧度转度（范围[-pi，pi])
            // 根据输入方向计算角度（以度为单位）
            float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            if (directionAngle < 0f)
            {
                directionAngle += 360f;
            }

            return directionAngle;
        }
        private float AddCameraRotationToAngle(float angle)
        {
            // 将相机的旋转角度添加到给定的角度上，考虑到相机当前的y轴旋转。
            angle += m_Camera.transform.eulerAngles.y;

            if (angle > 360f)
            {
                angle -= 360f;
            }

            return angle;
        }
        private void UpdateTargetRotationData(float targetAngle)
        {
            //  使用给定的目标角度更新目标旋转数据。
            currentTargetRotation.y = targetAngle;

            dampedTargetRotationPassedTime.y = 0f;
        }
        protected Vector3 GetTargetRotationDirection(float targetAngle)
        {
            // 四元数和向量相乘可以表示这个向量按照这个四元数进行旋转之后得到的新的向量
            // Vector3.forward 表示世界坐标系中的正前方（0, 0, 1）。
            // 它表示将正前方向量 Vector3.forward 绕 y 轴旋转了 targetAngle 度后的方向。
            return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        protected Vector3 GetPlayerHorizontalVelocity()
        {
            // 返回玩家刚体速度的水平分量
            Vector3 playerHorizontalVelocity = characterController.velocity;

            playerHorizontalVelocity.y = 0f;

            return playerHorizontalVelocity;
        }
        private void RotateTowardsTargetRotation()
        {
            // 使用SmoothDampAngle平滑地将玩家旋转到目标旋转。
            float currentYAngle = rb.rotation.eulerAngles.y;

            if (currentYAngle == currentTargetRotation.y)
            {
                return;
            }

            float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, currentTargetRotation.y, ref dampedTargetRotationCurrentVelocity.y,
                                                        timeToReachTargetRotation.y - dampedTargetRotationPassedTime.y);
            dampedTargetRotationPassedTime.y += Time.deltaTime;

            Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

            rb.MoveRotation(targetRotation);
        }
        private Vector3 GetPalyerMoveVector()
        {
            Transform PlayerTransform = this.transform;
            Vector3 moveDirection = playerMoveContext.x * PlayerTransform.right + playerMoveContext.y * PlayerTransform.forward;
            return moveDirection.normalized;
        }

        public void ResetVelocity()
        {
            // 立即停止移动
            moveDirection = new Vector3(0f, moveDirection.y, 0f);
            // rb.velocity = Vector3.zero;       
        }
        protected virtual void AddInputActionsCallbacks()
        {
            moveActions.Move.canceled += OnMovementCanceled;
            moveActions.Move.performed += GetplayerMoveInput;
        }

        protected virtual void RemoveInputActionsCallbacks()
        {
            moveActions.Move.canceled -= OnMovementCanceled;
            moveActions.Move.performed -= GetplayerMoveInput;
        }

        protected virtual void OnMovementCanceled(InputAction.CallbackContext context)
        {
            playerMoveContext = Vector2.zero;
        }

        private void GetplayerMoveInput(InputAction.CallbackContext context)
        {
            playerMoveContext = context.ReadValue<Vector2>();
        }
        #endregion

        #region Animation Function
        public void PlayAnimation(int animationHash, float value)
        {
            animator.SetFloat(animationHash, value);
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
