using UnityEngine;

namespace Gameplay
{
    public class SuctionArea : MonoBehaviour, IDeactivatable
    {
        [SerializeField] private SphereCollider _sphereCollider;
        [SerializeField] private BrickSucker _sucker;
        [SerializeField] private GameObject _suctionVFX;

        [field: SerializeField] public int MaxSuctionRate { get; private set; } = 3;
        [field: SerializeField] public int CurrentSuctionRate { get; set; } = 0;
        
        private float _triggerRadius;

        private void Reset()
        {
            _sphereCollider = GetComponent<SphereCollider>();
        }

        private void Awake()
        {
            _triggerRadius = _sphereCollider.radius;
        }

        public void Activate()
        {
            _sphereCollider.radius = _triggerRadius;
            _sucker.gameObject.SetActive(true);
            _suctionVFX.SetActive(true);
        }

        public void Deactivate()
        {
            _sphereCollider.radius = 0;
            _sucker.gameObject.SetActive(false);
            _suctionVFX.SetActive(false);
        }

        public void Upgrade()
        {
            _triggerRadius += 0.1f;
        }
    }
}