using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource backgroundSource;
    [SerializeField] AudioSource sfxSource;

    public AudioClip background;
    public AudioClip fire;
    public AudioClip miss;
    public AudioClip coin;
    public AudioClip pawn;

    private void Start()
    {
        backgroundSource.clip = background;
        backgroundSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
