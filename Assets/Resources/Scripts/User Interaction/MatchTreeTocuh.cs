using UnityEngine;
using UnityEngine.EventSystems;

public class MatchTreeTocuh : MonoBehaviour, IPointerClickHandler ,IBeginDragHandler, IDragHandler
{
    public MatchType matchType = MatchType.empty;
    private bool canTouch = true;
    private RectTransform rectTransform;
    private Vector2 middlePoint;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        HexagonController.SetBoardTouchable += SetTouchable;
        HexagonController.SetBoardUnuouchable += SetUntouchable;
    }

    private void OnDisable()
    {
        HexagonController.SetBoardTouchable -= SetTouchable;
        HexagonController.SetBoardUnuouchable -= SetUntouchable;
    }

    /// <summary>
    /// Selects the three hexagon element and return their mid point to use at rotation calculation
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canTouch)
            return;
        middlePoint = HexagonController.instance.ClickedHexagons(rectTransform.anchoredPosition);
    }

    /// <summary>
    /// Calculates rotation direction and send it to the hexagon controller to set match element
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canTouch)
            return;
        StopAllCoroutines();
        bool isTurnClockwise = IsRotationClockWise(eventData.pressPosition, eventData.position);
        HexagonController.instance.SetMacthElement(isTurnClockwise); // Send match element to the hexagon controller
    }

    public void OnDrag(PointerEventData eventData)
    {
    }


    /// <summary>
    /// Find movement rotation by taking absolute values of x and y with respect to current and last press posiiton.
    /// </summary>
    /// <param name="pressPosition"></param>
    /// <param name="currentPosition"></param>
    /// <returns>Is rotation clockwise?</returns>
    private bool IsRotationClockWise(Vector2 pressPosition, Vector2 currentPosition) // Is clockwise
    {
        if (Mathf.Abs(pressPosition.x - currentPosition.x) > Mathf.Abs(pressPosition.y - currentPosition.y))
        {
            if (pressPosition.x - currentPosition.x > 0) // Drag to left
            {
                if (middlePoint.y < pressPosition.y ) // Finger is above the selected hexagons
                    return true; // So rotation direction is clockwise
                else // Finger is below the the selected hexagons
                    return false; // So rotation direction is counterclockwise
            }
            else // Drag to right
            {
                if (middlePoint.y < pressPosition.y) // Finger is above the selected hexagons
                    return false; // So rotation direction is counterclockwise
                else // Finger is below the the selected hexagons
                    return true; // So rotation direction is clockwise
            }
        }
        else
        {
            if (pressPosition.y - currentPosition.y > 0) // Drag to down
            {
                if (middlePoint.x < pressPosition.x) // Finger is right the selected hexagons
                    return false; // So rotation direction is counterclockwise
                else // Finger is left the selected hexagons
                    return true; // So rotation direction is cclockwise
            }
            else // Drag to up
            {
                if (middlePoint.x < pressPosition.x) // Finger is right the selected hexagons
                    return true; // So rotation direction is clockwise
                else // Finger is left the selected hexagons
                    return false; // So rotation direction is counterclockwise
            }
        }
    }


    private void SetTouchable() => canTouch = true;

    private void SetUntouchable() => canTouch = false;

}
