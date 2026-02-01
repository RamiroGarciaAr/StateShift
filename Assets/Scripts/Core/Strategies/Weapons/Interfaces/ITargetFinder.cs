using UnityEngine;

namespace Core.Strategies.Weapons
{
    public interface ITargetFinder
    {
        bool TryFindTarget(Vector3 origin, out Vector3 target);
    }
}