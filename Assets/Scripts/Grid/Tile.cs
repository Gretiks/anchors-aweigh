using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void Innit(bool isOffset) { 
        _spriteRenderer.color = isOffset ? _offsetColor : _baseColor;
    }
}
