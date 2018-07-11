/*
 *	Battle Delts
 *	AnimatorWrapper.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleDelts 
{
	public class AnimatorWrapper : MonoBehaviour
	{
        [SerializeField] Animator Animator;
        
        bool QueuedAnimationCompleted = true;
        
        public void OnAnimationComplete()
        {
            QueuedAnimationCompleted = true;
        }

        public IEnumerator TriggerAndWait(string key)
        {
            while (!QueuedAnimationCompleted)
            {
                yield return null;
            }

            QueuedAnimationCompleted = false;

            Animator.SetTrigger(key);

            while (!QueuedAnimationCompleted)
            {
                yield return null;
            }
        }

        public IEnumerator TriggerAndWait(string key, int value)
        {
            while (!QueuedAnimationCompleted)
            {
                yield return null;
            }

            QueuedAnimationCompleted = false;

            Animator.SetInteger(key, value);

            while (!QueuedAnimationCompleted)
            {
                yield return null;
            }
        }

        public void SetIdleAsync()
        {
            Animator.SetTrigger("Idle");
        }
	}
}