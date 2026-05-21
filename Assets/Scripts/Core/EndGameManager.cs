using TMPro; // Wymagane do obsługi TextMeshPro
using UnityEngine;

namespace Core
{
    public class EndGameManager : MonoBehaviour
    {
        // Statyczna zmienna, którą ustawimy w GameManagerze przed załadowaniem tej sceny
        public static bool IsVictory = false;
    
        [SerializeField] private TextMeshProUGUI _resultText;
    
        void Start()
        {
            if (_resultText == null)
            {
                Debug.LogError("Nie przypisano komponentu tekstowego w inspektorze!");
                return;
            }
    
            // Dynamicznie zmieniamy napis w zależności od wyniku bitwy
            if (IsVictory)
            {
                _resultText.text = "ZWYCIĘSTWO!";
                _resultText.color = Color.green; // Zielony napis dla wygranej
            }
            else
            {
                _resultText.text = "PRZEGRANA";
                _resultText.color = Color.red; // Czerwony napis dla przegranej
            }
        }
    
}

}
