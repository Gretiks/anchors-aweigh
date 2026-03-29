using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _highlight;

    public void Innit(bool isOffset) { 
        _spriteRenderer.color = isOffset ? _offsetColor : _baseColor;
    }

    private void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        _highlight.SetActive(false);
    }
}
