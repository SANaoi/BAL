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

        public int speedParameterHash { get; private set; }
        public int aimParameterHash { get; private set; }
        public int shootParameterHash { get; set; }

        public int shootAnimationName { get; private set; }

        public void InitializeData()
        {
            speedParameterHash = Animator.StringToHash(SpeedParameterName);
            aimParameterHash = Animator.StringToHash(AimParameterName);
            shootParameterHash = Animator.StringToHash(ShootParameterName);

            shootAnimationName = Animator.StringToHash(ShootAnimationName);
        }
    }
}