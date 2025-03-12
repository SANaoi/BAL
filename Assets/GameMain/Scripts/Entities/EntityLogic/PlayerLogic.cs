using System.Collections.Generic;
using Aki.Scripts.FSM;
using GameFramework.Fsm;
using UnityEngine;
using Aki.Plugins;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;
using System.Net.Security;
using System;
using UnityEngine.InputSystem;


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
        public CharacterController characterController;
        public Rigidbody rb;
        protected PlayerInputAction inputActions;
        protected PlayerInputAction.PlayerActions moveActions;

        // 玩家数据
        private Camera m_Camera;
        public PlayerData playerData;
        protected IFsm<PlayerLogic> fsm;
        protected List<FsmState<PlayerLogic>> stateList;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_Camera = Camera.main;
            playerData = userData as PlayerData;
            stateList = new List<FsmState<PlayerLogic>>();
            inputActions = new PlayerInputAction();
            moveActions = inputActions.Player;

            characterController = GetComponent<CharacterController>();
            rb = GetComponent<Rigidbody>();
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            if (playerData == null)
            {
                Log.Error("Player data is invalid.");
                return;
            }
            CreateFsm();

            inputActions.Enable();
            AddInputActionsCallbacks(); // 添加输入回调
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            
            ReadMovementInput();
        }

        /// <summary>
        /// 继承自MonoBehaviour的FixedUpdate方法
        /// </summary>
        private void FixedUpdate()
        {
            Move();
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

        protected virtual void AddFsmState()
        {
            stateList.Add(PlayerIdleState.Create());
            stateList.Add(PlayerMoveState.Create());
        }

        protected virtual void StartState()
        {
            fsm.Start<PlayerIdleState>();
        }

        /// <summary>
        /// 创建状态机
        /// </summary>
        protected virtual void CreateFsm()
        {
            AddFsmState();
            fsm = GameEntry.Fsm.CreateFsm<PlayerLogic>(gameObject.name, this, stateList.ToArray());
            StartState();
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
    }
}
