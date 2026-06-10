using UnityEngine;

[System.Serializable] // So we can see the class in the inspector
public class SpringVector3
{
    Vector3 currentValue; // The current value of the spring
    Vector3 currentVelocity; // The current velocity of the spring, used for the spring calculation
    Vector3 targetValue; // The target value the spring is trying to reach

    [Tooltip("How stiff the spring is, higher values will make it more responsive")]
    [SerializeField]
    float stiffness = 100f;

    [Tooltip(
        "How much damping the spring has, higher values will make it settle faster but can also make it feel less responsive"
    )]
    [SerializeField]
    float damping = 10f;

    public void SetConstants(float stiffness, float damping)
    {
        this.stiffness = stiffness;
        this.damping = damping;
    }

    public void Update(float deltaTime)
    {
        Vector3 acceleration =
            -stiffness * (currentValue - targetValue) - damping * currentVelocity;
        currentVelocity += acceleration * deltaTime;
        currentValue += currentVelocity * deltaTime;
    }

    public void AddImpulse(Vector3 impulse) => currentVelocity += impulse; // Add an impulse to the current velocity

    public void SetTarget(Vector3 target) => targetValue = target; // Set the target value for the spring

    public Vector3 Value => currentValue;
}
