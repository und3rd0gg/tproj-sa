using UnityEngine;

public class SuctionLayer : MonoBehaviour
{
    [SerializeField] private BoxCollider _boxCollider;

    private int _objectInTriggerCount;

    private void Reset()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        _objectInTriggerCount = Physics.OverlapBox(_boxCollider.center, _boxCollider.size).Length;
        Debug.Log(_boxCollider.center);
        Debug.Log(_boxCollider.size);
    }

    private void OnTriggerEnter(Collider other)
    {
        _objectInTriggerCount += 1;
    }
}
