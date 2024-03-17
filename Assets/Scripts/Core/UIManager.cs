using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>
{
    [Header("-----MenuCanvas-----")]
    public GameObject menuCanvas;
    public Button startBtn;

    [Header("-----GameCanvas-----")]
    public GameObject gameCanvas;
    public Slider waterBar;
    public TMP_Text divedMeterText;


    private void Start()
    {
        BindUI();
    }

    private void BindUI()
    {
        startBtn.onClick.RemoveAllListeners();
        startBtn.onClick.AddListener(OnGameStartClicked);
    }

    private void OnGameStartClicked()
    {
        ShowMenu(false);
        ShowGame(true);
        GameManager.Instance.StartGame();
    }

    public void RefreshWater(float currWater, float maxWater)
    {
        waterBar.value = currWater/maxWater;
    }

    public void RefreshDivedMeterText(float currDepth)
    {
        divedMeterText.text = currDepth.ToString("F1");
    }

    public void ShowMenu(bool active)
    {
        menuCanvas.SetActive(active);
    }

    public void ShowGame(bool active)
    {
        gameCanvas.SetActive(active);   
    }
}
