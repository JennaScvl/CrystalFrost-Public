using UnityEngine;

namespace CrystalFrost
{
    public class EngineBehavior : MonoBehaviour
    {
        private IEngineBehaviorEvents _events;

        void Awake()
        {
            _events = Services.GetService<IEngineBehaviorEvents>();
            _events.DoAwake();
        }

        void OnEnable()
        {
            _events.DoOnEnable();
        }

        void Start()
        {
            _events.DoStart();
        }

        void FixedUpdate()
        {
            _events.DoFixedUpdate();
        }

        void Update()
        {
            _events.DoUpdate();
        }

        void LateUpdate()
        {
            _events.DoLateUpdate();
        }

        void OnDisable()
        {
            _events.DoOnDisable();
        }

        void OnDestroy()
        {
            _events.DoOnDestroy();
        }
    }
}
