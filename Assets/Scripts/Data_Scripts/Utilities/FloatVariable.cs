using UnityEngine;

//reference: https://www.youtube.com/watch?v=raQ3iHhE_Kk&t=2s|

//TODO: we can expand this later to be more private and have getter/setter functions, but for now this is fine. We can also add events to it so that we can have things subscribe to it and react when the value changes.
[CreateAssetMenu]
public class FloatVariable : ScriptableObject
{
    public float Value;
}
