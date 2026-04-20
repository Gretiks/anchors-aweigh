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

    [SerializeField] private TextMeshProUGUI hpText;


    void Awake()
    {
        Instance = this;
    }

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
        if (hero == null)
        {
            _selectedHeroObject.SetActive(false);
            return;
        }

        _selectedHeroObject.GetComponentInChildren<Text>().text = hero.unitName;
        _selectedHeroObject.SetActive(true);
    }

    public void RefreshHeroList(List<BaseHero> heroes)
    {
        // Clear old cards
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
        if (ship == null)
        {
            _selectedShipObject.SetActive(false);
            return;
        }
        
        _selectedShipObject.GetComponentInChildren<Text>().text = ship.currentHealth.ToString();
        _selectedShipObject.SetActive(true);
    }

    public void RefreshShipHealth(BaseShip ship)
    {
        Destroy(ship.gameObject);

        var card = Instantiate(_shipPanelPrefab);
        var texts = card.GetComponentsInChildren<Text>();
        texts[0].text = ship.currentHealth.ToString();
    }

    public void ShowShipStats(string name, float current, float max)
    {
        _shipPanelPrefab.SetActive(true);
        hpText.text = $"{name} HP: {current}/{max}";
    }
}
