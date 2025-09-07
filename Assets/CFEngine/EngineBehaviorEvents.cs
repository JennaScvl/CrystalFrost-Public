using System;
using System.Threading.Tasks;

namespace CrystalFrost
{
    /// <summary>
    /// Defines events corresponding to Unity's MonoBehaviour lifecycle methods.
    /// </summary>
    public interface IEngineBehaviorEvents
    {
        /// <summary>
        /// Occurs when the script instance is being loaded.
        /// </summary>
        event Action Awake;
        /// <summary>
        /// Occurs when the object becomes enabled and active.
        /// </summary>
        event Action OnEnable;
        /// <summary>
        /// Occurs on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        event Action Start;
        /// <summary>
        /// Occurs every fixed framerate frame.
        /// </summary>
        event Action FixedUpdate;
        /// <summary>
        /// Occurs every frame.
        /// </summary>
        event Action Update;
        /// <summary>
        /// Occurs every frame, after all Update functions have been called.
        /// </summary>
        event Action LateUpdate;
        /// <summary>
        /// Occurs when the behaviour becomes disabled or inactive.
        /// </summary>
        event Action OnDisable;
        /// <summary>
        /// Occurs when the MonoBehaviour will be destroyed.
        /// </summary>
        event Action OnDestroy;

        /// <summary>
        /// Invokes the Awake event.
        /// </summary>
        void DoAwake();
        /// <summary>
        /// Invokes the OnEnable event.
        /// </summary>
        void DoOnEnable();
        /// <summary>
        /// Invokes the Start event.
        /// </summary>
        void DoStart();
        /// <summary>
        /// Invokes the FixedUpdate event.
        /// </summary>
        void DoFixedUpdate();
        /// <summary>
        /// Invokes the Update event.
        /// </summary>
        void DoUpdate();
        /// <summary>
        /// Invokes the LateUpdate event.
        /// </summary>
        void DoLateUpdate();
        /// <summary>
        /// Invokes the OnDisable event.
        /// </summary>
        void DoOnDisable();
        /// <summary>
        /// Invokes the OnDestroy event.
        /// </summary>
        void DoOnDestroy();
    }

    /// <summary>
    /// Implements events corresponding to Unity's MonoBehaviour lifecycle methods,
    /// allowing other parts of the application to subscribe to these events.
    /// </summary>
    public class EngineBehaviorEvents : IEngineBehaviorEvents
    {
        public event Action Awake;
        public event Action OnEnable;
        public event Action Start;
        public event Action FixedUpdate;
        public event Action Update;
        public event Action LateUpdate;
        public event Action OnDisable;
        public event Action OnDestroy;

        private static void DoInBackground(Action action)
        {
            // capture the current event handler
            // in case it changes on another thread
            // while we are using it.
            var a = action;
            if (a is null) return;
            _ = Task.Run(() => a?.Invoke());
        }

        public void DoAwake() => DoInBackground(Awake);
        public void DoOnDestroy() => DoInBackground(OnDestroy);
        public void DoOnDisable() => DoInBackground(OnDisable);
        public void DoOnEnable() => DoInBackground(OnEnable);
        public void DoFixedUpdate() => DoInBackground(FixedUpdate);
        public void DoLateUpdate() => DoInBackground(LateUpdate);
        public void DoStart() => DoInBackground(Start);
        public void DoUpdate() => DoInBackground(Update);
    }
}
