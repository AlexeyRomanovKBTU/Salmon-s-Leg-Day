using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balance : MonoBehaviour
{
    public float targetRotation;
    Rigidbody2D rb;
    public float force;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetRotation, force * Time.deltaTime));
    }
}
