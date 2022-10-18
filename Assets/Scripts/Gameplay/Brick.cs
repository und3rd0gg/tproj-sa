using System.Collections;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Brick : MonoBehaviour, ISuckable
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _collider;
        [SerializeField] private Brick _connectedBrick;
        [SerializeField] private float _maxSqrSpeed = 200;


        private Vector3 _startPosition;
        private Coroutine _suctionRoutine;
        private Coroutine _restoreDefaultScaleRoutine;
        private Transform _target;
        private SuctionArea _suctionArea;
        private Vector3 _defaultScale;

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            GetComponent<MeshCollider>().convex = true;
            _rigidbody.isKinematic = true;
            InitializeConnectedBrick();

            void InitializeConnectedBrick()
            {
                Physics.Raycast(transform.position, Vector3.up * 0.5f, out var hitInfo);
                hitInfo.transform.TryGetComponent(out Brick brick);
                _connectedBrick = brick;
            }
        }

        private void Awake()
        {
            _defaultScale = transform.localScale;
        }

        private void FixedUpdate()
        {
            if (_rigidbody.velocity.sqrMagnitude > _maxSqrSpeed)
                _rigidbody.velocity = _rigidbody.velocity.normalized * 10;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out _suctionArea))
            {
                if (_restoreDefaultScaleRoutine != null)
                    StopCoroutine(_restoreDefaultScaleRoutine);
                _suctionArea.CurrentSuctionRate++;
                    _startPosition = transform.position;
                    _target = other.transform;
                    _suctionRoutine = StartCoroutine(SuctionRoutine());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _suctionArea.CurrentSuctionRate--;
            if (_suctionRoutine != null)
            {
                StopCoroutine(_suctionRoutine);
                _restoreDefaultScaleRoutine = StartCoroutine(RestoreDefaultScaleRoutine());
            }

            EnablePhysics();
        }

        [field: SerializeField]
        public Vector3 ScaleModifier { get; set; } = new Vector3(0.382695436f, 0.153153211f, 0.379052579f);

        public void DisablePhysics()
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
        }

        public void EnablePhysics()
        {
            _rigidbody.isKinematic = false;
            //_connectedBrick?.EnablePhysics();
        }

        private IEnumerator SuctionRoutine()
        {
            for (float i = 0; i < 1; i += Time.deltaTime)
            {
                _rigidbody.MovePosition(Vector3.Lerp(_startPosition, _target.position, Easing(i)));
                transform.localScale = Vector3.MoveTowards(transform.localScale, ScaleModifier, Time.deltaTime * 1.2f);
                yield return null;
            }
        }

        private IEnumerator RestoreDefaultScaleRoutine()
        {
            for (float i = 0; i < 0.6f; i += Time.deltaTime)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, _defaultScale, Time.deltaTime * 0.8f);
                yield return null;
            }
        }

        private static float Easing(float x) => x * x;
    }
}