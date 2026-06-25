using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource backgroundSource;
    [SerializeField] AudioSource sfxSource;

    public AudioClip background;
    public AudioClip shopBackground;
    public AudioClip fire;
    public AudioClip miss;
    public AudioClip coin;
    public AudioClip pawn;
    public AudioClip sword;
    public AudioClip buy;
    public AudioClip button;
    public AudioClip popup;

    private void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "ShopScene" || currentSceneName == "MainMenuScene")
        {
            backgroundSource.clip = shopBackground;
        }
        else
        {
            backgroundSource.clip = background;
        }

        backgroundSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
