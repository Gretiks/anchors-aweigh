using UnityEngine;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public int Gold { get; private set; } = 50;
    public int BonusDamage { get;  set; } = 0;
    public float BonusHp { get;  set; } = 0;

    public float BonusEvasion { get; set; } = 0f;    
    public float BonusHitChance { get; set; } = 0f;  
    public int BonusShipSpeed { get; set; } = 0;     

    public int PlayerSlotsCount { get; set; } = 5;

    [Header("Progresja trudności wroga")]
    [SerializeField] private int battlesPerShipStage = 2;
    [SerializeField] private int battlesPerCrewIncrease = 4;
    [SerializeField] private float enemyShipHpStep = 25f;
    [SerializeField] private float enemyCannonDmgStep = 5f;
    [SerializeField] private int maxEnemyCrewCap = 5;

    public int BattlesWon { get; private set; } = 0;
    public int ShipProgressionStage => BattlesWon / (battlesPerShipStage > 0 ? battlesPerShipStage : 1);
    public int CrewProgressionStage => BattlesWon / (battlesPerCrewIncrease > 0 ? battlesPerCrewIncrease : 1);

    public int EnemySlotsCount
    {
        get => Mathf.Min(3 + CrewProgressionStage, maxEnemyCrewCap);
        set {} 
    }

    public float GetBonusEnemyShipHp() => ShipProgressionStage * enemyShipHpStep;
    public float GetBonusEnemyCannonDamage() => ShipProgressionStage * enemyCannonDmgStep;

    public void IncrementBattlesWon()
    {
        BattlesWon++;
        Debug.Log($"Zwycięstwo! Wygranych bitew: {BattlesWon}. Załoga wroga w kolejnej walce: {EnemySlotsCount} piratów.");
    }

    [System.Serializable]
    public class ShipSaveData
    {
        public float currentHealth;
    }

    [System.Serializable]
    public class UnitSaveData
    {
        public string uniqueID;
        public string prefabName; 
        public string unitName;
        public int currentMovement;
        public float currentHealth;
        public float maxHealth; 
    }

    public bool HasExistingSave { get; private set; } = false;
    
    private ShipSaveData _shipData = new ShipSaveData();
    private Dictionary<string, UnitSaveData> _unitsData = new Dictionary<string, UnitSaveData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsCrewDamaged()
    {
        if (!HasExistingSave) return false;
        foreach (var kvp in _unitsData)
        {
            // ZMIANA: Lekarz sprawdza tylko tych, którzy przeżyli (HP > 0)
            if (kvp.Key.StartsWith("Player_Hero_") && kvp.Value.currentHealth > 0)
            {
                float max = kvp.Value.maxHealth > 0 ? kvp.Value.maxHealth : 100f;
                if (kvp.Value.currentHealth < max) return true;
            }
        }
        return false;
    }

    public int GetCrewHealCost()
    {
        if (!HasExistingSave || _unitsData.Count == 0) return 100;

        float currentHpSum = 0f;
        float maxHpSum = 0f;

        foreach (var kvp in _unitsData)
        {
            // ZMIANA: Do średniej leczenia nie wliczamy już zwłok (obliczamy próg 50% tylko dla żywych)
            if (kvp.Key.StartsWith("Player_Hero_") && kvp.Value.currentHealth > 0)
            {
                float cur = kvp.Value.currentHealth;
                float max = kvp.Value.maxHealth > 0 ? kvp.Value.maxHealth : 100f;

                currentHpSum += cur;
                maxHpSum += max;
            }
        }

        if (maxHpSum <= 0f) return 100;

        float healthRatio = currentHpSum / maxHpSum;
        return healthRatio >= 0.5f ? 100 : 200;
    }

    public void HealEntireCrew()
    {
        foreach (var kvp in _unitsData)
        {
            // ZMIANA: Leczymy TYLKO żywych
            if (kvp.Key.StartsWith("Player_Hero_") && kvp.Value.currentHealth > 0)
            {
                float max = kvp.Value.maxHealth > 0 ? kvp.Value.maxHealth : 100f;
                kvp.Value.currentHealth = max;
            }
        }
    }
    
    public string GetRandomPolishName()
    {
        string[] names = {
            "Jan", "Stanislaw", "Kazimierz", "Wojciech", "Jerzy", "Zbigniew", 
            "Boguslaw", "Jaroslaw", "Mieszko", "Boleslaw", "Wladyslaw", "Zygmunt", 
            "Czeslaw", "Ignacy", "Waclaw", "Feliks", "Tadeusz", "Andrzej", 
            "Jozef", "Marian", "Lech", "Ryszard", "Kajetan", "Maurycy", "Ambrozy",
            "Anna", "Katarzyna", "Malgorzata", "Barbara", "Zofia", "Elzbieta", 
            "Jadwiga", "Helena", "Marianna", "Agnieszka", "Wiktoria", "Urszula", 
            "Kornelia", "Rozalia", "Teodora", "Konstancja", "Gertruda", "Krystyna", 
            "Wanda", "Hanna", "Balbina", "Franciszka", "Matylda", "Eleonora", "Aniela"
        };
        return names[Random.Range(0, names.Length)];
    }

    public int GetCurrentCrewCount()
    {
        if (!HasExistingSave) return 3;

        int count = 0;
        foreach (var kvp in _unitsData)
        {
            if (kvp.Key.StartsWith("Player_Hero_") && kvp.Value.currentHealth > 0)
            {
                count++;
            }
        }
        return count;
    }

    public bool TryAddRecruit(string prefabName, float maxHp, int movement)
    {
        if (!HasExistingSave)
        {
            for (int i = 0; i < 3; i++)
            {
                string starterID = "Player_Hero_" + i;
                _unitsData[starterID] = new UnitSaveData
                {
                    uniqueID = starterID,
                    prefabName = prefabName,
                    unitName = GetRandomPolishName(),
                    currentMovement = movement,
                    currentHealth = maxHp,
                    maxHealth = maxHp
                };
            }
            HasExistingSave = true;
        }

        if (GetCurrentCrewCount() >= PlayerSlotsCount) return false;

        for (int i = 0; i < PlayerSlotsCount; i++)
        {
            string targetSlotID = "Player_Hero_" + i;
            if (!_unitsData.ContainsKey(targetSlotID))
            {
                _unitsData[targetSlotID] = new UnitSaveData
                {
                    uniqueID = targetSlotID,
                    prefabName = prefabName,
                    unitName = GetRandomPolishName(),
                    currentMovement = movement,
                    currentHealth = maxHp,
                    maxHealth = maxHp
                };
                return true; 
            }
        }
        return false;
    }

    public void SaveShipState(BaseShip ship)
    {
        if (ship == null) return;
        _shipData.currentHealth = ship.currentHealth;
        HasExistingSave = true;
    }

    public bool TryLoadingShipState(BaseShip ship)
    {
        if (!HasExistingSave || ship == null) return false;
        ship.currentHealth = _shipData.currentHealth;
        return true;
    }

    public void SaveUnitState(BaseUnit unit)
    {
        if (unit == null || string.IsNullOrEmpty(unit.uniqueID)) return;
        
        // =====================================================================
        // [NOWOŚĆ: PERMADEATH]: Jeśli jednostka zginęła, natychmiast wymazujemy ją z dysku!
        // =====================================================================
        if (unit.currentHealth <= 0)
        {
            RemoveUnitSaveData(unit.uniqueID);
            return; // Zakończenie metody (nie puszczamy dalej informacji o trupie)
        }
        
        if (!_unitsData.ContainsKey(unit.uniqueID))
            _unitsData[unit.uniqueID] = new UnitSaveData { uniqueID = unit.uniqueID };
        
        _unitsData[unit.uniqueID].prefabName = unit.prefabName; 
        _unitsData[unit.uniqueID].unitName = unit.unitName;
        _unitsData[unit.uniqueID].currentMovement = unit.UnitMovement;
        _unitsData[unit.uniqueID].currentHealth = unit.currentHealth;
        _unitsData[unit.uniqueID].maxHealth = unit.maxHealth; 
        HasExistingSave = true;
    }

    public bool TryLoadUnitState(BaseUnit unit)
    {
        if (unit == null || string.IsNullOrEmpty(unit.uniqueID) || !HasExistingSave) return false;

        if (_unitsData.TryGetValue(unit.uniqueID, out UnitSaveData savedData))
        {
            unit.prefabName = savedData.prefabName;
            unit.unitName = savedData.unitName;
            unit.UnitMovement = savedData.currentMovement;
            unit.currentHealth = savedData.currentHealth;
            return true;
        }
        return false;
    }
    
    public bool TryGetUnitSaveData(string unitID, out UnitSaveData data) => _unitsData.TryGetValue(unitID, out data);

    public void RemoveUnitSaveData(string unitID)
    {
        if (_unitsData.ContainsKey(unitID)) _unitsData.Remove(unitID);
    }

    public void ResetAllData()
    {
        HasExistingSave = false;
        _unitsData.Clear();
        PlayerSlotsCount = 5;
        BattlesWon = 0; 
        
        BonusEvasion = 0f;
        BonusHitChance = 0f;
        BonusShipSpeed = 0;
    }
    
    public void AddGold(int amount) => Gold += amount;

    public bool TrySpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }
    
    public float GetSavedShipHealth() => HasExistingSave ? _shipData.currentHealth : 100f;

    public void UpdateSavedShipHealth(float newHealth)
    {
        _shipData.currentHealth = newHealth;
        HasExistingSave = true;
    }
    
    public List<UnitSaveData> GetAllUnitsData()
    {
        List<UnitSaveData> list = new List<UnitSaveData>();
        foreach (var kvp in _unitsData)
        {
            // Filtrujemy tylko herosów (żeby nie pokazało wrogów, jeśli jacyś zostali w pamięci)
            if (kvp.Key.StartsWith("Player_Hero_"))
            {
                list.Add(kvp.Value);
            }
        }
        return list;
    }
}