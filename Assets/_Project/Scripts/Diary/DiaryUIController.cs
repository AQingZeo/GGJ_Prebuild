using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GameContracts; // 确保你的项目中存在这个命名空间，用于切换游戏状态

public class DiaryUIController : MonoBehaviour
{
    [Header("UI 引用")]
    public Image displayImage;      // 拖入显示日记内容的 Image 物体（那张黄纸）
    public GameObject nextButton;   // 拖入右箭头按钮
    public GameObject prevButton;   // 拖入左箭头按钮
    public GameObject closeButton;  // 拖入关闭红叉按钮

    [Header("日记内容配置")]
    public List<Sprite> diaryImagePages = new List<Sprite>(); // 拖入那 7 张日记图片

    private int currentIndex = 0;

    private void Start()
    {
        // 1. 基础防错检查
        if (diaryImagePages == null || diaryImagePages.Count == 0)
        {
            Debug.LogError("【日记系统】错误：图片列表为空！请在 Inspector 面板中拖入图片。");
            return;
        }

        if (displayImage == null)
        {
            Debug.LogError("【日记系统】错误：未关联 Display Image 组件！");
            return;
        }

        // 2. 初始化显示第一页
        currentIndex = 0;
        UpdatePage();
    }

    // 下一页：绑定到 NextButton 的 OnClick
    public void NextPage()
    {
        if (currentIndex < diaryImagePages.Count - 1)
        {
            currentIndex++;
            UpdatePage();
        }
    }

    // 上一页：绑定到 PrevButton 的 OnClick
    public void PrevPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdatePage();
        }
    }

    // 关闭日记：绑定到 CloseButton 的 OnClick
    public void CloseDiary()
    {
        // 隐藏整个日记面板
        this.gameObject.SetActive(false);

        // 切换回探索模式状态（防止玩家视角被锁死）
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.SetState(GameState.Explore);
        }

        Debug.Log("日记已关闭，回到探索状态。");
    }

    // 核心显示逻辑
    private void UpdatePage()
    {
        // 更新图片显示
        displayImage.sprite = diaryImagePages[currentIndex];

        // 按钮显隐逻辑：
        // 第一页不显示 Prev，最后一页不显示 Next
        if (nextButton) nextButton.SetActive(currentIndex < diaryImagePages.Count - 1);
        if (prevButton) prevButton.SetActive(currentIndex > 0);

        // 确保关闭按钮始终存在
        if (closeButton) closeButton.SetActive(true);

        Debug.Log($"当前页码: {currentIndex + 1} / {diaryImagePages.Count}");
    }
}