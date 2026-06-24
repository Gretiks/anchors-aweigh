using UnityEngine;
using UnityEngine.UI;
using TMPro; // Kluczowe do obsługi TextMeshPro!
using System.Collections.Generic;

public class ShopCrewUI : MonoBehaviour
{
    public static ShopCrewUI Instance;

    [Header("Referencje UI")]
    [SerializeField] private Transform _crewListContainer; 
    [SerializeField] private GameObject _crewCardPrefab;   

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RefreshCrewList();
    }

    public void RefreshCrewList()
    {
        if (PlayerDataManager.Instance == null) return;

        // 1. Czyścimy starą listę
        foreach (Transform child in _crewListContainer)
        {
            Destroy(child.gameObject);
        }

        var units = PlayerDataManager.Instance.GetAllUnitsData();

        foreach (var unit in units)
        {
            if (unit.currentHealth <= 0) continue; 

            // 2. Tworzymy nową kartę
            GameObject card = Instantiate(_crewCardPrefab, _crewListContainer);
            
            // 3. Szukamy tekstów po DOKŁADNYCH NAZWACH (niezależnie czy to Text czy TextMeshPro)
            Transform nameTransform = card.transform.Find("NameText");
            Transform hpTransform = card.transform.Find("HpText");

            if (nameTransform == null || hpTransform == null)
            {
                Debug.LogError($"[BŁĄD UI]: W prefabie CrewCard brakuje obiektów o nazwach 'NameText' lub 'HpText'. Sprawdź nazwy w hierarchii prefabu!");
                continue;
            }

            // 4. Bezpiecznie wpisujemy dane
            SetTextOnTransform(nameTransform, unit.unitName);
            SetTextOnTransform(hpTransform, $"HP: {unit.currentHealth} / {unit.maxHealth}");
        }
    }

    // =====================================================================
    // Uniwersalna metoda ładująca tekst dla Text oraz TextMeshPro
    // =====================================================================
    private void SetTextOnTransform(Transform t, string content)
    {
        // Sprawdzamy, czy to nowoczesny TextMeshPro
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) 
        {
            tmp.text = content;
            return;
        }

        // Jeśli nie, sprawdzamy czy to zwykły Text
        var stdText = t.GetComponent<Text>();
        if (stdText != null)
        {
            stdText.text = content;
            return;
        }

        Debug.LogWarning($"[BŁĄD UI]: Obiekt {t.name} nie ma ani komponentu Text, ani TextMeshProUGUI!");
    }
}