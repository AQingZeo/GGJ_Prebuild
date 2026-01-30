using UnityEngine;

public class MaskController : MonoBehaviour
{
    [Header("--- 视觉设置 ---")]
    public Material targetMaterial; 
    public float switchSpeed = 5f;  

    [Header("--- Zeo 物品 ID 配置 ---")]

    public string redMaskID = "Mask_Red";
    public string blueMaskID = "Mask_Blue";
    public string yellowMaskID = "Mask_Yellow";

    private InventoryService _inventoryService; 
    private bool _isRedActive;
    private bool _isBlueActive;
    private bool _isYellowActive;

    private float _curRed, _curBlue, _curYellow;

    public void Initialize(InventoryService service)
    {
        _inventoryService = service;
        Debug.Log("MaskController: 已连接到 InventoryService");
    }

    void Update()
    {
        if (_inventoryService == null) return; 

        HandleInput();
        UpdateShader();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && _inventoryService.HasItem(redMaskID))
            _isRedActive = !_isRedActive;

        if (Input.GetKeyDown(KeyCode.Alpha2) && _inventoryService.HasItem(blueMaskID))
            _isBlueActive = !_isBlueActive;

        if (Input.GetKeyDown(KeyCode.Alpha3) && _inventoryService.HasItem(yellowMaskID))
            _isYellowActive = !_isYellowActive;
    }

    void UpdateShader()
    {
        if (targetMaterial == null) return;

        float targetR = _isRedActive ? 1f : 0f;
        float targetG = _isBlueActive ? 1f : 0f;
        float targetB = _isYellowActive ? 1f : 0f;

        _curRed = Mathf.MoveTowards(_curRed, targetR, Time.deltaTime * switchSpeed);
        _curBlue = Mathf.MoveTowards(_curBlue, targetG, Time.deltaTime * switchSpeed);
        _curYellow = Mathf.MoveTowards(_curYellow, targetB, Time.deltaTime * switchSpeed);

        targetMaterial.SetFloat("_RedWeight", Mathf.Clamp01(_curRed));
        targetMaterial.SetFloat("_BlueWeight", Mathf.Clamp01(_curBlue));
        targetMaterial.SetFloat("_YellowWeight", Mathf.Clamp01(_curYellow));
    }
}