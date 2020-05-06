using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexagonController : MonoBehaviour
{
    #region Variables & Classes
    public static HexagonController instance;
    [Range(2,15)]
    public int colorNo = 5;
    [Range(1, 9)]
    public int xTreeSize = 8;
    [Range(1, 11)]
    public int yTreeSize = 9;
    public List<ItemComponents> itemComponentsPool = new List<ItemComponents>();
    public MatchTreeUIDataHolder matchTreeUIDataHolder;
    public RectTransform matchTreeSpritesRectTransforms;

    private Dictionary<Vector2, ItemComponents> gridComponentHolder = new Dictionary<Vector2, ItemComponents>();
    private HexagonLogic hexagonLogic;
    private List<Vector2> selectedCoords = new List<Vector2>();
    private SelectionHandler touchMoves;
    private PositionCalculator positionCalculator;
    #endregion

    #region Events
    public delegate void HexagonControllerEvent();
    public static event HexagonControllerEvent SetBoardTouchable;
    public static event HexagonControllerEvent SetBoardUnuouchable;
    public static event HexagonControllerEvent GameOver;

    public delegate void ScoreEvent(int score);
    public static event ScoreEvent IncreaseScore;
    #endregion


    private void Awake()
    {
        SetLevel();
        instance = this;
    }

    private void OnEnable()
    {
        GameOver += StopAllCoroutines;
    }

    private void OnDisable()
    {
        GameOver -= StopAllCoroutines;
    }

    /// <summary>
    /// Set match element to explode initialized board if there is any match
    /// </summary>
    private void Start()
    {
        SetMatchElement();
    }

    /// <summary>
    /// Set rules, moves, selection handler, position calculator, hexagon logic and unity interface board.
    /// </summary>
    private void SetLevel()
    {
        var rules = HexagonRule.Rules(colorNo);
        var moves = HexagonMove.Moves();
        touchMoves = new SelectionHandler(xTreeSize, yTreeSize);
        positionCalculator = new PositionCalculator(xTreeSize, yTreeSize);
        hexagonLogic = new HexagonLogic(xTreeSize, yTreeSize, colorNo, rules, moves);
        SetBoard();
        selectedCoords = null;
    }

    /// <summary>
    /// When a hexagon is clicked, deselect previous selected hexagons. Calculate grid coord position and available other two hexagon element to complete selection to three hexagon.
    /// Find their middle anchored coordination and return it.
    /// </summary>
    /// <param name="anchoredPositions"></param>
    /// <returns></returns>
    public Vector2 ClickedHexagons(Vector2 anchoredPositions)
    {
        DeselectHexagons();
        Vector2 coordPosition = positionCalculator.AnchoredToCoord(anchoredPositions);
        selectedCoords = touchMoves.GetSelectionPattern(coordPosition);
        SetSelectedHexagons();
        List<Vector2> selectedAnchored = new List<Vector2>();
        foreach (Vector2 vector in selectedCoords)
            selectedAnchored.Add(positionCalculator.CoordToAnchored(vector));
        Vector2 middleCoord = positionCalculator.SetMiddleCoord(selectedAnchored);
        return middleCoord;
    }

    /// <summary>
    /// Used for non player match element input.
    /// First set board untouchable. Send null match element and get explode event.
    /// If there wont be any explosion then we can set board touchable and return. If there will be any explosion then set these explosions.
    /// </summary>
    public void SetMatchElement()
    {
        SetBoardUnuouchable?.Invoke();
        MatchElement matchElement = new MatchElement();
        ExplodeEvent explodeEvent = hexagonLogic.Explode(matchElement);
        if (!explodeEvent.IsActionValid)
        {
            SetBoardTouchable?.Invoke();
            return;
        }
        SetExplosion(explodeEvent);
    }

    /// <summary>
    /// Player match element input.
    /// Set boarch untouchable. create match element with selected coords and send it to the logic to see if match request is available or not.
    /// With return explode event set bomb texts. If any bomb explodes then game over.
    /// If action is valid turn hexagon gameobjects, explode them and deselect previous selected coords.
    /// If action is not valid, calculate the destinations of all hexagon gameobject to make it rotation move.
    /// </summary>
    /// <param name="isTurnClockWise"></param>
    public void SetMacthElement(bool isTurnClockWise)
    {
        if (selectedCoords == null)
            return;
        SetBoardUnuouchable?.Invoke();

        MatchElement matchElement = new MatchElement(selectedCoords);

        ExplodeEvent explodeEvent = hexagonLogic.Explode(matchElement, isTurnClockwise: isTurnClockWise);

        SetBombTexts(explodeEvent);

        if (explodeEvent.bombEvent.isBombeExploded)
        {
            Debug.Log("Bomb exploded! Game Over!");
            GameOver?.Invoke();
        }

        if (explodeEvent.IsActionValid)
        {
            Debug.Log("Player action is valid. Explosion and fill events are starting.");
            ChangeElementsCoordsAndPosiitons(selectedCoords, explodeEvent.turnNo, isTurnClockWise);
            SetExplosion(explodeEvent);
            DeselectHexagons();
        }
        else
        {
            Debug.Log("Player action is not valid. Turn selected coords and return to their coords.");
            List<List<Vector2>> destinations = TurnClockwise(selectedCoords, 3, isTurnClockWise);
            TurnGridComponents(destinations);
            StopAllCoroutines();
            StartCoroutine(Move());
        }
    }

    /// <summary>
    /// Wait a little for move actions.
    /// Set boarc touchable again.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Move()
    {
        yield return new WaitForSeconds(1.5f);
        SetBoardTouchable?.Invoke();
        DeselectHexagons();
    }

    #region Selection & Position

    /// <summary>
    /// Use bold edge hexagon for selected hexagon gameobjects.
    /// </summary>
    private void SetSelectedHexagons()
    {
        if (selectedCoords == null)
            return;
        foreach (Vector2 coord in selectedCoords)
        {
            gridComponentHolder[coord].image.sprite = matchTreeUIDataHolder.SelectedHexagonSprite;
        }
    }

    /// <summary>
    /// Deselect selected hexagons and return their image to normal hexagon sprites.
    /// </summary>
    private void DeselectHexagons()
    {
        if (selectedCoords == null)
            return;
        foreach (Vector2 coord in selectedCoords)
        {
            if (!gridComponentHolder.ContainsKey(coord))
                continue;
            gridComponentHolder[coord].image.sprite = matchTreeUIDataHolder.HexagonSprite;
        }
        selectedCoords = null;
    }

    /// <summary>
    /// Find hexagon gameobjects destinations according to the turn direction and number.
    /// Turn hexagon gameobjects. Set their new coords according to the turn destinaions.
    /// </summary>
    /// <param name="coordPositions">Which coords to turn</param>
    /// <param name="turnNo">How many 120 degree turn needed</param>
    /// <param name="isTurnClockwise">Turn direciton</param>
    private void ChangeElementsCoordsAndPosiitons(List<Vector2> coordPositions, int turnNo, bool isTurnClockwise)
    {
        List<List<Vector2>> destinations = TurnClockwise(coordPositions, turnNo, isTurnClockwise);
        TurnGridComponents(destinations);
        SetNewCoords(coordPositions, destinations[destinations.Count - 1]);
    }

    /// <summary>
    /// Calculate new destinations of the coords according to number of turn and turn direction
    /// </summary>
    /// <param name="coords">Coords needed to be turned</param>
    /// <param name="numberofTurns">How many 120 degree turn needed</param>
    /// <param name="isTurnClockwise">Turn direciton</param>
    /// <returns></returns>
    private List<List<Vector2>> TurnClockwise(List<Vector2> coords, int numberofTurns, bool isTurnClockwise)
    {
        List<List<Vector2>> destinations = new List<List<Vector2>>();
        for (int i = 0; i < numberofTurns; i++)
        {
            List<Vector2> initialCoords = new List<Vector2>();
            foreach (Vector2 coord in coords)
                initialCoords.Add(coord);
            int firstIndex = isTurnClockwise ? 2 : 0;
            int lastIndex = isTurnClockwise ? 0 : 2;
            for (int j = 0; j < i + 1; j++)
            {
                Vector2 initial = initialCoords[firstIndex];
                initialCoords[firstIndex] = initialCoords[1];
                initialCoords[1] = initialCoords[lastIndex];
                initialCoords[lastIndex] = initial;
            }
            destinations.Add(initialCoords);
        }
        return destinations;
    }

    /// <summary>
    /// Turn grid components to specified locations in order.
    /// </summary>
    /// <param name="destinations"></param>
    private void TurnGridComponents(List<List<Vector2>> destinations)
    {
        for (int i = 0; i < selectedCoords.Count; i++)
        {
            List<Vector2> moveToAnchored = new List<Vector2>();
            foreach (List<Vector2> destination in destinations)
            {
                moveToAnchored.Add(positionCalculator.CoordToAnchored(destination[i]));
            }
            gridComponentHolder[selectedCoords[i]].move.CreateMoveToCoroutine(moveToAnchored);
        }
    }

    /// <summary>
    /// Set new coords of the hexagon gameobjcts in the initialcoords.
    /// </summary>
    /// <param name="initialCoordPositions">Which hexagon gameobjects need new coords</param>
    /// <param name="lastCoords">Destinaiton coords</param>
    private void SetNewCoords(List<Vector2> initialCoordPositions, List<Vector2> lastCoords)
    {
        List<ItemComponents> itemComponents = new List<ItemComponents>();
        for (int i = 0; i < initialCoordPositions.Count; i++)
            itemComponents.Add(gridComponentHolder[initialCoordPositions[i]]);
        for (int i = 0; i < initialCoordPositions.Count; i++)
            gridComponentHolder[lastCoords[i]] = itemComponents[i];
    }

    #endregion


    #region Explosion

    /// <summary>
    /// Start explosion coroutine.
    /// </summary>
    /// <param name="explodeEvent">Explode event</param>
    private void SetExplosion(ExplodeEvent explodeEvent)
    {
        StopAllCoroutines();
        StartCoroutine(Explode(explodeEvent));
    }

    /// <summary>
    /// Explodes all the explode element in the explode event, add explodede gameobjects to object pool to reuse them.
    /// Invoke increase score after all explosions and start fill event for vacancies of exploded gameobjects.
    /// </summary>
    /// <param name="explodeEvent"></param>
    /// <returns></returns>
    private IEnumerator Explode(ExplodeEvent explodeEvent)
    {
        yield return new WaitForSeconds(0.3f);
        foreach (ExplodeElement explodeElement in explodeEvent.explodeElements)
        {
            foreach (Vector2 coord in explodeElement.positions)
            {
                gridComponentHolder[coord].explode.SetExplode();
                itemComponentsPool.Add(gridComponentHolder[coord]);
                gridComponentHolder[coord] = null;
            }
        }
        yield return new WaitForSeconds(1f);
        IncreaseScore?.Invoke(hexagonLogic.GetCurrentScore());
        FillEvent fillEvent = hexagonLogic.Fill();
        SetFill(fillEvent);
        yield return null;
    }


    #endregion

    #region Fill

    /// <summary>
    /// Start fill coroutine.
    /// </summary>
    /// <param name="fillEvent"></param>
    private void SetFill(FillEvent fillEvent)
    {
        StopAllCoroutines();
        StartCoroutine(Fill(fillEvent));
    }

    /// <summary>
    /// Fill all the element with fill event. If To = from then hexagon element is new instantiated. Use object pooling.
    /// If To != from then move hexagon gameobject from location to to location.
    /// Check if there is any move left for player to play. If there is not, then game is over.
    /// If there is more moves left, set non player match elkements to see if there is any match to ready to expldoe.
    /// </summary>
    /// <param name="fillEvent"></param>
    /// <returns></returns>
    private IEnumerator Fill(FillEvent fillEvent)
    {

        foreach (List<MoveElement> list in fillEvent.fillLists)
        {
            foreach (MoveElement moveElemnt in list)
            {

                if (moveElemnt.to == moveElemnt.from)
                {
                    SetNewType(itemComponentsPool[0], moveElemnt.to, moveElemnt.hexagonElement.matchType, moveElemnt.hexagonElement.bombInfo);
                    if (!gridComponentHolder.ContainsKey(moveElemnt.to))
                        gridComponentHolder.Add(moveElemnt.to, itemComponentsPool[0]);
                    else
                        gridComponentHolder[moveElemnt.to] = itemComponentsPool[0];
                    itemComponentsPool.RemoveAt(0);
                }
                else
                {
                    gridComponentHolder[moveElemnt.from].move.CreateMoveToCoroutine(positionCalculator.CoordToAnchored(moveElemnt.to));
                    gridComponentHolder[moveElemnt.to] = gridComponentHolder[moveElemnt.from];
                    gridComponentHolder[moveElemnt.from] = null;
                    
                }
            }
            yield return new WaitForSeconds(0.05f);
        }

        // Sets null match element to get successive explode events.
        if (fillEvent.leftMove)
        {
            SetMatchElement();
        }
        else
        {
            Debug.Log("No move left move! Game over.");
            GameOver?.Invoke();
        }
        yield return null;
    }

    #region Unity Interface Class Reference Methods

    /// <summary>
    /// For every grid element, create hexagon gameobjects from prefab. Set its coordinats, add its components to gridComponentHolder and set all its values.
    /// </summary>
    private void SetBoard()
    {
        for (int x = 0; x < xTreeSize; x++)
        {
            for (int y = 0; y < yTreeSize; y++)
            {
                if (hexagonLogic.GetGridValue(x, y) != null)
                {
                    GameObject gameObject = Instantiate(matchTreeUIDataHolder.HexagonPrefab, matchTreeSpritesRectTransforms);
                    Vector2 coordPosition = new Vector2(x, y);
                    gridComponentHolder.Add(coordPosition, new ItemComponents(gameObject));
                    SetNewType(gridComponentHolder[coordPosition], coordPosition, (MatchType)hexagonLogic.GetGridValue(x, y));
                }
                else
                {
                    Debug.LogError(x + "," + y + "is null!");
                }
            }
        }
        Debug.Log("Board is initialized.");
    }

    /// <summary>
    /// Set all hexagon element values to the hexagon gameobject components and activate gameobject.
    /// </summary>
    /// <param name="itemComponents"></param>
    /// <param name="toPosition"></param>
    /// <param name="matchType"></param>
    /// <param name="bombInfo"></param>
    private void SetNewType(ItemComponents itemComponents, Vector2 toPosition, MatchType matchType, BombInfo bombInfo = new BombInfo())
    {
        itemComponents.explode.StopAllCoroutines();
        itemComponents.touch.matchType = matchType;
        itemComponents.move.SetTransform(positionCalculator.CoordToAnchored(toPosition));

        if (bombInfo.hasBomb)
            Debug.Log("One bomb is dropped to the board!");
        SetUIValues(itemComponents, matchType, bombInfo);
        itemComponents.gameObject.SetActive(true);
    }

    /// <summary>
    /// Set UI components of the hexagon gameobjects like hesagon sprite, hexagon color, bomb text etc.
    /// </summary>
    /// <param name="itemComponents"></param>
    /// <param name="matchType"></param>
    /// <param name="bombInfo"></param>
    private void SetUIValues(ItemComponents itemComponents, MatchType matchType, BombInfo bombInfo)
    {
        if (itemComponents == null)
            return;
        Image image = itemComponents.image;
        image.sprite = matchTreeUIDataHolder.HexagonSprite;
        image.color = matchTreeUIDataHolder.SpriteColors[(int)matchType - 1];
        if (bombInfo.hasBomb)
        {
            ActivateBombText(bombInfo, itemComponents);
        }
        else
        {
            Text text = itemComponents.text;
            text.enabled = false;
        }
    }

    /// <summary>
    /// Set bomb texts of the all bombs in the scene.
    /// </summary>
    /// <param name="explodeEvent"></param>
    private void SetBombTexts(ExplodeEvent explodeEvent)
    {
        if (explodeEvent.bombEvent.bombElements.Count > 0)
            Debug.Log("There is at least one bomb in the scene and its timer is running out!");
        foreach (BombElement element in explodeEvent.bombEvent.bombElements)
        {
            ActivateBombText(element.bombInfo, gridComponentHolder[element.coords]);
        }
    }

    /// <summary>
    /// Activate bomb text of the hexagon gameobject and set its bomb timer value to the text,
    /// </summary>
    /// <param name="bombInfo"></param>
    /// <param name="itemComponents"></param>
    private void ActivateBombText(BombInfo bombInfo, ItemComponents itemComponents)
    {
        Text text = itemComponents.text;
        text.enabled = true;
        text.text = bombInfo.bombLeftMove.ToString();
        text.color = Color.black;
    }

    #endregion

    #endregion
}

/// <summary>
/// Calculate the relationship between grid coords and anchored coords of the board.
/// </summary>
public struct PositionCalculator
{
    private int xTreeSize;
    private int yTreeSize;
    private Vector2 canvasReferenceResolution;
    public readonly Vector2 anchorOffsetToScreenCenter;

    public PositionCalculator(int xTreeSize, int yTreeSize)
    {
        this.xTreeSize = xTreeSize;
        this.yTreeSize = yTreeSize;
        this.anchorOffsetToScreenCenter = Vector2.zero;
        this.canvasReferenceResolution = GameObject.Find("Canvas").GetComponent<CanvasScaler>().referenceResolution;
        this.anchorOffsetToScreenCenter = SetAnchorOffsetToScreenCenter();
    }

    /// <summary>
    /// Turn grid coordination to the anchored coords.
    /// </summary>
    /// <param name="coord">Grid coord</param>
    /// <returns>Anchored coord</returns>
    public Vector2 CoordToAnchored(Vector2 coord)
    {
        return new Vector2(coord.x * 120 + this.anchorOffsetToScreenCenter.x, coord.y * 120 + this.anchorOffsetToScreenCenter.y + ((int)coord.x % 2 == 1 ? -60 : 0));
    }

    /// <summary>
    /// Turn anchored coord to grid coord.
    /// </summary>
    /// <param name="position">Anchored coords</param>
    /// <returns>Grid coord</returns>
    public Vector2 AnchoredToCoord(Vector2 position)
    {
        float y = (position.y - this.anchorOffsetToScreenCenter.y) / 120;
        float x = (position.x - this.anchorOffsetToScreenCenter.x) / 120;
        y += 0.5f;
        x += 0.5f;
        return new Vector2((int)x, (int)y);
    }

    /// <summary>
    /// Return middle point of the all points.
    /// </summary>
    /// <param name="selectedAnchored">Points</param>
    /// <returns>middle point</returns>
    public Vector2 SetMiddleCoord(List<Vector2> selectedAnchored)
    { 
        Vector2 middleCoord = new Vector2((selectedAnchored[0].x + selectedAnchored[1].x + selectedAnchored[2].x) / 3, (selectedAnchored[0].y + selectedAnchored[1].y + selectedAnchored[2].y) / 3);
        middleCoord += new Vector2(this.canvasReferenceResolution.x / 2, this.canvasReferenceResolution.y / 2);
        return middleCoord;
    }

    /// <summary>
    /// Set acnhored offset to screen center for ready the board to the different grid widths and heights.
    /// </summary>
    /// <returns></returns>
    private Vector2 SetAnchorOffsetToScreenCenter()
    {
        float xOffset = this.xTreeSize % 2 == 0 ? (this.xTreeSize / 2) * (-120) + 60 : this.xTreeSize / 2 * (-120);
        float yOffset = this.yTreeSize % 2 == 0 ? (this.yTreeSize / 2) * (-120) : this.yTreeSize / 2 * (-120);
        yOffset -= 150;
        return new Vector2(xOffset, yOffset);
    }

}

public class ItemComponents
{
    public GameObject gameObject;
    public RectTransform rectTransform;
    public Move move;
    public Explode explode;
    public Image image;
    public MatchTreeTocuh touch;
    public Text text;
    public ItemComponents(GameObject gameObject)
    {
        this.gameObject = gameObject;
        this.rectTransform = gameObject.GetComponent<RectTransform>();
        this.move = gameObject.GetComponent<Move>();
        this.explode = gameObject.GetComponent<Explode>();
        this.image = gameObject.GetComponent<Image>();
        this.touch = gameObject.GetComponent<MatchTreeTocuh>();
        this.text = gameObject.GetComponentInChildren<Text>();
        text.enabled = false;
    }
}