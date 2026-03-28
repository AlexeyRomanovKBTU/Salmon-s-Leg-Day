using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip[] slaps;
    public AudioClip eugh;

    public AudioClip background;
    public AudioClip ending_start;
    public AudioClip ending_loop;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void Update()
    {
        if(!musicSource.isPlaying)
        {
            musicSource.Play();
        }
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
        SFXSource.Stop();
        SFXSource.clip = clip;
        SFXSource.Play();
    }

    public void PlaySlapsSFX(AudioClip[] clips)
    {
        if (!SFXSource.isPlaying)
        {
            int randomIndex = Random.Range(0, clips.Length);
            AudioClip clipToPlay = clips[randomIndex];
            SFXSource.clip = clipToPlay;
            SFXSource.Play();
        }
    }
}
