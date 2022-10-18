using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class BrickPivot : MonoBehaviour, IDeactivatable
    {
        [SerializeField] private float _rotationLoopDuration = 3f;

        private readonly Vector3 _rotation = new Vector3(360, 0, 0);
        private Tween _rotationTween;

        private void Start()
        {
            _rotationTween = transform
                .DOLocalRotate(_rotation, _rotationLoopDuration, RotateMode.LocalAxisAdd)
                .SetLoops(-1)
                .SetEase(Ease.Linear);
        }

        public void Activate()
        {
            StartRotation();
        }

        public void Deactivate()
        {
            StopRotation();
        }

        private void StopRotation()
        {
            _rotationTween.Pause();
        }

        private void StartRotation()
        {
            _rotationTween.Restart();
        }
    }
}