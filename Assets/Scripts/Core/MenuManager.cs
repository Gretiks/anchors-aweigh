using Assets.Scripts.Grid.Tiles.Modules;
using Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] private GameObject _selectedHeroObject, _selectedShipObject;
    [SerializeField] private Transform _heroListContainer, _enemyListContainer;
    [SerializeField] private GameObject _heroPanelPrefab, _shipPanelPrefab, _enemyPanelPrefab;
    
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

    // Helm menu
    [SerializeField] private GameObject _helmMenuObject;
    [SerializeField] private Text _helmDescText, _helmCrewText;
    [SerializeField] private Button _approachButton, _stopButton, _fleeButton;
    [SerializeField] private Text _helmOrderText;
    private HelmTile _currentHelm;

    //Position bar
    [SerializeField] private RectTransform _playerPositionMarker;
    [SerializeField] private RectTransform _enemyPositionMarker;
    [SerializeField] private float _barMinX = -200f; // lewa kraw�d� paska w px
    [SerializeField] private float _barMaxX = 200f;  // prawa kraw�d� paska w px

    //Turn indicator
    [SerializeField] private GameObject _playerTurnIndicator;
    [SerializeField] private GameObject _enemyTurnIndicator;

    void Awake() { Instance = this; }

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
            texts[1].text = $"HP: {hero.currentHealth}/{hero.maxHealth}";
            texts[2].text = $"Move: {hero.UnitMovement}/{hero.baseMovement}";
            texts[3].text = hero.hasAttacked ? "Attacked" : "Ready";
        }
    }

    public void RefreshEnemyList(List<BaseEnemy> enemies)
    {
        foreach (Transform child in _enemyListContainer)
            Destroy(child.gameObject);
        foreach (var enemy in enemies)
        {
            var card = Instantiate(_enemyPanelPrefab, _enemyListContainer);
            var texts = card.GetComponentsInChildren<Text>();
            texts[0].text = enemy.unitName;
            texts[1].text = $"HP: {enemy.currentHealth}/{enemy.maxHealth}";
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


    //Helm
    public void ShowHelmMenu(HelmTile helm)
    {
        HideCannonMenu();
        HideMastMenu();
        _currentHelm = helm;
        _helmDescText.text = helm.helmDescription;
        _helmCrewText.text = $"Crew: {helm.CurrentCrew}";
        _helmOrderText.text = $"Order: {helm.CurrentOrder}";
        _helmMenuObject.SetActive(true);
    }

    public void OnApproachPressed()
    {
        _currentHelm?.SetOrder(HelmTile.HelmOrder.Approach);
        _helmOrderText.text = "Order: Approach";
    }

    public void OnStopPressed()
    {
        _currentHelm?.SetOrder(HelmTile.HelmOrder.Stop);
        _helmOrderText.text = "Order: Stop";
    }

    public void OnFleePressed()
    {
        _currentHelm?.SetOrder(HelmTile.HelmOrder.Flee);
        _helmOrderText.text = "Order: Flee";
    }

    public void HideHelmMenu()
    {
        _currentHelm = null;
        _helmMenuObject.SetActive(false);
    }

    //Position bar
    public void UpdatePositionBar()
    {
        if (ShipManager.Instance.playerShip == null || ShipManager.Instance.enemyShip == null) return;

        _playerPositionMarker.anchoredPosition = new Vector2(
            Mathf.Lerp(_barMinX, _barMaxX, (ShipManager.Instance.playerShip.Position + 10f) / 20f), 0);
        _enemyPositionMarker.anchoredPosition = new Vector2(
            Mathf.Lerp(_barMinX, _barMaxX, (ShipManager.Instance.enemyShip.Position + 10f) / 20f), 0);
    }

    //Turn indicator
    public void ShowTurnIndicator(bool isPlayerTurn)
    {
        _playerTurnIndicator.SetActive(isPlayerTurn);
        _enemyTurnIndicator.SetActive(!isPlayerTurn);
    }

}