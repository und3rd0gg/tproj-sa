using UnityEngine;

namespace Gameplay
{
    public class PointerRotator : MonoBehaviour
    {
        [SerializeField] private Transform _ropeStartConnector;

        private void Update()
        {
            var direction = _ropeStartConnector.position - transform.position;
            transform.forward = -direction;
        }
    }
}