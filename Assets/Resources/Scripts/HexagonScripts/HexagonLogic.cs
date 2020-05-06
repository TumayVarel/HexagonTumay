using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Hexagon grid element. Consist of match type and bomb info.
/// </summary>
public struct HexagonElement
{
    public MatchType matchType;
    public BombInfo bombInfo;
    public HexagonElement(MatchType matchType, BombInfo bombInfo = new BombInfo())
    {
        this.matchType = matchType;
        this.bombInfo = bombInfo;
    }
}

/// <summary>
/// Bomb info for hexagon grid elements.
/// </summary>
public struct BombInfo
{
    public bool hasBomb;
    public int bombLeftMove;
    public BombInfo(bool hasBomb = false, int bombLeftMove = 0)
    {
        this.hasBomb = hasBomb;
        this.bombLeftMove = bombLeftMove;
    }
}

/// <summary>
/// Creates, clones, set and gets elements of the Hexagon Grid.
/// </summary>
public struct HexagonGrid : ICloneable
{
    public readonly int width;
    public readonly int height;
    public HexagonElement[,] grid;

    public HexagonGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.grid = new HexagonElement[this.width, this.height];
    }

    /// <summary>
    /// Get hexagon element at the specified coords.
    /// </summary>
    public HexagonElement? GetElement(int x, int y)
    {
        return (this.IsValid(x, y) ? (HexagonElement?)this.grid[x, y] : null);
    }

    /// <summary>
    /// Get match type at the specified coords.
    /// </summary>
    public MatchType? GetMatchType(int x, int y)
    {
        return (this.IsValid(x, y) ? (MatchType?)this.grid[x, y].matchType : null);
    }

    /// <summary>
    /// Set match type of the grid element at the specified coords.
    /// </summary>
    public void SetMatchType(int x, int y, MatchType type)
    {
        this.grid[x, y] = new HexagonElement(type);
    }

    /// <summary>
    /// Set hexagon element to the coords.
    /// </summary>
    public void SetElement(int x, int y, HexagonElement element)
    {
        this.grid[x, y] = element;
    }

    /// <summary>
    /// Clone the the grid.
    /// </summary>
    public object Clone()
    {
        var g = new HexagonGrid(this.width, this.height);
        g.grid = new HexagonElement[this.width, this.height];
        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                g.grid[x, y] = this.grid[x, y];
            }
        }
        return g;
    }

    /// <summary>
    /// Is coords in the grid?
    /// </summary>
    public bool IsValid(int x, int y)
    {
        if (0 > x || x > this.width - 1)
        {
            return false;
        }
        if (0 > y || y > this.height - 1)
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// Specifies the match rules of the game and check grid according to these rules.
/// </summary>
public struct HexagonRule : IComparable
{
    private int priority;
    private MatchType type;
    private Tuple<int, int> modularCondition;
    public readonly MatchPattern pattern;
    private List<Tuple<int, int>> offsets;

    public HexagonRule(int priority, MatchType type, Tuple<int, int> modularCondition, MatchPattern pattern, List<Tuple<int, int>> offsets)
    {
        this.priority = priority;
        this.pattern = pattern;
        this.modularCondition = modularCondition;
        this.type = type;
        this.offsets = offsets;
    }

    /// <summary>
    /// Specifies match rules to use them at the game to check every match attempt.
    /// Rules include their priority number, allowed math types, modular conditions for coords, match pattern and offset values.
    /// </summary>
    public static List<HexagonRule> Rules(int colorNo)
    {
        List<HexagonRule> array = new List<HexagonRule>();
        int addedColorNo = 0;
        foreach (MatchType type in Enum.GetValues(typeof(MatchType)))
        {
            if (type == MatchType.empty)
                continue;
            if (addedColorNo >= colorNo)
                break;
            array.Add(new HexagonRule(0, type, new Tuple<int, int>(0, 0), MatchPattern.Triple, new List<Tuple<int, int>> { new Tuple<int, int>(0, 0), new Tuple<int, int>(1, 0), new Tuple<int, int>(1, 1)}));
            array.Add(new HexagonRule(0, type, new Tuple<int, int>(0, 1), MatchPattern.Triple, new List<Tuple<int, int>> { new Tuple<int, int>(0, 0), new Tuple<int, int>(1, 0), new Tuple<int, int>(1, 1) }));
            array.Add(new HexagonRule(0, type, new Tuple<int, int>(1, 0), MatchPattern.Triple, new List<Tuple<int, int>> { new Tuple<int, int>(0, 0), new Tuple<int, int>(1, 0), new Tuple<int, int>(0, 1) }));
            array.Add(new HexagonRule(0, type, new Tuple<int, int>(1, 1), MatchPattern.Triple, new List<Tuple<int, int>> { new Tuple<int, int>(0, 0), new Tuple<int, int>(1, 0), new Tuple<int, int>(0, 1) }));
            array.Add(new HexagonRule(0, type, new Tuple<int, int>(1, 0), MatchPattern.Triple, new List<Tuple<int, int>> { new Tuple<int, int>(0, 0), new Tuple<int, int>(1, -1), new Tuple<int, int>(1, 0) }));
            array.Add(new HexagonRule(0, type, new Tuple<int, int>(1, 1), MatchPattern.Triple, new List<Tuple<int, int>> { new Tuple<int, int>(0, 0), new Tuple<int, int>(1, -1), new Tuple<int, int>(1, 0) }));
            array.Add(new HexagonRule(0, type, new Tuple<int, int>(0, 0), MatchPattern.Triple, new List<Tuple<int, int>> { new Tuple<int, int>(0, 0), new Tuple<int, int>(0, 1), new Tuple<int, int>(1, 1) }));
            array.Add(new HexagonRule(0, type, new Tuple<int, int>(0, 1), MatchPattern.Triple, new List<Tuple<int, int>> { new Tuple<int, int>(0, 0), new Tuple<int, int>(0, 1), new Tuple<int, int>(1, 1) }));
            addedColorNo++;
        }
        return array;
    }

    /// <summary>
    /// Check coords according match rules to see if there is a match.
    /// First it check modular conditions, if it suits to that rule then check every offset value whether all the grids at offset coords are the same match type.
    /// If every grid have same match then turn true. It means there is a match according to this match rule.
    /// </summary>
    public bool Check(HexagonGrid grid, int x, int y)
    {
        if (!(x % 2 == this.modularCondition.Item1 && y % 2 == this.modularCondition.Item2))
            return false;
        foreach (Tuple<int, int> offset in offsets)
        {
            int dx = offset.Item1;
            int dy = offset.Item2;
            if (grid.GetMatchType(x + dx, y + dy) != this.type)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Set empty the grid to prepare fill or move event to the grid element.
    /// </summary>
    public List<Vector2> Clean(HexagonGrid grid, int x, int y)
    {
        var positions = new List<Vector2>();
        foreach (Tuple<int, int> offset in offsets)
        {
            var dx = offset.Item1;
            var dy = offset.Item2;
            grid.SetMatchType(x + dx, y + dy, MatchType.empty);
            positions.Add(new Vector2(x + dx, y + dy));
        }
        return positions;
    }

    /// <summary>
    /// Rules priority check.
    /// </summary>
    public int CompareTo(object obj)
    {
        return this.priority - ((HexagonRule)obj).priority;
    }
}

/// <summary>
/// Haxegon moves. Specifies the grid movement.
/// </summary>
public struct HexagonMove : IComparable
{
    private int priority;
    private List<Tuple<int, int>> offsets;

    public HexagonMove(int priority, List<Tuple<int, int>> offsets)
    {
        this.priority = priority;
        this.offsets = offsets;
    }

    /// <summary>
    /// Specifies the moves with their priotiy, and offset values.
    /// </summary>
    public static List<HexagonMove> Moves()
    {
        List<HexagonMove> array = new List<HexagonMove>();
        array.Add(new HexagonMove(0, new List<Tuple<int, int>> { new Tuple<int, int>(0, -1) }));
        return array;
    }

    /// <summary>
    /// Move grid element to new destination specified with offset values.
    /// The left grid will become empty for the vacancy.
    /// </summary>
    public Tuple<int, int> Move(HexagonGrid grid, int x, int y, HexagonElement? hexagonElement = null)
    {
        HexagonElement element = hexagonElement == null ? new HexagonElement(MatchType.empty) : (HexagonElement)hexagonElement;
        bool valid = grid.IsValid(x, y);
        if (valid)
        {
            element = (HexagonElement)grid.GetElement(x, y);
        }
        if (element.matchType == MatchType.empty)
        {
            return null;
        }
        foreach (Tuple<int, int> offset in offsets)
        {
            var dx = offset.Item1;
            var dy = offset.Item2;
            if (grid.GetMatchType(x + dx, y + dy) == MatchType.empty)
            {
                grid.SetElement(x + dx, y + dy, element);

                if (valid)
                {
                    grid.SetElement(x, y, new HexagonElement(MatchType.empty));
                }
                return new Tuple<int, int>(x + dx, y + dy);
            }
        }
        return null;
    }

    /// <summary>
    /// Compare moves in terms of their priority.
    /// </summary>
    public int CompareTo(object obj)
    {
        return this.priority - ((HexagonMove)obj).priority;
    }
}

/// <summary>
/// Generate new hexagon element.
/// </summary>
public class HexagonGenerator
{
    public HexagonElement hexagonElement;
    public readonly Tuple<int, int> target;
    private int colorNo;
    private int bombLifeTime;

    public HexagonGenerator(Tuple<int, int> target, int colorNo)
    {
        this.target = target;
        this.colorNo = colorNo;
        this.bombLifeTime = 10;
        this.Generate();
    }

    /// <summary>
    /// Generate a hexagon element with match type from allowable color range.
    /// If bomb has to be dropped it add bomb to the hexagon element.
    /// </summary>
    public void Generate(bool generateBomb = false)
    {
        Array array = Enum.GetValues(typeof(MatchType));
        MatchType matchType = (MatchType)array.GetValue(UnityEngine.Random.Range(1, colorNo + 1)); // No empty
        this.hexagonElement = new HexagonElement(matchType, new BombInfo(generateBomb, generateBomb ? this.bombLifeTime : 0));
    }

}

/// <summary>
/// Scoring class for bomb logic.
/// </summary>
public class HexagonScoring
{
    private int score;
    private int droppedBombCount;
    private int bombScore;
    private int explosionBlockScore;

    public HexagonScoring()
    {
        this.score = 0;
        this.droppedBombCount = 0;
        this.bombScore = 1000;
        this.explosionBlockScore = 5;
    }

    /// <summary>
    /// Calulates the number of exploded block and the score.
    /// </summary>
    /// <param name="explodeEvent"></param>
    public void SetScore(ExplodeEvent explodeEvent)
    {
        if (!explodeEvent.IsActionValid)
            return;
        int explosionCount = 0;
        foreach(ExplodeElement explodeElement in explodeEvent.explodeElements)
        {
            foreach(Vector2 block in explodeElement.positions)
            {
                explosionCount++;
            }
        }
        this.score += explosionCount * this.explosionBlockScore;
    }

    public int GetScore()
    {
        return this.score;
    }

    /// <summary>
    /// Calculates the bomb number that has to be dropped.
    /// </summary>
    public int GetReadyBombCount()
    {
        int unusedScore = this.score - this.droppedBombCount * this.bombScore;
        if (unusedScore < this.bombScore)
            return 0;
        int readyBombCount = unusedScore / this.bombScore;
        this.droppedBombCount += readyBombCount;
        return readyBombCount;
    }

    /// <summary>
    /// If all bombs cannot be dropped at the fill event.
    /// </summary>
    /// <param name="leftMoveBomb"></param>
    public void LeftBombs(int leftMoveBomb)
    {
        this.droppedBombCount -= leftMoveBomb;
    }

    /// <summary>
    /// Check grid to decrease every bomb timer with every user action.
    /// If any bomb timer reaches to 0, then send this information to end the game.
    /// </summary>
    public BombEvent BombCheck(ref HexagonGrid grid)
    {
        BombEvent bombEvent = new BombEvent(false, new List<BombElement>());
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                if (!(bool)grid.GetElement(x, y)?.bombInfo.hasBomb) // If hexagon element does not have bomb then continue
                    continue;
                HexagonElement hexagonElement = (HexagonElement)grid.GetElement(x, y);
                int bombTimer = hexagonElement.bombInfo.bombLeftMove;
                bombTimer--;
                if (bombTimer <= 0) // Bomb time reaches to zero end the game
                {
                    bombEvent.isBombeExploded = true;
                    Debug.Log("Bomb exploded at: (" + x + ", " + y + ")");
                }
                grid.SetElement(x, y, new HexagonElement(hexagonElement.matchType, new BombInfo(hexagonElement.bombInfo.hasBomb, bombTimer)));
                bombEvent.bombElements.Add(new BombElement(new Vector2(x, y), new BombInfo(true, bombTimer))); // Send list of bomb info with their timer to renew UI and such
            }
        }
        return bombEvent;
    }
}

/// <summary>
/// Main hexagon logic class.
/// </summary>
public class HexagonLogic
{
    private HexagonGrid grid;
    private List<HexagonRule> rules;
    private List<HexagonMove> moves;
    private List<HexagonGenerator> generators;
    private HexagonGenerator initializer;
    private int colorNo;
    private SelectionHandler selectionMoves;
    private HexagonScoring scoring;

    public HexagonLogic(int width, int height, int colorNo, List<HexagonRule> rules, List<HexagonMove> moves, List<HexagonGenerator> generators = null, HexagonGenerator initializer = null)
    {
        this.grid = new HexagonGrid(width, height);
        this.rules = rules;
        this.colorNo = colorNo;
        this.rules.Sort();
        this.moves = moves;
        this.moves.Sort();
        this.generators = generators;
        this.initializer = initializer;
        this.selectionMoves = new SelectionHandler(width, height);
        this.scoring = new HexagonScoring();

        // Create generators, at the top of the board.
        if (this.generators == null)
        {
            this.generators = new List<HexagonGenerator>();
            for (int x = 0; x < width; x++)
            {
                this.generators.Add(new HexagonGenerator(new Tuple<int, int>(x, height), this.colorNo));
            }
        }
        // Create initializers all over the board.
        if (this.initializer == null)
        {
            this.initializer = new HexagonGenerator(new Tuple<int, int>(width, height), this.colorNo);
        }
        // Create all board.
        this.Refresh();
    }

    public MatchType? GetGridValue(int x, int y)
    {
        return this.grid.GetMatchType(x, y);
    }

    /// <summary>
    /// With match element, clone mock grids to simulate the situation if the match is valid or not.
    /// Search mock grids to find a match with specified rules. If there is match, add matches to the explode events, send turn numbe rof the mock grid and bomb events.
    /// </summary>
    /// <param name="matchElement">Threee coord values for the match.</param>
    /// <param name="simulate">If match is valid, make it a valid simulation by changing original grid with the mock one.</param>
    /// <returns>Explode event</returns>
    public ExplodeEvent Explode(MatchElement? matchElement, bool isTurnClockwise = false, bool simulate = true)
    {
        // Create mock grid to simulate request
        List<HexagonGrid> mockGrids = new List<HexagonGrid>();
        for (int i = 0; i < 2; i++)
            mockGrids.Add((HexagonGrid)this.grid.Clone());

        // Turn mock grids. First one with 120 degree, secone one with 240 degree.
        if (matchElement != null && matchElement?.coords != null)
        {
            int firstIndex = isTurnClockwise ? 0 : 2;
            int lastIndex = isTurnClockwise ? 2 : 0;
            for (int mockIndex = 0; mockIndex < 2; mockIndex++)
            {
                for (int turn = 0; turn < mockIndex + 1; turn++)
                {
                    HexagonElement element = (HexagonElement)mockGrids[mockIndex].GetElement((int)matchElement?.coords[firstIndex].x, (int)matchElement?.coords[firstIndex].y);
                    mockGrids[mockIndex].grid[(int)matchElement?.coords[firstIndex].x, (int)matchElement?.coords[firstIndex].y] = mockGrids[mockIndex].grid[(int)matchElement?.coords[1].x, (int)matchElement?.coords[1].y];
                    mockGrids[mockIndex].grid[(int)matchElement?.coords[1].x, (int)matchElement?.coords[1].y] = mockGrids[mockIndex].grid[(int)matchElement?.coords[lastIndex].x, (int)matchElement?.coords[lastIndex].y];
                    mockGrids[mockIndex].grid[(int)matchElement?.coords[lastIndex].x, (int)matchElement?.coords[lastIndex].y] = element;
                }
            }
        }

        // Check explode events on the mock grids to see if the action is valid or not.
        // Every rule has to be tried to be sure the match.
        List<ExplodeElement> explodeElements = new List<ExplodeElement>();
        int selectedMockGridNo = 0;
        CheckGridWithrulesToFindMatch(mockGrids, explodeElements, ref selectedMockGridNo);

        // If action is valid and not for game over check  
        if (explodeElements.Count > 0 && simulate)
        {
            this.grid = mockGrids[selectedMockGridNo];
        }

        ExplodeEvent explodeEvent = new ExplodeEvent(explodeElements.Count > 0, selectedMockGridNo + 1, explodeElements);

        // If action is valid and not for game over check  
        if (simulate)
            scoring.SetScore(explodeEvent);

        // If action is valid and done by user then check for bomb timers and explosion.  
        if (matchElement != null && matchElement?.coords != null && matchElement?.coords.Count > 0 && simulate)
        {
            BombEvent bombEvent = scoring.BombCheck(ref this.grid);
            explodeEvent.bombEvent = bombEvent;
        }


        return explodeEvent;
    }

    /// <summary>
    /// Fill method to fill vacancies after explode events with new hexagon elements.
    /// </summary>
    /// <returns>Fill event</returns>
    public FillEvent Fill()
    {
        List<List<MoveElement>> fillEvents = new List<List<MoveElement>>();
        while (true)
        {
            // Check for vacancies to fill these vacancies with other hexagon elements
            List<MoveElement> fillElements = new List<MoveElement>();
            CheckForVacanciesAndMoveHexagonElementsToVacancies(fillElements);


            // Check for bomb ready count
            int generateBombCount = scoring.GetReadyBombCount();

            // Check for generators for fill vacancies and generate new hexagon elements
            GenerateNewElements(fillElements, ref generateBombCount);

            // If all bombs could not be dropped then scoring has to know this
            scoring.LeftBombs(generateBombCount);

            if (fillElements.Count == 0)
            {
                break;
            }
            fillEvents.Add(fillElements);
        }

        bool leftMove = LeftMoveCheck();
        return new FillEvent(fillEvents, leftMove);
    }

    public int GetCurrentScore()
    {
        return scoring.GetScore();
    }

    /// <summary>
    /// Refresh board to change every hexagon elements
    /// </summary>
    public void Refresh()
    {
        for (int i = 0; i < this.grid.width; i++)
        {
            for (int j = 0; j < this.grid.height; j++)
            {
                this.initializer.Generate();
                this.grid.SetElement(i, j, this.initializer.hexagonElement);
            }
        }

        // If created board has no move then refresh again
        if (!LeftMoveCheck())
            Refresh();
    }

    /// <summary>
    /// Check explode events on the mock grids to see if the action is valid or not.
    /// Every rule has to be tried to be sure the match.
    /// </summary>
    /// <param name="mockGrids">Mock Grids</param>
    /// <param name="explodeElements">Explode events</param>
    /// <param name="selectedMockGridNo">Selected mock grid</param>
    /// <returns></returns>
    private void CheckGridWithrulesToFindMatch(List<HexagonGrid> mockGrids, List<ExplodeElement> explodeElements, ref int selectedMockGridNo)
    {
        for (int i = 0; i < 2; i++) // Iterate for two mock grids.
        {
            for (int x = 0; x < mockGrids[i].width; x++)
            {
                for (int y = 0; y < mockGrids[i].height; y++)
                {
                    foreach (HexagonRule rule in this.rules) // Check every rule
                    {
                        if (rule.Check(mockGrids[i], x, y))// if rule is satisfied
                        {
                            List<Vector2> positions = rule.Clean(mockGrids[i], x, y); // Clean the exploded coords for new elements to come
                            explodeElements.Add(new ExplodeElement((MatchType)mockGrids[i].GetMatchType(x, y), rule.pattern, positions));
                        }
                    }
                }
            }
            if (explodeElements.Count > 0)
            {
                selectedMockGridNo = i;
                break;
            }
        }
    }

    /// <summary>
    /// Check for generators for fill vacancies and generate new hexagon elements
    /// </summary>
    /// <param name="fillElements">Fill elements</param>
    /// <param name="randomColomGenerators">Generator indexes to drop bomb</param>
    private void GenerateNewElements(List<MoveElement> fillElements, ref int bombNumber)
    {
        for (int i = 0; i < this.generators.Count; i++) // Iterate for every generators
        {
            int x = generators[i].target.Item1;
            int y = generators[i].target.Item2;
            foreach (HexagonMove move in this.moves) // For each move
            {
                Tuple<int, int> dest = move.Move(this.grid, x, y, generators[i].hexagonElement);
                if (dest != null)
                {
                    fillElements.Add(new MoveElement(generators[i].hexagonElement, new Vector2(x, y), new Vector2(x, y))); // Specifies the generation of the hexagpn element

                    MoveElement element = new MoveElement(generators[i].hexagonElement, new Vector2(x, y), new Vector2(dest.Item1, dest.Item2));
                    fillElements.Add(element);

                    // Generate new hexagon elements with bomb or not
                    if (bombNumber > 0)
                    {
                        generators[i].Generate(true);
                        bombNumber--;
                    }
                    else
                    {
                        generators[i].Generate();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check for vacancies to fill these vacancies with other hexagon elements
    /// </summary>
    /// <param name="fillElementss">Fill elements</param>
    private void CheckForVacanciesAndMoveHexagonElementsToVacancies(List<MoveElement> fillElementss)
    {
        for (int x = 0; x < this.grid.width; x++)
        {
            for (int y = 0; y < this.grid.height; y++)
            {
                foreach (HexagonMove move in this.moves) // Check for every specified move
                {
                    Tuple<int, int> dest = move.Move(this.grid, x, y);
                    if (dest != null) // Move is valid then create move elemnt and add it to the fill element
                    {
                        MoveElement element = new MoveElement((HexagonElement)this.grid.GetElement(x, y), new Vector2(x, y), new Vector2(dest.Item1, dest.Item2));
                        fillElementss.Add(element);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if there is a move for player to play
    /// </summary>
    /// <returns></returns>
    private bool LeftMoveCheck()
    {
        bool leftMove = false;
        for (int x = 0; x < this.grid.width && !leftMove; x++)
        {
            for (int y = 0; y < this.grid.height && !leftMove; y++)
            {
                Vector2 source = new Vector2(x, y);
                List<Vector2> coords = new List<Vector2>();
                coords = selectionMoves.GetSelectionPattern(source); // Get suitable moves for the coord
                leftMove = this.Explode(new MatchElement(coords), simulate : false).IsActionValid; // Send explode to see if there is any check
            }
        }
        return leftMove;
    }
}

