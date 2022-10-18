using Gameplay;
using UnityEngine;

public class BricksRoot : MonoBehaviour
{
    [SerializeField] private Vector3 _suckedScale;

    public int BricksCount { get; private set; }

    private void OnValidate()
    {
        var children = transform.GetComponentsInChildren<Brick>();

        foreach (var child in children)
        {
            child.ScaleModifier = _suckedScale;
        }
    }

    private void OnEnable()
    {
        BricksCount = transform.childCount;
        transform.DetachChildren();
    }
}
