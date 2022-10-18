using UnityEngine;

namespace Obi
{
    public abstract class ObiMeshBasedActorBlueprint : ObiActorBlueprint
    {
        public Mesh inputMesh;

        /**< Mesh used to generate the blueprint.*/
        public Vector3 scale = Vector3.one;

        public Quaternion rotation = Quaternion.identity;
    }
}