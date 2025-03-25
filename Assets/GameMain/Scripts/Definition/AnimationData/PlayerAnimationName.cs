using System;
using UnityEngine;

namespace Aki.Scripts.Definition.AnimationName
{
    public class PlayerAnimationName
    {
        [Header("Animation参数命名")]
        [SerializeField] private string SpeedParameterName = "Speed";
        [SerializeField] private string IsRunningParameterName = "isRunning";
        [SerializeField] private string PlayerHorizontalVelocity = "x";
        [SerializeField] private string PlayerVerticalVelocity = "y";

        public int isRunningParameterHash { get; private set; }
        public int playerHorizontalVelocityHash { get; private set; }
        public int playerVerticalVelocityHash { get; private set; }

        public void InitializeData()
        {
            isRunningParameterHash = Animator.StringToHash(IsRunningParameterName);
            playerHorizontalVelocityHash = Animator.StringToHash(PlayerHorizontalVelocity);
            playerVerticalVelocityHash = Animator.StringToHash(PlayerVerticalVelocity);
        }
    }
}