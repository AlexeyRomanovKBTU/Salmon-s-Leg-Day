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
            particleEffect.transform.SetParent(Camera.main.transform);
            var main = particleEffect.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            particleEffect.Play();

            SaveSystem.Save(new SaveData { completed = true });

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnding();
            }

            Destroy(gameObject);
        }
    }
}