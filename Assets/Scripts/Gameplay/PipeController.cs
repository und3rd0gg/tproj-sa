using DG.Tweening;
using Packages.Joystick_Pack.Scripts.Joysticks;
using UnityEngine;

namespace Gameplay
{
    public class PipeController : MonoBehaviour, IDeactivatable
    {
        [SerializeField] private BoxCollider _suctionController;
        [SerializeField] private DynamicJoystick _joystick;
        [SerializeField] [Range(0.0001f, 5f)] private float _suctionPointSpeed;
        [SerializeField] private bool _isActivated;

        private SuctionPointConstrains _moveConstrains;
        private Vector3 _position;
        private Vector3 _startPosition;

        private Vector2 LastMousePosition;

        private float minX, maxX, minZ, maxZ;

        private void Awake()
        {
            _startPosition = transform.position;
            var bounds = _suctionController.bounds;
            minX = bounds.max.x;
            maxX = bounds.min.x;
            minZ = bounds.max.z;
            maxZ = bounds.min.z;
            _moveConstrains = new SuctionPointConstrains(bounds.max.x, bounds.min.x,
                bounds.max.z, bounds.min.z);
            _suctionController.enabled = false;
        }

        private void Update()
        {
            _position = transform.position;
            var mouseDelta = new Vector2((Input.mousePosition.x - LastMousePosition.x) * Time.deltaTime,
                (Input.mousePosition.y - LastMousePosition.y) * Time.deltaTime);
            LastMousePosition = Input.mousePosition;

            if (Input.GetMouseButton(0))
            {
                if (_position.x - mouseDelta.x < minX && _position.x - mouseDelta.x > maxX)
                    _position.x -= mouseDelta.x * 0.25f;

                if (_position.z - mouseDelta.y < minZ && _position.z - mouseDelta.y > maxZ)
                    _position.z -= mouseDelta.y * 0.25f;
            }
            else
            {
                _position = Vector3.MoveTowards(transform.position,
                    new Vector3(_startPosition.x, transform.position.y, _startPosition.z), Time.deltaTime * 3f);
            }

            transform.position = _position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(new Vector3(minX, transform.position.y, transform.position.z), 0.3f);
            Gizmos.DrawSphere(new Vector3(maxX, transform.position.y, transform.position.z), 0.3f);
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y, minZ), 0.3f);
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y, maxZ), 0.3f);
        }

        public void Activate()
        {
            enabled = true;
        }

        public void Deactivate()
        {
            enabled = false;
        }

        public void MoveToStart(float time)
        {
            transform.DOMove(_startPosition, time);
        }
    }
}