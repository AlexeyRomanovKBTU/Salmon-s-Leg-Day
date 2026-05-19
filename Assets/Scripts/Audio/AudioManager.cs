using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    public AudioClip[] slaps;
    public AudioClip eugh;
    public AudioClip background;
    public AudioClip ending_start;
    public AudioClip ending_loop;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        musicSource.clip = background;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayEnding()
    {
        musicSource.clip = ending_start;
        musicSource.loop = false;
        musicSource.Play();
        StartCoroutine(PlayEndingLoopAfterDelay(ending_start.length));
    }

    private IEnumerator PlayEndingLoopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        musicSource.clip = ending_loop;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void PlaySlapsSFX(AudioClip[] clips)
    {
        if (!SFXSource.isPlaying)
        {
            int randomIndex = Random.Range(0, clips.Length);
            SFXSource.PlayOneShot(clips[randomIndex]);
        }
    }
}