using System;
using UnityEngine;

public class ImpactDetector : MonoBehaviour
{
    public Action<float> onHeavyImpact;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float relativeVelocity = collision.relativeVelocity.magnitude;
        if (relativeVelocity > 5f)
        {
            onHeavyImpact?.Invoke(relativeVelocity);
        }
    }
}
