﻿using System.Collections.Generic;
using Obi;
using UnityEngine;

/**
 * Sample component that makes a collider "grab" any particle it touches (regardless of which Actor it belongs to).
 */
[RequireComponent(typeof(ObiCollider))]
public class ObiContactGrabber : MonoBehaviour
{
    public ObiSolver[] solvers = { };

    private readonly Dictionary<ObiSolver, ObiSolver.ObiCollisionEventArgs> collisionEvents =
        new Dictionary<ObiSolver, ObiSolver.ObiCollisionEventArgs>();

    /**< set to store all currently grabbed particles.*/
    private readonly HashSet<ObiActor> grabbedActors = new HashSet<ObiActor>();

    /**< the collider on this gameObject.*/
    private readonly HashSet<GrabbedParticle> grabbedParticles = new HashSet<GrabbedParticle>();

    /**< store the current collision event*/
    private ObiCollider localCollider;

    public bool grabbed => grabbedActors.Count > 0;

    /**< set of softbodies grabbed during this step.*/
    private void Awake()
    {
        localCollider = GetComponent<ObiCollider>();
    }

    /**
     * Updates the position of the grabbed particles.
     */
    private void FixedUpdate()
    {
        foreach (var p in grabbedParticles)
        {
            var grabber2Solver = p.solver.transform.worldToLocalMatrix * transform.localToWorldMatrix;
            p.solver.positions[p.index] = grabber2Solver.MultiplyPoint3x4(p.localPosition);
        }
    }

    private void OnEnable()
    {
        if (solvers != null)
            foreach (var solver in solvers)
                solver.OnCollision += Solver_OnCollision;
    }

    private void OnDisable()
    {
        if (solvers != null)
            foreach (var solver in solvers)
                solver.OnCollision -= Solver_OnCollision;
    }

    private void Solver_OnCollision(object sender, ObiSolver.ObiCollisionEventArgs e)
    {
        collisionEvents[(ObiSolver) sender] = e;
    }

    private void UpdateParticleProperties()
    {
        // Update rest shape matching of all grabbed softbodies:
        foreach (var actor in grabbedActors) actor.UpdateParticleProperties();
    }

    /**
     * Creates and stores a GrabbedParticle from the particle at the given index.
     * Returns true if we sucessfully grabbed a particle, false if the particle was already grabbed.
     */
    private bool GrabParticle(ObiSolver solver, int index)
    {
        var p = new GrabbedParticle(solver, index, solver.invMasses[index]);

        // in case this particle has not been grabbed yet:
        if (!grabbedParticles.Contains(p))
        {
            var solver2Grabber = transform.worldToLocalMatrix * solver.transform.localToWorldMatrix;

            // record the particle's position relative to the grabber, and store it.
            p.localPosition = solver2Grabber.MultiplyPoint3x4(solver.positions[index]);
            grabbedParticles.Add(p);

            // Set inv mass and velocity to zero:
            solver.invMasses[index] = 0;
            solver.velocities[index] = Vector4.zero;

            return true;
        }

        return false;
    }

    /**
     * Grabs all particles currently touching the grabber.
     */
    public void Grab()
    {
        Release();

        var world = ObiColliderWorld.GetInstance();

        if (solvers != null && collisionEvents != null)
            foreach (var solver in solvers)
            {
                ObiSolver.ObiCollisionEventArgs collisionEvent;
                if (collisionEvents.TryGetValue(solver, out collisionEvent))
                    foreach (var contact in collisionEvent.contacts)
                        // this one is an actual collision:
                        if (contact.distance < 0.01f)
                        {
                            var contactCollider = world.colliderHandles[contact.bodyB].owner;
                            var particleIndex = solver.simplices[contact.bodyA];

                            // if the current contact references our collider, proceed to grab the particle.
                            if (contactCollider == localCollider)
                                // try to grab the particle, if not already grabbed.
                                if (GrabParticle(solver, particleIndex))
                                    grabbedActors.Add(solver.particleToActor[particleIndex].actor);
                        }
            }

        UpdateParticleProperties();
    }

    /**
     * Releases all currently grabbed particles. This boils down to simply resetting their invMass.
     */
    public void Release()
    {
        // Restore the inverse mass of all grabbed particles, so dynamics affect them.
        foreach (var p in grabbedParticles)
            p.solver.invMasses[p.index] = p.invMass;

        UpdateParticleProperties();
        grabbedActors.Clear();
        grabbedParticles.Clear();
    }

    /**
     * Helper class that stores the index of a particle in the solver, its position in the grabber's local space, and its inverse mass previous to being grabbed.
     * This makes it easy to tell if a particle has been grabbed, update its position while grabbing, and restore its mass after being released.
     */
    private class GrabbedParticle : IEqualityComparer<GrabbedParticle>
    {
        public readonly int index;
        public readonly float invMass;
        public Vector3 localPosition;
        public readonly ObiSolver solver;

        public GrabbedParticle(ObiSolver solver, int index, float invMass)
        {
            this.solver = solver;
            this.index = index;
            this.invMass = invMass;
        }

        public bool Equals(GrabbedParticle x, GrabbedParticle y)
        {
            return x.index == y.index;
        }

        public int GetHashCode(GrabbedParticle obj)
        {
            return index;
        }
    }
}