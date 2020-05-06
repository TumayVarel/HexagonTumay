using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Explode : MonoBehaviour
{
    public float animationSpeed = 0.25f;

    private RectTransform rectTransform;
    private Image image;
    private Text text;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        text = GetComponentInChildren<Text>();
    }

    private void OnEnable()
    {
        HexagonController.GameOver += StopAllCoroutines;
    }

    private void OnDisable()
    {
        HexagonController.GameOver -= StopAllCoroutines;
    }


    public void SetExplode()
    {
        StopAllCoroutines();
        StartCoroutine(TillCenterExplode());
    }

    /// <summary>
    /// Fade outs the color of the image and text. 
    /// </summary>
    private IEnumerator TillCenterExplode()
    {
        float time = 0;
        while (Mathf.Abs(image.color.a - 0) > 0.0001f)
        {
            time += Time.deltaTime;
            image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(image.color.a, 0, time * animationSpeed));
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(text.color.a, 0, time * animationSpeed));
            yield return new WaitForEndOfFrame();
        }
    }


}
