﻿using System.Collections;
using Common;
using UnityEngine;

namespace StateMachine
{
    /// <summary>
    /// This class enables easy transitions between multiple windows of the new UI System. 
    /// Uses the Animator component to animate transitions. There must be an "Open" and "Closed"
    /// animation state. For performance reasons it is recommended to use TransitionAlphaBlend
    /// whenever possible.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class TransitionAnimator : StateListener
    {
        [SerializeField] private bool moveToFront = true;

        private Animator animator;

        //Hash of the parameter we use to control the transitions.
        private int m_OpenParameterId;
 
        //Animator State and Transition names we need to check against.
        const string k_OpenTransitionName = "Open";
        const string k_ClosedStateName = "Closed";

        private void Awake()
        {
            animator = GetComponent<Animator>();
            m_OpenParameterId = Animator.StringToHash(k_OpenTransitionName);
        }
        
        public void Open()
        {
            Activated();
        }

        public void Close()
        {
            Deactivated();
        }
        
        /// <summary>
        /// Toggle (open / close) this window.
        /// </summary>
        /// <param name="animOverride">Set to false to skip animation.</param>
        public void OpenClose()
        {
            if (gameObject.activeSelf)
                Deactivated();
            else
                Activated();
        }

        /// <summary>
        /// Closes the currently open panel and opens the provided one.
        /// It also takes care of handling the navigation, setting the new Selected element.
        /// </summary>
        /// <param name="animOverride">Set to false to ignore animation</param>
        protected override void Activated()
        {
            StopAllCoroutines();
            gameObject.SetActive(true);
            animator.enabled = true;
            if (moveToFront) transform.SetAsLastSibling();
            animator.SetBool(m_OpenParameterId, true);
            
            if (debugLog) Logging.Log(this, "'" + gameObject.name + "' Opened");
        }

        /// <inheritdoc />
        /// <summary>
        /// Closes the currently open Screen. It also takes care of navigation.
        /// Reverting selection to the Selectable used before opening the current screen.
        /// </summary>
        protected override void Deactivated(bool atStart = false)
        {
            //Start Coroutine to disable the hierarchy when closing animation finishes.
            StopAllCoroutines();
            
            animator.SetBool(m_OpenParameterId, false);

            StartCoroutine(DisablePanelDelayed());
        }

        /// <summary>
        /// Coroutine that will detect when the Closing animation is finished and it will
        /// deactivate the hierarchy.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisablePanelDelayed()
        {
            //Start the close animation.

            var closedStateReached = false;
            var shouldBeClosed = true;
            while (!closedStateReached && shouldBeClosed)
            {
                if (!animator.IsInTransition(0))
                    closedStateReached = animator.GetCurrentAnimatorStateInfo(0).IsName(k_ClosedStateName);

                shouldBeClosed = !animator.GetBool(m_OpenParameterId);

                yield return new WaitForEndOfFrame();
            }

            if (!shouldBeClosed) yield break;
            
            if (debugLog) Logging.Log(this, "'" + gameObject.name + "' Closed");

            gameObject.SetActive(false);
            animator.enabled = false;
        }
    }
}