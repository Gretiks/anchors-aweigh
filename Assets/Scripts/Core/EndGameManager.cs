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
    
        void Start()
        {
            
            // Dynamicznie zmieniamy napis w zależności od wyniku bitwy
            if (IsVictory)
            {
                _resultText.text = "YOU WIN!";
                _resultText.color = Color.green; // Zielony napis dla wygranej
                
                //dodanie zlota po wygranej
                PlayerDataManager.Instance.AddGold(100);
                SceneManager.LoadScene("ShopScene");
            }
            else
            {
                _resultText.text = "YOU LOSE";
                _resultText.color = Color.red; // Czerwony napis dla przegranej
                _quitButton.SetActive(true);
            }
        }

        public void End()
        {
            SceneManager.LoadScene("MainMenuScene");
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
