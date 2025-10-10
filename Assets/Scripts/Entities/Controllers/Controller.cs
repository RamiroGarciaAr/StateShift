
using Strategies;
using UnityEngine;

namespace Entities.Controllers
{
    public abstract class Controller : MonoBehaviour, IController
    {
        public virtual IControllable Controllable { get; protected set; }

        protected virtual void Awake()
        {
            Controllable = GetComponent<IControllable>();
            if (Controllable == null)
            {
                Debug.LogError($"[{nameof(Controller)}] {name} necesita un componente que implemente IControllable.");
            }
        }
    }
}
