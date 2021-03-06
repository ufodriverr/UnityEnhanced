﻿// ----------------------------------------------------------------------------
// Based on Work from Ryan Hipple, Unite 2017 - Game Architecture with Scriptable Objects
// ----------------------------------------------------------------------------

using UE.Instancing;
using UnityEngine;
using UnityEngine.Events;

namespace UE.Events
{
    [AddComponentMenu("Unity Enhanced/Events/Game Event Listener", 0)]
    public class GameEventListener : InstanceObserver
    {
        [Tooltip("When this is checked, the listener will still work when the game object is disabled.")]
        [SerializeField]
        private bool persistent;
        
        [Tooltip("Event to register with.")] [SerializeField]
        internal GameEvent Event;
        
        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent Response;

        protected virtual void OnEnable()
        {
            if (!persistent) Event.AddListener(this, key);
        }

        protected virtual void OnDisable()
        {
            if (!persistent) Event.RemoveListener(this, key);
        }

        protected virtual void Awake()
        {
            if (persistent) Event.AddListener(this, key);
        }

        protected virtual void OnDestroy()
        {
            if (persistent) Event.RemoveListener(this, key);
        }

        /// <summary>
        /// This is called when the listened event gets raised.
        /// </summary>
        public virtual void OnEventRaised()
        {
            Response.Invoke();
        }

        public override IInstanciable GetTarget()
        {
            return Event;
        }
    }
}