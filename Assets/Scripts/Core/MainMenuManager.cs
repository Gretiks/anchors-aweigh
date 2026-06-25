using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject instructionPanel;

    [Header("Scene Configuration")]
    [SerializeField] private string battleSceneName = "BattleScene";

    private AudioManager _audioManager;
    private AudioManager AudioManagerInstance
    {
        get
        {
            // Jeśli referencja jest pusta, wyszukaj ją
            if (_audioManager == null)
            {
                GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
                if (audioObject != null)
                {
                    _audioManager = audioObject.GetComponent<AudioManager>();
                }
                else
                {
                    Debug.LogWarning("Nie znaleziono obiektu z tagiem 'Audio' w scenie.");
                }
            }
            return _audioManager;
        }
    }

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(false);

    }
    
    public void StartGame()
    {
        if (AudioManagerInstance != null)
        {
            AudioManagerInstance.PlaySFX(AudioManagerInstance.button);
        }
        Debug.Log("Uruchamianie bitwy morskiej...");
        SceneManager.LoadScene(battleSceneName);
    }
    
    public void ShowCredits()
    {
        if (AudioManagerInstance != null)
        {
            AudioManagerInstance.PlaySFX(AudioManagerInstance.button);
        }
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
        if (instructionPanel != null) instructionPanel.SetActive(false);
    }

    public void ShowInstrtuction()
    {
        if(AudioManagerInstance != null)
            AudioManagerInstance.PlaySFX(AudioManagerInstance.button);
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(true);
    }
    public void BackToMainMenu()
    {
        if (AudioManagerInstance != null)
        {
            AudioManagerInstance.PlaySFX(AudioManagerInstance.button);
        }
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(false);

    }
    
    public void CloseGame()
    {
        if (AudioManagerInstance != null)
        {
            AudioManagerInstance.PlaySFX(AudioManagerInstance.button);
        }
        Debug.Log("Zamykanie programu...");
        
        #if UNITY_EDITOR
        // Jeśli testujemy w edytorze Unity, zatrzymaj tryb Play
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Jeśli to gotowa kompilacja, zamknij program
        Application.Quit();
        #endif
    }
}