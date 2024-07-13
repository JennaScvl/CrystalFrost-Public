using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemObjectTracker : MonoBehaviour
{
    public ParticleSystem p;
    public ParticleSystem.Particle[] particles;
    public Transform Target;
    public float affectDistance;
    float sqrDist;
    Transform thisTransform;


    void Start()
    {
        p = GetComponent<ParticleSystem>();
        sqrDist = affectDistance * affectDistance;
    }


    void Update()
    {

        particles = new ParticleSystem.Particle[p.particleCount];

        p.GetParticles(particles);

        for (int i = 0; i < particles.GetUpperBound(0); i++)
        {

            float ForceToAdd = (particles[i].startLifetime - particles[i].remainingLifetime) * (10 * Vector3.Distance(Target.position, particles[i].position));

            //Debug.DrawRay (particles [i].position, (Target.position - particles [i].position).normalized * (ForceToAdd/10));

            particles[i].velocity = (Target.position - particles[i].position).normalized * ForceToAdd;

            //particles [i].position = Vector3.Lerp (particles [i].position, Target.position, Time.deltaTime / 2.0f);

        }

        p.SetParticles(particles, particles.Length);

    }
}
