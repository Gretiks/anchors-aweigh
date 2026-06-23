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
        
    }

}
