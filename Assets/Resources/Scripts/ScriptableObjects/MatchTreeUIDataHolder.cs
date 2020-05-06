using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds UI elements for the game
/// </summary>
[CreateAssetMenu(fileName = "MatchTreeUIDataHolder", menuName = "ScriptableObjects/MatchTreeUIDataHolder")]
public class MatchTreeUIDataHolder : ScriptableObject
{
    [SerializeField]
    private GameObject hexagonPrefab = null;
    [SerializeField]
    private Sprite hexagonSprite = null;
    [SerializeField]
    private Sprite selectedHexagonSprite = null;
    [SerializeField]
    private List<Color> spriteColors = new List<Color>();
    

    public GameObject HexagonPrefab { get => hexagonPrefab; }
    public Sprite SelectedHexagonSprite { get => selectedHexagonSprite; }
    public List<Color> SpriteColors { get => spriteColors; }
    public Sprite HexagonSprite { get => hexagonSprite; }
}
