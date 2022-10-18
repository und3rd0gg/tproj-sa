using System;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(SphereCollider))]
    public class BrickSucker : MonoBehaviour
    {
        [SerializeField] private Bubble _bubble;
        [SerializeField] private SuctionArea _suctionArea;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out Brick brick))
            {
                brick.DisablePhysics();
                _bubble.AddBrick(brick);
                _suctionArea.CurrentSuctionRate--;
            }

            if (collision.gameObject.GetComponent<Ground>())
                GroundTouched?.Invoke();
        }

        public event Action GroundTouched;
    }
}