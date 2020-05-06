using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float animationRate = 0.5f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        HexagonController.GameOver += StopAllCoroutines;
    }

    private void OnDisable()
    {
        HexagonController.GameOver -= StopAllCoroutines;
    }

    /// <summary>
    /// Start move coroutine for all destinations in order.
    /// </summary>
    /// <param name="toPosition">All anchored destinations</param>
    public void CreateMoveToCoroutine(List<Vector2> toPositionList)
    {
        StopAllCoroutines();
        StartCoroutine(MoveToTransform(toPositionList));
    }

    /// <summary>
    /// Start coroutine to the destination
    /// </summary>
    /// <param name="toPosition">Destination anchored position</param>
    public void CreateMoveToCoroutine(Vector2 toPosition)
    {
        List<Vector2> toPositionList = new List<Vector2>() { toPosition };
        StopAllCoroutines();
        StartCoroutine(MoveToTransform(toPositionList));
    }

    /// <summary>
    /// Set transform directly.
    /// </summary>
    /// <param name="toPosition">Anchored position</param>
    public void SetTransform(Vector2 toPosition)
    {

        rectTransform.anchoredPosition = toPosition;
        rectTransform.localScale = Vector2.one;
    }

    /// <summary>
    /// Move position to to positions with lerp funcion in order.
    /// </summary>
    /// <param name="toPosition">All destinaitons</param>
    /// <returns></returns>
    private IEnumerator MoveToTransform(List<Vector2> toPosition)
    {
        foreach(Vector2 destination in toPosition)
        {
            float time = 0;
            while (Vector2.Distance(rectTransform.anchoredPosition, destination) > 0.001f)
            {
                time += Time.deltaTime;
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, destination, time * animationRate);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(0.2f);
        }


    }

}
