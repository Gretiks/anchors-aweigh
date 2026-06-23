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
        
        PlayerShip playerShip = ShipManager.Instance.playerShip;

        void Start() => UpdateUI();

        public void BuyHeal()
        {
            if (playerShip.currentHealth < playerShip.maxHealth)
            {
                if (PlayerDataManager.Instance.TrySpendGold(_healCost))
                {
                    playerShip.currentHealth += _healfAmount;
                    playerShip.currentHealth = Mathf.Clamp(playerShip.currentHealth, 0f, playerShip.maxHealth);
                    UpdateUI();
                }
                
            }
            else
            {
                //za malo zlota jakis popup
            }
        }

        public void UpgradeGuns()
        {
            if (PlayerDataManager.Instance.TrySpendGold(_DamageCost))
            {
                PlayerDataManager.Instance.BonusDamage += _DamageAmount;
                UpdateUI();
            }
        }

        public void UpgradeArmor()
        {
            if (PlayerDataManager.Instance.TrySpendGold(_ArmorCost))
            {
                PlayerDataManager.Instance.BonusHp += _ArmorAmount;
                playerShip.currentHealth += _ArmorAmount;
                UpdateUI();
            }
        }

        public void NextBattle()
        {
            SceneManager.LoadScene("BattleScene");
        }

        private void UpdateUI()
        {
            _ShipHP.text = $"Ship HP: {playerShip.currentHealth}";
            _goldText.text = $"Gold: {PlayerDataManager.Instance.Gold}";
            _ShipDMG.text = $"Cannon damage: {20 + PlayerDataManager.Instance.BonusDamage}";
        }
    }
}