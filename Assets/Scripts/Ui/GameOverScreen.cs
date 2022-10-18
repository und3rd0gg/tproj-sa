using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private List<Star> Stars;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TMP_Text _totalBricksView;

    private readonly Dictionary<int, int> ResultMap = new()
    {
        [0] = 1,
        [40] = 2,
        [85] = 3
    };

    public void Activate(int suckedBricksPercent, int suckedBricksTotal)
    {
        gameObject.SetActive(true);
        var starsCount = CalculateStarsCount(suckedBricksPercent);
        ActivateStars(starsCount);
        SetCompletionInfo(suckedBricksTotal);
    }

    private void SetCompletionInfo(int suckedBricksTotal)
    {
        var bricksCount = FindObjectOfType<BricksRoot>().BricksCount;
        //_totalBricksView.text = $"{suckedBricksTotal}/{bricksCount}";
        _totalBricksView.text = $"{suckedBricksTotal}";
        _progressSlider.value = (float) suckedBricksTotal / bricksCount;
    }

    private void ActivateStars(int starsCount)
    {
        for (var i = 0; i < starsCount; i++)
        {
            Stars[i].StarView.gameObject.SetActive(true);
            Stars[i].FX.gameObject.SetActive(true);
        }
    }

    private int CalculateStarsCount(int suckedBricksPercent)
    {
        var starsCount = 0;

        foreach (var result in ResultMap.Where(result => suckedBricksPercent > result.Key)) starsCount = result.Value;

        return starsCount;
    }
}

[Serializable]
public struct Star
{
    [field: SerializeField] public GameObject StarView { get; private set; }
    [field: SerializeField] public GameObject FX { get; private set; }
}