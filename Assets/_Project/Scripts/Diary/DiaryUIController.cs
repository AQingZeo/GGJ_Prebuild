using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Collections.Generic;
using GameContracts;

public class DiaryUIController : MonoBehaviour
{
    public static DiaryUIController Instance;

    [Header("UI ÒýÓÃ")]
    public TextMeshProUGUI contentText;
    public Image displayImage; 
    public GameObject nextButton;
    public GameObject prevButton;

    public List<Sprite> diaryImagePages = new List<Sprite>();
    private int currentIndex = 0;

    private void Awake() { Instance = this; }

    public void ShowDiaryText(List<string> textPages)
    {
        
    }
    public void NextPage() { if (currentIndex < diaryImagePages.Count - 1) { currentIndex++; UpdatePage(); } }
    public void PrevPage() { if (currentIndex > 0) { currentIndex--; UpdatePage(); } }

    void UpdatePage()
    {
        if (displayImage && diaryImagePages.Count > 0)
        {
            displayImage.sprite = diaryImagePages[currentIndex];
        }

        if (nextButton) nextButton.SetActive(currentIndex < diaryImagePages.Count - 1);
        if (prevButton) prevButton.SetActive(currentIndex > 0);
    }

    public void CloseDiary()
    {
        gameObject.SetActive(false);
        if (GameStateMachine.Instance != null) GameStateMachine.Instance.SetState(GameState.Explore);
    }
}