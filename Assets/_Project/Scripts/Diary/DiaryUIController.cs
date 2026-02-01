using UnityEngine;
using TMPro;
using System.Collections.Generic; 
using GameContracts; 

public class DiaryUIController : MonoBehaviour
{
    public static DiaryUIController Instance;

    [Header("UI 引用")]
    public TextMeshProUGUI contentText;
    public GameObject nextButton;
    public GameObject prevButton;

    private List<string> pages = new List<string>();
    private int currentIndex = 0;

    private void Awake() { Instance = this; }

    public void ShowDiary(List<string> diaryPages)
    {
        pages = diaryPages;
        currentIndex = 0;
        UpdatePage();
    }

    public void NextPage() { if (currentIndex < pages.Count - 1) { currentIndex++; UpdatePage(); } }
    public void PrevPage() { if (currentIndex > 0) { currentIndex--; UpdatePage(); } }

    void UpdatePage()
    {
        if (contentText) contentText.text = pages[currentIndex];
        if (nextButton) nextButton.SetActive(currentIndex < pages.Count - 1);
        if (prevButton) prevButton.SetActive(currentIndex > 0);
    }

    public void CloseDiary()
    {
        try
        {
            if (GameStateMachine.Instance != null)
            {
                GameStateMachine.Instance.SetState(GameState.Explore);
                return; 
            }
        }
        catch { /* 忽略错误 */ }

        gameObject.SetActive(false);
        Debug.Log("找不到状态机，已执行强制隐藏。");
    }
}