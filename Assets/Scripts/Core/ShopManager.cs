using Grid;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Core
{
    public class ShopManager : MonoBehaviour
    {
        [SerializeField] private int _healCost = 20;
        [SerializeField] private float _healfAmount = 25f;
        
        [SerializeField] private int _DamageCost = 50;
        [SerializeField] private int _DamageAmount = 10;
        
        [SerializeField] private int _ArmorCost = 30;
        [SerializeField] private float _ArmorAmount = 20f;
        
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _ShipHP;
        [SerializeField] private TextMeshProUGUI _ShipDMG;
        
        [SerializeField] private GameObject _healButton;
        [SerializeField] private GameObject _DamageButton;
        [SerializeField] private GameObject _ArmorButton;
        [SerializeField] private GameObject _nextButton;
        [SerializeField] private GameObject _quitButton;
        
        [SerializeField] private float basePlayerMaxHealth = 100f;

        void Start() => UpdateUI();

        private float GetCurrentMaxHealth()
        {
            float bonus = 0f;
            if (PlayerDataManager.Instance != null)
                bonus = PlayerDataManager.Instance.BonusHp;

            return basePlayerMaxHealth + bonus;
        }
        
        private float GetCurrentHealthFromSave()
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.HasExistingSave)
                return PlayerDataManager.Instance.GetSavedShipHealth();
            
            return GetCurrentMaxHealth();
        }
        
        public void BuyHeal()
        {
            if (PlayerDataManager.Instance == null) return;

            // Pobieramy aktualne zdrowie z pamięci podręcznej managera danych
            float currentHp = PlayerDataManager.Instance.GetSavedShipHealth();
            float maxHp = GetCurrentMaxHealth();

            if (currentHp < maxHp)
            {
                if (PlayerDataManager.Instance.TrySpendGold(_healCost))
                {
                    currentHp += _healfAmount;
                    currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
                    
                    // Zapisujemy zmodyfikowaną wartość bezpośrednio w PlayerDataManager
                    PlayerDataManager.Instance.UpdateSavedShipHealth(currentHp);
                    
                    UpdateUI();
                }
            }
        }

        public void UpgradeGuns()
        {
            if (PlayerDataManager.Instance == null) return;
            
            if (PlayerDataManager.Instance.TrySpendGold(_DamageCost))
            {
                PlayerDataManager.Instance.BonusDamage += _DamageAmount;
                UpdateUI();
            }
        }

        public void UpgradeArmor()
        {
            if (PlayerDataManager.Instance == null) return;
            
            if (PlayerDataManager.Instance.TrySpendGold(_ArmorCost))
            {
                PlayerDataManager.Instance.BonusHp += _ArmorAmount;
                
                float currentHp = PlayerDataManager.Instance.GetSavedShipHealth();
                currentHp += _ArmorAmount;
                
                PlayerDataManager.Instance.UpdateSavedShipHealth(currentHp);
                
                UpdateUI();
            }
        }

        public void NextBattle()
        {
            SceneManager.LoadScene("BattleScene");
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
        
        private void UpdateUI()
        {
            float currentHp = PlayerDataManager.Instance.GetSavedShipHealth();
            float maxHp = GetCurrentMaxHealth();
            
            _ShipHP.text = $"Ship HP: {currentHp} /  {maxHp}";
            _goldText.text = $"Gold: {PlayerDataManager.Instance.Gold}";
            _ShipDMG.text = $"Cannon damage: {20 + PlayerDataManager.Instance.BonusDamage}";
        }
    }
}