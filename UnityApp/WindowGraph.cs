using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using CodeMonkey.Utils;
using Vector3 = UnityEngine.Vector3;

public class WindowGraph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    private ThingSpeakFetcher thingSpeakFetcher;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;

    public float yMax = 100f;
    public float yMin = -100f;
    public float xRange = 50f;
    public int fieldToRead = 1;

    void Awake()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();

        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();


        thingSpeakFetcher = gameObject.GetComponent<ThingSpeakFetcher>();
        if (thingSpeakFetcher != null)
        {
            thingSpeakFetcher.FetchData(fieldIndex: fieldToRead);
            thingSpeakFetcher.StartCoroutine(FetchDataAndShowGraph());
        }


    }

    private IEnumerator FetchDataAndShowGraph()
    {
        // Wait for the data to be fetched from ThingSpeak
        while (thingSpeakFetcher.dataValuesTemp.Count == 0)
        {
            yield return null;
        }

        // Show the graph using the fetched data
        ShowGraph(thingSpeakFetcher.dataValuesTemp);
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(10, 10);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    /// <summary>
    /// Show the graph and the labels dynamically according to the canvas size.
    /// </summary>
    /// <param name="valueList"></param>
    private void ShowGraph(List<float> valueList)
    {   

        // boundaries of the graph
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = yMax;
        float yMinimum = yMin;
        float xSize = xRange;

        GameObject lastCircleGameObject = null;

        // sets the position of the circles and the lines between them
        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = i * xSize;

            float yPosition = (valueList[i] - yMinimum) / (yMaximum - yMinimum) * graphHeight;
            if (yPosition >= graphHeight)
            {
                yPosition = graphHeight;
            }
            else if (yPosition <= 0)
            {
                yPosition = 0;
            }


            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));

            // if there is a previous circle, create a line between them
            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }

            lastCircleGameObject = circleGameObject;

            // set the labels of the graph
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer);
            labelX.gameObject.SetActive(true);

            // set the position of the label
            labelX.anchoredPosition = new Vector2(xPosition, -20f);
            labelX.GetComponent<TMP_Text>().text = i.ToString();
        }

        int separatorCount = 10; // number of vertical separators in the graph
        
        // set seperators and labels for the y axis
        for (int i = 0; i < separatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer);
            labelY.gameObject.SetActive(true);

            float normalizedValue = i *1f / separatorCount;
            labelY.anchoredPosition = new Vector2(-30f, normalizedValue * graphHeight);
            labelY.GetComponent<TMP_Text>().text = normalizedValue * 100f + "%";
        }
    }

    /// <summary>
    /// Create a line between two circles.
    /// </summary>
    /// <param name="dotPositionA"></param>
    /// <param name="dotPositionB"></param>
    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB) {
        
        // creates a circle.
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);


        // set the color of the line
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        
        // set the direction and the lengtg of the line
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        // set the position of the line
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir)); // rotate the line according to the direction


    }

}
