using UnityEngine;

public interface IPlatformRider
{
    void OnPlatformEnter(Transform platform);
    void OnPlatformExit(Transform platform);
    void OnPlatformMove(Vector3 deltaPosition, Quaternion deltaRotation);
}
