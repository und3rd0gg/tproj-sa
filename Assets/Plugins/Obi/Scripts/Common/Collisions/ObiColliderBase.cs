using System;
using UnityEngine;

namespace Obi
{
    /**
	 * Implements common functionality for ObiCollider and ObiCollider2D.
	 */
    public abstract class ObiColliderBase : MonoBehaviour
    {
        [SerializeProperty("Thickness")] [SerializeField]
        private float thickness;

        [SerializeProperty("CollisionMaterial")] [SerializeField]
        private ObiCollisionMaterial material;

        [SerializeField] private int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        protected bool dirty;
        protected ObiRigidbodyBase obiRigidbody;

        protected IntPtr oniCollider;

        protected ObiColliderHandle shapeHandle;

        protected ObiShapeTracker tracker;
        protected bool wasUnityColliderEnabled = true;

        public ObiCollisionMaterial CollisionMaterial
        {
            set => material = value;
            get => material;
        }

        public int Filter
        {
            set
            {
                if (filter != value)
                {
                    filter = value;
                    dirty = true;
                }
            }
            get => filter;
        }

        public float Thickness
        {
            set
            {
                if (!Mathf.Approximately(thickness, value))
                {
                    thickness = value;
                    dirty = true;
                }
            }
            get => thickness;
        }

        public ObiShapeTracker Tracker => tracker;

        public ObiColliderHandle Handle
        {
            get
            {
                if (shapeHandle == null)
                    FindSourceCollider();
                return shapeHandle;
            }
        }

        public IntPtr OniCollider
        {
            get
            {
                if (oniCollider == IntPtr.Zero)
                    FindSourceCollider();

                return oniCollider;
            }
        }

        public ObiRigidbodyBase Rigidbody => obiRigidbody;

        private void OnEnable()
        {
            // Initialize using the source collider specified by the user (or find an appropiate one).
            FindSourceCollider();
        }

        private void OnDisable()
        {
            RemoveCollider();
        }

        private void OnTransformParentChanged()
        {
            CreateRigidbody();
        }

        /**
         * < tracker object used to determine when to update the
         *     collider's shape
         *  Creates an OniColliderTracker of the appropiate type.
         */
        protected abstract void CreateTracker();

        protected abstract Component GetUnityCollider(ref bool enabled);

        protected abstract void FindSourceCollider();

        protected void CreateRigidbody()
        {
            obiRigidbody = null;

            // find the first rigidbody up our hierarchy:
            var rb = GetComponentInParent<Rigidbody>();
            var rb2D = GetComponentInParent<Rigidbody2D>();

            // if we have an rigidbody above us, see if it has a ObiRigidbody component and add one if it doesn't:
            if (rb != null)
            {
                obiRigidbody = rb.GetComponent<ObiRigidbody>();

                if (obiRigidbody == null)
                    obiRigidbody = rb.gameObject.AddComponent<ObiRigidbody>();
            }
            else if (rb2D != null)
            {
                obiRigidbody = rb2D.GetComponent<ObiRigidbody2D>();

                if (obiRigidbody == null)
                    obiRigidbody = rb2D.gameObject.AddComponent<ObiRigidbody2D>();
            }
        }

        protected void AddCollider()
        {
            var unityCollider = GetUnityCollider(ref wasUnityColliderEnabled);

            if (unityCollider != null && (shapeHandle == null || !shapeHandle.isValid))
            {
                shapeHandle = ObiColliderWorld.GetInstance().CreateCollider();
                shapeHandle.owner = this;

                // Create shape tracker:
                CreateTracker();

                // Create rigidbody if necessary, and link ourselves to it:
                CreateRigidbody();
            }
        }

        protected void RemoveCollider()
        {
            ObiColliderWorld.GetInstance().DestroyCollider(shapeHandle);

            // Destroy shape tracker:
            if (tracker != null)
            {
                tracker.Destroy();
                tracker = null;
            }
        }

        /** 
		 * Check if the collider transform or its shape have changed any relevant property, and update their Oni counterparts.
		 */
        public void UpdateIfNeeded()
        {
            var unityColliderEnabled = false;
            var unityCollider = GetUnityCollider(ref unityColliderEnabled);
            var colliderWorld = ObiColliderWorld.GetInstance();

            if (unityCollider != null)
            {
                // no need to test for changes, all we are doing is setting some variables here.
                if (tracker != null)
                    tracker.UpdateIfNeeded();
            }
            // If the unity collider is null but its handle is valid, the unity collider has been destroyed.
            else if (shapeHandle != null && shapeHandle.isValid)
            {
                RemoveCollider();
            }
        }
    }
}