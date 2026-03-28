using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem particleEffect;

    public AudioManager audioManager;

    private void Start()
    {
        particleEffect.Stop();
    }

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            particleEffect.Play();
            
            audioManager.PlayEnding();

            Destroy(gameObject);
        }
    }
}
