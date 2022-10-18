using UnityEngine;

namespace Obi
{
    public class ObiTerrainShapeTracker : ObiShapeTracker
    {
        private ObiHeightFieldHandle handle;
        private bool heightmapDataHasChanged = false;

        public ObiTerrainShapeTracker(ObiCollider source, TerrainCollider collider)
        {
            this.source = source;
            this.collider = collider;
        }

        public void UpdateHeightData()
        {
            ObiColliderWorld.GetInstance().DestroyHeightField(handle);
        }

        public override bool UpdateIfNeeded()
        {
            var terrain = collider as TerrainCollider;

            // retrieve collision world and index:
            var world = ObiColliderWorld.GetInstance();
            var index = source.Handle.index;

            var resolution = terrain.terrainData.heightmapResolution;

            // get or create the heightfield:
            if (handle == null || !handle.isValid)
            {
                handle = world.GetOrCreateHeightField(terrain.terrainData);
                handle.Reference();
            }

            // update collider:
            var shape = world.colliderShapes[index];
            shape.type = ColliderShape.ShapeType.Heightmap;
            shape.filter = source.Filter;
            shape.flags = terrain.isTrigger ? 1 : 0;
            shape.rigidbodyIndex = source.Rigidbody != null ? source.Rigidbody.handle.index : -1;
            shape.materialIndex = source.CollisionMaterial != null ? source.CollisionMaterial.handle.index : -1;
            shape.contactOffset = source.Thickness;
            shape.dataIndex = handle.index;
            shape.size = terrain.terrainData.size;
            shape.center = new Vector4(resolution, resolution, resolution, resolution);
            world.colliderShapes[index] = shape;

            // update bounds:
            var aabb = world.colliderAabbs[index];
            aabb.FromBounds(terrain.bounds, shape.contactOffset);
            world.colliderAabbs[index] = aabb;

            // update transform:
            var trfm = world.colliderTransforms[index];
            trfm.FromTransform(terrain.transform);
            world.colliderTransforms[index] = trfm;

            return true;
        }

        public override void Destroy()
        {
            base.Destroy();

            if (handle != null && handle.Dereference())
                ObiColliderWorld.GetInstance().DestroyHeightField(handle);
        }
    }
}