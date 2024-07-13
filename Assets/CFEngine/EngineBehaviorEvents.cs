using System;
using System.Threading.Tasks;

namespace CrystalFrost
{
    public interface IEngineBehaviorEvents
    {
        event Action Awake;
        event Action OnEnable;
        event Action Start;
        event Action FixedUpdate;
        event Action Update;
        event Action LateUpdate;
        event Action OnDisable;
        event Action OnDestroy;

        void DoAwake();
        void DoOnEnable();
        void DoStart();
        void DoFixedUpdate();
        void DoUpdate();
        void DoLateUpdate();
        void DoOnDisable();
        void DoOnDestroy();
    }

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
