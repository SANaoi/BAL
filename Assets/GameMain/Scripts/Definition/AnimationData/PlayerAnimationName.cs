using System;
using UnityEngine;

namespace Aki.Scripts.Definition.AnimationName
{
    public class PlayerAnimationName
    {
        [Header("Animation参数命名")]
        [SerializeField] private string SpeedParameterName = "Speed";
        [SerializeField] private string AimParameterName = "Aim";
        [SerializeField] private string ShootParameterName = "Shoot";
        [SerializeField] private string ShootAnimationName = "Attack";
        [SerializeField] private string PlayerHorizontalVelocity = "x";
        [SerializeField] private string PlayerVerticalVelocity = "y";

        public int speedParameterHash { get; private set; }
        public int aimParameterHash { get; private set; }
        public int shootParameterHash { get; set; }
        public int shootAnimationName { get; private set; }
        public int playerHorizontalVelocityHash { get; private set; }
        public int playerVerticalVelocityHash { get; private set; }

        public void InitializeData()
        {
            speedParameterHash = Animator.StringToHash(SpeedParameterName);
            aimParameterHash = Animator.StringToHash(AimParameterName);
            shootParameterHash = Animator.StringToHash(ShootParameterName);

            shootAnimationName = Animator.StringToHash(ShootAnimationName);
            
            playerHorizontalVelocityHash = Animator.StringToHash(PlayerHorizontalVelocity);
            playerVerticalVelocityHash = Animator.StringToHash(PlayerVerticalVelocity);
        }
    }
}