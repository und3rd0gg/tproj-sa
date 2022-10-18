using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTest : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;

    private void Update()
    {
        Debug.Log(_rigidbody.velocity.sqrMagnitude);
    }
}
