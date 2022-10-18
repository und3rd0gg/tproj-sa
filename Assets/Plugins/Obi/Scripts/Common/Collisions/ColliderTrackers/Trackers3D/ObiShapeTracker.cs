using UnityEngine;

namespace Obi
{
    public abstract class ObiShapeTracker
    {
        protected Component collider;
        protected ObiColliderBase source;

        public virtual void Destroy()
        {
        }

        public abstract bool UpdateIfNeeded();
    }
}