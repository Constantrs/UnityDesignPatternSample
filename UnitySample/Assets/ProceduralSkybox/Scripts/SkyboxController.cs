using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyboxController : MonoBehaviour
{
    public bool updateFlag;
    public Camera mainCamera;
    //public Light light;
    public Transform lightTransform;
    public Transform moonTransform;
    public Material skyboxmaterial;

    public Image uiGauge;
    public Image uiDayNightIcon;
    public Sprite sunSprite;
    public Sprite moonSprite;

    public float startFrame = 0.0f;
    public float halfDayFrame = 0.0f;
    private bool _IsNight = true;
    private float _Timer = 0.0f;
    private float _RotateSpeed = 0.0f;

    private void OnValidate()
    {
        UpdateSkybox();
        Debug.Log("ChangeValue!");
    }

    // Start is called before the first frame update
    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    private void Start()
    {
        _IsNight = true;
        _RotateSpeed = -(1.0f / halfDayFrame) * 180.0f;
        _Timer = startFrame;
    }

    // Update is called once per frame
    private void Update()
    {
        if(updateFlag)
        {
            UpdateDayNight();
            UpdateSkybox();
        }
    }
    private void UpdateDayNight()
    {
        if (skyboxmaterial && lightTransform)
        {
            lightTransform.Rotate(Vector3.right, _RotateSpeed);
            moonTransform.Rotate(Vector3.right, _RotateSpeed);
            if (_Timer < halfDayFrame)
            {
                _Timer += Time.timeScale;
                uiGauge.fillAmount = Mathf.Lerp(0.0f, 1.0f, _Timer / halfDayFrame);
            }
            else
            {
                _IsNight = !_IsNight;
                uiDayNightIcon.sprite = (_IsNight) ? moonSprite : sunSprite;
                uiGauge.fillAmount = 0.0f;
                _Timer = 0.0f;
            }
        }
    }

    private void UpdateSkybox()
    {
        if (skyboxmaterial)
        {
            if(lightTransform)
            {
                // Sun
                skyboxmaterial.SetVector("_SunDir", -lightTransform.forward);
                //skyboxmaterial.SetVector("_MoonDir", lightTransform.forward);
            }

            if (moonTransform)
            {
                // Moon
                skyboxmaterial.SetVector("_MoonDir", moonTransform.up);
            }
        }
    }
}
