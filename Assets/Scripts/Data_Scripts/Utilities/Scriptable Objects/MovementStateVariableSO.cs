using Core;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewMovementStateVariable",
    menuName = "State Shift/Variables/Movement State Variable"
)]
public class MovementStateVariableSO : ScriptableObject
{
    public MovementState Value;
}
