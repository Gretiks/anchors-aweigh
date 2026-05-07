using Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] private GameObject _selectedHeroObject, _selectedShipObject, _tileObject, _tileUnitObject;
    [SerializeField] private Transform _heroListContainer;
    [SerializeField] private GameObject _heroPanelPrefab, _shipPanelPrefab;
    
    //Ship stats
    [SerializeField] private Text playerHpText, enemyHpText;
    [SerializeField] private Text playerEvasionText, enemyEvasionText;

    // Cannon menu
    [SerializeField] private GameObject _cannonMenuObject;
    [SerializeField] private Text _cannonDescText, _cannonCrewText, _cannonHitChanceText;
    [SerializeField] private Button _cannonFireButton;
    private CannonTile _currentCannon;

    // Mast menu
    [SerializeField] private GameObject _mastMenuObject;
    [SerializeField] private Text _mastDescText, _mastCrewText, _mastEvasionText;

    // Hit/miss popup
    [SerializeField] private GameObject _hitPopupObject;
    [SerializeField] private Text _hitPopupText;

    void Awake() { Instance = this; }

    public void ShowTileInfo(Tile tile)
    {
        if (tile == null)
        {
            _tileObject.SetActive(false);
            _tileUnitObject.SetActive(false);
            return;
        }
        _tileObject.GetComponentInChildren<Text>().text = tile.tileName;
        _tileObject.SetActive(true);
        if (tile.OccupiedUnit)
        {
            _tileUnitObject.GetComponentInChildren<Text>().text = tile.OccupiedUnit.unitName;
            _tileUnitObject.SetActive(true);
        }
    }

    public void ShowSelectedHero(BaseHero hero)
    {
        if (hero == null) { _selectedHeroObject.SetActive(false); return; }
        _selectedHeroObject.GetComponentInChildren<Text>().text = hero.unitName;
        _selectedHeroObject.SetActive(true);
    }

    public void RefreshHeroList(List<BaseHero> heroes)
    {
        foreach (Transform child in _heroListContainer)
            Destroy(child.gameObject);
        foreach (var hero in heroes)
        {
            var card = Instantiate(_heroPanelPrefab, _heroListContainer);
            var texts = card.GetComponentsInChildren<Text>();
            texts[0].text = hero.unitName;
            texts[1].text = $"Move: {hero.UnitMovement}";
        }
    }

    public void ShowShipHealth(BaseShip ship)
    {
        if (ship == null) { _selectedShipObject.SetActive(false); return; }
        _selectedShipObject.GetComponentInChildren<Text>().text = ship.currentHealth.ToString();
        _selectedShipObject.GetComponentInChildren<Text>().text = ship.evasion.ToString();
        _selectedShipObject.SetActive(true);
    }

    public void ShowShipStats(string name, float current, float max, float evasion)
    {
        if (name == "Player")
        {
            playerHpText.text = $"Player HP: {current}/{max}";
            playerEvasionText.text = $"Player Evasion: {evasion * 100:0}%";
        }
        else
        {
            enemyHpText.text = $"Enemy HP: {current}/{max}";
            enemyEvasionText.text = $"Enemy Evasion: {evasion * 100:0}%";
        }
    }

    // Cannon
    public void ShowCannonMenu(CannonTile cannon)
    {
        HideMastMenu();
        _currentCannon = cannon;
        _cannonDescText.text = cannon.cannonDescription;
        _cannonCrewText.text = $"Crew: {cannon.CurrentCrew} / {cannon.requiredCrew}";
        float hitChance = (1f - ShipManager.Instance.enemyShip.evasion) * 100f;
        _cannonHitChanceText.text = $"Hit chance: {hitChance:0}%";
        _cannonFireButton.interactable = cannon.CurrentCrew >= cannon.requiredCrew && !cannon.HasFired;
        _cannonMenuObject.SetActive(true);
    }

    public void HideCannonMenu()
    {
        _currentCannon = null;
        _cannonMenuObject.SetActive(false);
    }

    public void OnCannonFirePressed()
    {
        if (_currentCannon == null) return;
        _currentCannon.Fire();
    }

    // Mast
    public void ShowMastMenu(MastTile mast)
    {
        HideCannonMenu();
        _mastDescText.text = mast.mastDescription;
        _mastCrewText.text = $"Crew: {mast.CurrentCrew} / {mast.maxCrew}";
        _mastEvasionText.text = $"Evasion: +{mast.EvasionBonus * 100:0}%";
        _mastMenuObject.SetActive(true);
    }

    public void HideMastMenu()
    {
        _mastMenuObject.SetActive(false);
    }

    // Hit/miss popup
    public void ShowHitPopup(bool hit)
    {
        _hitPopupText.text = hit ? "HIT!" : "MISS!";
        _hitPopupObject.SetActive(true);
        Invoke(nameof(HideHitPopup), 1.5f);
    }

    private void HideHitPopup() { _hitPopupObject.SetActive(false); }
}