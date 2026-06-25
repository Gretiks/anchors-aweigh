using Grid;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

namespace Core
{
    public class ShopManager : MonoBehaviour
    {
        [SerializeField] private ShopCrewUI _shopCrewUI;
        
        [SerializeField] private int _healCost = 20;
        [SerializeField] private float _healfAmount = 25f;
        
        [SerializeField] private int _DamageCost = 75;
        [SerializeField] private int _DamageAmount = 10;
        
        [SerializeField] private int _ArmorCost = 75;
        [SerializeField] private float _ArmorAmount = 20f;

        [SerializeField] private int _meleeCost = 50;
        [SerializeField] private float _meleeStep = 10f; 
        
        [SerializeField] private int _crewCost = 300;

        // =====================================================================
        // CENY RZADKIECH ULEPSZEŃ
        // =====================================================================
        [Header("Rzadkie Ulepszenia")]
        [SerializeField] private int _evasionCost = 150;
        [SerializeField] private float _evasionStep = 0.10f; // +10%

        [SerializeField] private int _hitChanceCost = 150;
        [SerializeField] private float _hitChanceStep = 0.10f; // +10%

        [SerializeField] private int _shipSpeedCost = 200;
        [SerializeField] private int _shipSpeedStep = 1;

        [Header("Referencje UI")]
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _ShipHP;
        [SerializeField] private TextMeshProUGUI _ShipDMG;
        [SerializeField] private TextMeshProUGUI _crewText; 

        [SerializeField] private TextMeshProUGUI _evasionText;
        [SerializeField] private TextMeshProUGUI _hitChanceText;
        [SerializeField] private TextMeshProUGUI _shipSpeedText;
        [SerializeField] private TextMeshProUGUI _healAllCrewBtnText; // Tekst NA przycisku leczenia
        [SerializeField] private TextMeshProUGUI _meleeDMGText;
        
        [SerializeField] private GameObject _evasionBtn;
        [SerializeField] private GameObject _hitChanceBtn;
        [SerializeField] private GameObject _shipSpeedBtn;
        
        [SerializeField] private float basePlayerMaxHealth = 100f;

        void Start()
        {
            // Losowanie ofert dla każdego z rzadkich ulepszeń (25% szansy)
            _evasionBtn.SetActive(Random.value <= 0.25f);
            _hitChanceBtn.SetActive(Random.value <= 0.25f);
            _shipSpeedBtn.SetActive(Random.value <= 0.25f);
    
            UpdateUI();
        }

        private float GetCurrentMaxHealth()
        {
            float bonus = 0f;
            if (PlayerDataManager.Instance != null)
                bonus = PlayerDataManager.Instance.BonusHp;

            return basePlayerMaxHealth + bonus;
        }
        
        public void BuyHeal()
        {
            if (PlayerDataManager.Instance == null) return;

            float currentHp = PlayerDataManager.Instance.GetSavedShipHealth();
            float maxHp = GetCurrentMaxHealth();

            if (currentHp < maxHp)
            {
                if (PlayerDataManager.Instance.TrySpendGold(_healCost))
                {
                    currentHp += _healfAmount;
                    currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
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

        public void BuyCrew()
        {
            if (PlayerDataManager.Instance == null) return;

            int currentCrew = PlayerDataManager.Instance.GetCurrentCrewCount();
            int maxCrew = PlayerDataManager.Instance.PlayerSlotsCount;

            if (currentCrew < maxCrew)
            {
                if (PlayerDataManager.Instance.TrySpendGold(_crewCost))
                {
                    PlayerDataManager.Instance.TryAddRecruit("Standard_Hero", 100f, 5);
                    UpdateUI();
                }
            }
        }

        // =====================================================================
        // ZAKUP RZADKIECH ULEPSZEŃ I LECZENIA ZAŁOGI
        // =====================================================================
        public void BuyEvasionUpgrade()
        {
            if (PlayerDataManager.Instance == null) return;
            if (PlayerDataManager.Instance.TrySpendGold(_evasionCost))
            {
                PlayerDataManager.Instance.BonusEvasion += _evasionStep;
                UpdateUI();
            }
        }

        public void BuyHitChanceUpgrade()
        {
            if (PlayerDataManager.Instance == null) return;
            if (PlayerDataManager.Instance.TrySpendGold(_hitChanceCost))
            {
                PlayerDataManager.Instance.BonusHitChance += _hitChanceStep;
                UpdateUI();
            }
        }

        public void BuyShipSpeedUpgrade()
        {
            if (PlayerDataManager.Instance == null) return;
            if (PlayerDataManager.Instance.TrySpendGold(_shipSpeedCost))
            {
                PlayerDataManager.Instance.BonusShipSpeed += _shipSpeedStep;
                UpdateUI();
            }
        }

        public void BuyHealAllCrew()
        {
            if (PlayerDataManager.Instance == null) return;

            if (!PlayerDataManager.Instance.IsCrewDamaged())
            {
                Debug.Log("Załoga jest już w 100% zdrowa!");
                return;
            }

            int dynamicCost = PlayerDataManager.Instance.GetCrewHealCost();
            if (PlayerDataManager.Instance.TrySpendGold(dynamicCost))
            {
                PlayerDataManager.Instance.HealEntireCrew();
                UpdateUI();
            }
        }

        public void BuyMeleeUpgrade()
        {
            if (PlayerDataManager.Instance == null) return;
            if (PlayerDataManager.Instance.TrySpendGold(_meleeCost))
            {
                PlayerDataManager.Instance.BonusMeleeDamage += _meleeStep;
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
                    UnityEditor.EditorApplication.isPlaying = false;
            #else
                    Application.Quit();
            #endif
        }
        
        private void UpdateUI()
        {
            if (PlayerDataManager.Instance == null) return;

            // 1. Aktualizacja podstawowych statystyk statku i złota
            float currentHp = PlayerDataManager.Instance.GetSavedShipHealth();
            float maxHp = GetCurrentMaxHealth();
            
            if (_ShipHP != null)
                _ShipHP.text = $"Ship HP: {currentHp} / {maxHp}";
            
            if (_goldText != null)
                _goldText.text = $"Gold: {PlayerDataManager.Instance.Gold}";
            
            if (_ShipDMG != null)
                _ShipDMG.text = $"Cannon damage: {20 + PlayerDataManager.Instance.BonusDamage}";

            // 2. Aktualizacja licznika załogi (np. "Crew: 3 / 5")
            if (_crewText != null)
            {
                int currentCrew = PlayerDataManager.Instance.GetCurrentCrewCount();
                int maxCrew = PlayerDataManager.Instance.PlayerSlotsCount;
                _crewText.text = $"Crew: {currentCrew} / {maxCrew}";
            }

            if (_meleeDMGText != null)
            {
                _meleeDMGText.text = $"Melee Damage: {35 + PlayerDataManager.Instance.BonusMeleeDamage}";
            }
            
            // 3. Aktualizacja tekstów rzadkich ulepszeń
            if (_evasionText != null)
                _evasionText.text = $"Ship Evasion: +{Mathf.RoundToInt(PlayerDataManager.Instance.BonusEvasion * 100)}%";

            if (_hitChanceText != null)
                _hitChanceText.text = $"Cannons Accuracy: +{Mathf.RoundToInt(PlayerDataManager.Instance.BonusHitChance * 100)}%";

            if (_shipSpeedText != null)
                _shipSpeedText.text = $"Ship Speed: +{PlayerDataManager.Instance.BonusShipSpeed}";

            // 4. Dynamiczna aktualizacja ceny leczenia załogi
            if (_healAllCrewBtnText != null)
            {
                int cost = PlayerDataManager.Instance.GetCrewHealCost();
                _healAllCrewBtnText.text = $"Heal All Crew ({cost}G)";
            }

            // =========================================================
            // 5. ODŚWIEŻENIE LISTY ZAŁOGI PRZY KAŻDEJ ZMIANIE W SKLEPIE
            // =========================================================
            if (ShopCrewUI.Instance != null) 
            {
                ShopCrewUI.Instance.RefreshCrewList();
            }
        }
    }
}