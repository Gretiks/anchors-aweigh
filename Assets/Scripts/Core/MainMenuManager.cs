using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Scene Configuration")]
    [SerializeField] private string battleSceneName = "BattleScene";

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
    
    public void StartGame()
    {
        Debug.Log("Uruchamianie bitwy morskiej...");
        SceneManager.LoadScene(battleSceneName);
    }
    
    public void ShowCredits()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }
    
    public void BackToMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
    
    public void CloseGame()
    {
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