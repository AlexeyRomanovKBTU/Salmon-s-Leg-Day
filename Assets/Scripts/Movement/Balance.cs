using UnityEngine;

public class Balance : MonoBehaviour
{
    public float targetRotation;
    [SerializeField] private float force;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        rb.MoveRotation(Mathf.LerpAngle(rb.rotation, targetRotation, force * Time.deltaTime));
    }
}