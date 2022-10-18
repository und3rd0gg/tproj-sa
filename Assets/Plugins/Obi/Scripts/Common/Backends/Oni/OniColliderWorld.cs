#if (OBI_ONI_SUPPORTED)
using UnityEngine;

namespace Obi
{
    public class OniColliderWorld : MonoBehaviour, IColliderWorldImpl
    {
        public void Awake()
        {
            ObiColliderWorld.GetInstance().RegisterImplementation(this);
        }

        public void OnDestroy()
        {
            ObiColliderWorld.GetInstance().UnregisterImplementation(this);
        }

        public int referenceCount { get; private set; }

        public void UpdateWorld(float deltaTime)
        {
            Oni.UpdateColliderGrid(deltaTime);
        }

        public void SetColliders(ObiNativeColliderShapeList shapes, ObiNativeAabbList bounds,
            ObiNativeAffineTransformList transforms, int count)
        {
            Oni.SetColliders(shapes.GetIntPtr(), bounds.GetIntPtr(), transforms.GetIntPtr(), count);
        }

        public void SetRigidbodies(ObiNativeRigidbodyList rigidbody)
        {
            Oni.SetRigidbodies(rigidbody.GetIntPtr());
        }

        public void SetCollisionMaterials(ObiNativeCollisionMaterialList materials)
        {
            Oni.SetCollisionMaterials(materials.GetIntPtr());
        }

        public void SetTriangleMeshData(ObiNativeTriangleMeshHeaderList headers, ObiNativeBIHNodeList nodes,
            ObiNativeTriangleList triangles, ObiNativeVector3List vertices)
        {
            Oni.SetTriangleMeshData(headers.GetIntPtr(), nodes.GetIntPtr(), triangles.GetIntPtr(),
                vertices.GetIntPtr());
        }

        public void SetEdgeMeshData(ObiNativeEdgeMeshHeaderList headers, ObiNativeBIHNodeList nodes,
            ObiNativeEdgeList edges, ObiNativeVector2List vertices)
        {
            Oni.SetEdgeMeshData(headers.GetIntPtr(), nodes.GetIntPtr(), edges.GetIntPtr(), vertices.GetIntPtr());
        }

        public void SetDistanceFieldData(ObiNativeDistanceFieldHeaderList headers, ObiNativeDFNodeList nodes)
        {
            Oni.SetDistanceFieldData(headers.GetIntPtr(), nodes.GetIntPtr());
        }

        public void SetHeightFieldData(ObiNativeHeightFieldHeaderList headers, ObiNativeFloatList samples)
        {
            Oni.SetHeightFieldData(headers.GetIntPtr(), samples.GetIntPtr());
        }

        public void IncreaseReferenceCount()
        {
            referenceCount++;
        }

        public void DecreaseReferenceCount()
        {
            if (--referenceCount <= 0 && gameObject != null)
                DestroyImmediate(gameObject);
        }
    }
}
#endif