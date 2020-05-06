using System.Collections.Generic;
using UnityEngine;

public enum MatchType { empty, color0, color1, color2, color3, color4, color5, color6, color7, color8, color9, color10, color11, color12, color13, color14, color15 };
public enum MatchPattern { Triple }

/// <summary>
/// Stores 3 coord for the match request
/// </summary>
public struct MatchElement
{
    public List<Vector2> coords;
    public MatchElement(List<Vector2> coords)
    {
        this.coords = coords;
    }
}

/// <summary>
/// Move element for fill event. Which hexagon element, from where to.
/// </summary>
public struct MoveElement
{
    public HexagonElement hexagonElement;
    public Vector2 from;
    public Vector2 to;
    public MoveElement(HexagonElement hexagonElement, Vector2 from, Vector2 to)
    {
        this.hexagonElement = hexagonElement;
        this.from = from;
        this.to = to;
    }
}

/// <summary>
/// Is match request is valid that user tried to? If it is valid how much turn is needed? All explode elements and bomb event.
/// </summary>
public struct ExplodeEvent
{
    public bool IsActionValid;
    public int turnNo;
    public List<ExplodeElement> explodeElements;
    public BombEvent bombEvent;
    public ExplodeEvent(bool IsActionValid, int turnNo, List<ExplodeElement> explodeElements)
    {
        this.IsActionValid = IsActionValid;
        this.turnNo = turnNo;
        this.explodeElements = explodeElements;
        this.bombEvent = new BombEvent();
    }
}

/// <summary>
/// Which type exploded with which pattern. The positions of the exploded hexagon elements.
/// </summary>
public struct ExplodeElement
{
    public MatchType type;
    public MatchPattern explosion;
    public List<Vector2> positions;
    public ExplodeElement(MatchType type, MatchPattern explosion, List<Vector2> positions)
    {
        this.type = type;
        this.explosion = explosion;
        this.positions = positions;
    }
}

/// <summary>
/// Is there a move left for player to play or game is over? All move elements to fill vacancy hexagons.
/// </summary>
public struct FillEvent
{
    public bool leftMove;
    public List<List<MoveElement>> fillLists;
    public FillEvent(List<List<MoveElement>> fillLists, bool leftMove)
    {
        this.fillLists = fillLists;
        this.leftMove = leftMove;
    }
}

/// <summary>
/// Did bomb explode? All bomb events in the grid.
/// </summary>
public struct BombEvent
{
    public bool isBombeExploded;
    public List<BombElement> bombElements;
    public BombEvent(bool bombExploded, List<BombElement> bombElements)
    {
        this.isBombeExploded = bombExploded;
        this.bombElements = bombElements;
    }
}

/// <summary>
/// At which coords the bomb info is.
/// </summary>
public struct BombElement
{
    public Vector2 coords;
    public BombInfo bombInfo;
    public BombElement(Vector2 coords, BombInfo bombInfo)
    {
        this.coords = coords;
        this.bombInfo = bombInfo;
    }
}