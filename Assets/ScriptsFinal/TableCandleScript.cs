using System.Collections.Generic;
using UnityEngine;

public class TableCandleScript : MonoBehaviour
{
    [Header("Candle Objects")]
    public List<GameObject> candleList = new List<GameObject>();

    [Header("Chances (0-1)")]
    [Range(0f, 1f)] public float chanceCandle = 0.5f;
    [Range(0f, 1f)] public float chanceCandleMultiple = 0.5f;
    [Range(0f, 1f)] public float chanceCandleEach = 0.5f;

    // Call this function whenever you want to trigger the logic
    public void ProcessCandles()
    {
        // If main chance fails → nothing happens
        if (Random.value > chanceCandle)
            return;

        // If multiple candles
        if (Random.value < chanceCandleMultiple)
        {
            foreach (GameObject candle in candleList)
            {
                if (Random.value < chanceCandleEach)
                    candle.SetActive(!candle.activeSelf);
            }
        }
        else
        {
            // Pick one random candle if list is not empty
            if (candleList.Count > 0)
            {
                int index = Random.Range(0, candleList.Count);
                candleList[index].SetActive(!candleList[index].activeSelf);
            }
        }
    }

    void Start()
    {
        ProcessCandles();
    }
}
