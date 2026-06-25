using TMPro; // Wymagane do obsługi TextMeshPro
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class EndGameManager : MonoBehaviour
    {
        // Statyczna zmienna, którą ustawimy w GameManagerze przed załadowaniem tej sceny
        public static bool IsVictory = false;
        
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private GameObject _quitButton;
        [SerializeField] private GameObject _endButton;

        private int totalReward = 100;
    
        void Start()
        {
            // Dynamicznie zmieniamy napis w zależności od wyniku bitwy
            if (IsVictory)
            {
                if (PlayerDataManager.Instance != null)
                {
                    int bonusGold = (PlayerDataManager.Instance.BattlesWon / 5) * 50;
                    totalReward += bonusGold;
                    
                    PlayerDataManager.Instance.AddGold(totalReward);
                }
                
                _quitButton.SetActive(PlayerDataManager.Instance.IsBossDefeated);
                _endButton.SetActive(PlayerDataManager.Instance.IsBossDefeated);

                if (!PlayerDataManager.Instance.IsBossDefeated)
                    SceneManager.LoadScene("ShopScene");
                else
                {
                    _resultText.text = "The enemy has been vanquished, and from their captured vessels, we have seized a fortune!We chart our course for home, wealthy beyond measure and unchallenged upon these waters.";
                    _resultText.color = Color.green; // Zielony napis dla wygranej
                }
                
                // SceneManager.LoadScene("ShopScene");
            }
            else
            {
                _resultText.text = "The ship has been lost. Our adversaries have won this battle, but they failed to take my life. I can still return for vengeance.";
                _resultText.color = Color.red; // Czerwony napis dla przegranej
                _quitButton.SetActive(true);
                _endButton.SetActive(true);
            }
        }

        public void End()
        {
            SceneManager.LoadScene("MainMenuScene");
            PlayerDataManager.Instance.ResetAllData();
        }
        
        public void Quit()
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
}