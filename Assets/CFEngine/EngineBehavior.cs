using UnityEngine;

namespace CrystalFrost
{
    /// <summary>
    /// A MonoBehaviour that hooks into Unity's lifecycle events and forwards them to the <see cref="IEngineBehaviorEvents"/> service.
    /// This allows other parts of the application to subscribe to Unity's lifecycle events without needing to be a MonoBehaviour.
    /// </summary>
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
