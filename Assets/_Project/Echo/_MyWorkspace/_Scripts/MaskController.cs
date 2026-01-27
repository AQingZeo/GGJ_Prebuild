using UnityEngine;

public class MaskController_Multi : MonoBehaviour
{
    public Material maskMaterial;

    // 记录三个面具的开关状态，默认都是关着的 (false)
    private bool redOn = false;
    private bool greenOn = false;
    private bool blueOn = false;
    void Start()
    {
        UpdateShader();
    }

    void Update()
    {
        // 按下 1：切换红色开关
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            redOn = !redOn; 
            UpdateShader();
        }

        // 按下 2：切换绿色开关
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            greenOn = !greenOn;
            UpdateShader();
        }

        // 按下 3：切换蓝色开关
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            blueOn = !blueOn;
            UpdateShader();
        }

        // 按下 0：一键全部关闭
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            redOn = greenOn = blueOn = false;
            UpdateShader();
        }
    }

    void UpdateShader()
    {
        if (maskMaterial == null) return;

        maskMaterial.SetFloat("_RedMask", redOn ? 1.0f : 0.0f);
        maskMaterial.SetFloat("_GreenMask", greenOn ? 1.0f : 0.0f);
        maskMaterial.SetFloat("_BlueMask", blueOn ? 1.0f : 0.0f);

        Debug.Log($"当前状态：红({redOn}) 绿({greenOn}) 蓝({blueOn})");
    }
}