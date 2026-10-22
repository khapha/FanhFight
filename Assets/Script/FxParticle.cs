using UnityEngine;

// Particle callbacks return completed emitters to the shared, bounded pool.
public class FxParticle : MonoBehaviour
{
    public Fx Owner;
    public ParticleSystem System;
    public bool InUse;

    void OnParticleSystemStopped()
    {
        if (Owner != null && InUse) Owner.ReturnParticle(this);
    }
}
