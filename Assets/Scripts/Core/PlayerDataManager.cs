using UnityEngine;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public int Gold { get; private set; } = 50;
    public int BonusDamage { get;  set; } = 0;
    public float BonusHp { get;  set; } = 0;

    [System.Serializable]
    public class ShipSaveData
    {
        public float currentHealth;
        //miejsce na ulepszenia statku
    }

    [System.Serializable]
    public class UnitSaveData
    {
        public string uniqueID;
        public string unitName;
        public int currentMovement;
        public float currentHealth;
        //inne statystyki
    }

    // --- PRZECHOWYWANE STANÓW ---
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
    
    // ================= METODY DLA STATKU =================
    public void SaveShipState(BaseShip ship)
    {
        _shipData.currentHealth = ship.currentHealth;
        HasExistingSave = true;
    }

    public bool TryLoadingShipState(BaseShip ship)
    {
        if (!HasExistingSave) return false;
        
        ship.currentHealth = _shipData.currentHealth;
        return true;
    }

    // ================= METODY DLA JEDNOSTEK =================

    public void SaveUnitState(BaseUnit unit)
    {
        if (string.IsNullOrEmpty(unit.uniqueID)) return;
        
        if (!_unitsData.ContainsKey(unit.uniqueID))
            _unitsData[unit.uniqueID] = new UnitSaveData { uniqueID = unit.uniqueID };
        
        _unitsData[unit.uniqueID].unitName = unit.unitName;
        _unitsData[unit.uniqueID].currentMovement = unit.UnitMovement;
        _unitsData[unit.uniqueID].currentHealth = unit.currentHealth;
        HasExistingSave = true;
    }

    public bool TryLoadUnitState(BaseUnit unit)
    {
        if (string.IsNullOrEmpty(unit.uniqueID) || !HasExistingSave) return false;

        if (_unitsData.TryGetValue(unit.uniqueID, out UnitSaveData savedData))
        {
            unit.UnitMovement = savedData.currentMovement;
            unit.currentHealth = savedData.currentHealth;
            return true;
        }
        return false;
    }
    
    public bool TryGetUnitSaveData(string unitID, out UnitSaveData data)
    {
        return _unitsData.TryGetValue(unitID, out data);
    }

    public void ResetAllData()
    {
        HasExistingSave = false;
        _unitsData.Clear();
    }
    
    public void AddGold(int amount)
    {
        Gold += amount;
        Debug.Log($"Zdobyto {amount} złota. Aktualny stan: {Gold}");
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }
    
    public float GetSavedShipHealth()
    {
        if (HasExistingSave)
            return _shipData.currentHealth;
        
        return 100f; 
    }
    
    public void UpdateSavedShipHealth(float newHealth)
    {
        _shipData.currentHealth = newHealth;
        HasExistingSave = true; //
    }

}
