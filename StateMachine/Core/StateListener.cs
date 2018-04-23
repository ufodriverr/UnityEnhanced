﻿using System.Collections.Generic;
using System.Linq;
using UE.Common;
using UE.Instancing;
using UnityEngine;
using UnityEngine.Events;

namespace UE.StateMachine
{
    /// <summary>
    /// This component sends events when the given states are activated. Can be inherited for special applications.
    /// </summary>
    public class StateListener : InstanceObserver
    {
        [SerializeField] 
        protected bool debug;
        [SerializeField] 
        private List<State> activeStates = new List<State>(){null};

        [SerializeField]
        protected UnityEvent OnActivated;
        [SerializeField]
        protected UnityEvent OnDeactivated;

        /// <summary>
        /// Returns true if this is currently activated.
        /// </summary>
        public bool Active { get; private set; }

        protected virtual void Start()
        {
            //check if there are any states defined
            if (!HasStates())
            {
                enabled = false;
                Logging.Warning(this, "'" + gameObject.name + "': There are no active states defined!");
                return;
            }

            var stateManager = activeStates[0].stateManager;

            //check if all states are in the same state system
            foreach (var state in activeStates)
            {
                if(state.stateManager == stateManager) continue;

                enabled = false;
                Logging.Warning(this, "'" + gameObject.name + "': Not all states belong to the same state machine!");
                return;
            }
            
            stateManager.Init(key);
            
            stateManager.AddStateEnterListener(OnStateEnter, key);

            if (IsActiveState(stateManager.GetState(key)))
            {
                Logging.Log(this, "'" + gameObject.name + "' Activated", debug);
                Active = true;
                Activated();
                OnActivated.Invoke();
            }
            else
            {
                Logging.Log(this, "'" + gameObject.name + "' Deactivated", debug);
                OnDeactivated.Invoke();
                Deactivated(true);
            }
        }

        /// <summary>
        /// Returns true if the given state is one of the active states.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        protected bool IsActiveState(State state)
        {
            return activeStates.Any(s => s == state);
        }

        /// <summary>
        /// Activates the single player window as soon as the state is set.
        /// </summary>
        /// <param name="state"></param>
        private void OnStateEnter(State state)
        {
            var previouslyActive = Active;
            Active = IsActiveState(state);

            if (!previouslyActive && Active)
            {
                Logging.Log(this, "'" + gameObject.name + "' Activated", debug);
                Activated();
                OnActivated.Invoke();
            }
            if (previouslyActive && !Active)
            {
                Logging.Log(this, "'" + gameObject.name + "' Deactivated", debug);
                OnDeactivated.Invoke();
                Deactivated();
            }
        }
        
        /// <summary>
        /// This is called at start and when one of the active states is entered and the previous state was not one
        /// of the active states. This can be overridden by sub classes to induce custom behaviour when the current
        /// state is activated.
        /// </summary>
        protected virtual void Activated()
        {   
        }
        
        /// <summary>
        /// This is called at start and when one of the active states is left and the next state is not one
        /// of the active states. This can be overridden by sub classes to induce custom behaviour when the current
        /// state is deactivated.
        /// </summary>
        /// <param name="atStart">This is true for immediate disabling at start.</param>
        protected virtual void Deactivated(bool atStart = false)
        {
        }
        
        protected virtual void OnDestroy()
        {
            if (!HasStates()) return;

            Logging.Log(this, "'" + gameObject.name + "' Removing Listener", debug);
            
//            activeStates[0].stateManager.OnStateEnter.RemoveListener(OnStateEnter);
            activeStates[0].stateManager.RemoveStateEnterListener(OnStateEnter, key);
        }

        public override IInstanciable GetTarget()
        {
            if (!activeStates.Any()) return null;
            
            if (activeStates[0] == null) return null;
            
            return activeStates[0].stateManager;
        }

        /// <summary>
        /// Returns true if there are states defined and the first state is not null.
        /// </summary>
        /// <returns></returns>
        protected bool HasStates()
        {
            return activeStates.Any() && activeStates[0] != null;
        }

        protected virtual void OnDrawGizmos()
        {
            if(!debug) return;
            if (!HasStates()) return;

            activeStates[0].stateManager?.DrawWorldSpaceGizmo(transform.position, key);
        }
    }
}