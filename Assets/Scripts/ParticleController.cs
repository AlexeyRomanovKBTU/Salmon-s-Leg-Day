using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleEffect;

    private void Start()
    {
        particleEffect.Stop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            particleEffect.transform.SetParent(null);
            particleEffect.Play();
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnding();
            }

            Destroy(gameObject);
        }
    }
}