using Managers;
using UnityEngine;

namespace Core.Strategies.Weapons
{
    public class PlayerTargetFinder : MonoBehaviour, ITargetFinder
    {
        [SerializeField] private float _radius = 5f;

        public bool TryFindTarget(Vector3 origin, out Vector3 target)
        {
            Transform player = GameManager.Player.transform;

            if (Physics.Raycast(origin, player.position - origin, out RaycastHit hitInfo))
            {
                if (hitInfo.transform == player && hitInfo.distance <= _radius)
                {
                    target = player.position;
                    return true;
                }
            }

            target = default;
            return false;
        }
    }
}