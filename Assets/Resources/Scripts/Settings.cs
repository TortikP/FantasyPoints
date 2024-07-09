using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Settings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown; 
    Resolution[] resolutions;
    // Start is called before the first frame update
    void Start()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        string option;
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;
        for (int i = 0;i<resolutions.Length;i++)
        {
            option = resolutions[i].width + "x" + resolutions[i].height + " " + resolutions[i].refreshRateRatio + "Hz";
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
        LoadSettings(currentResolutionIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen; 
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("QualitySettingsPreference", qualityDropdown.value);
        PlayerPrefs.SetInt("ResolutionSettingsPreference", resolutionDropdown.value);
        PlayerPrefs.SetInt("FullscreenSettingsPreference", System.Convert.ToInt32(Screen.fullScreen));
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        if (PlayerPrefs.HasKey("QualitySettingsPreference"))
        {
            qualityDropdown.value = PlayerPrefs.GetInt("QualitySettingsPreference");
        }
        else
        {
            qualityDropdown.value = 3;
        }
        if (PlayerPrefs.HasKey("ResolutionSettingsPreference"))
        {
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionSettingsPreference");
        }
        else
        {
            resolutionDropdown.value = currentResolutionIndex;
        }
        if (PlayerPrefs.HasKey("FullscreenSettingsPreference"))
        {
            Screen.fullScreen = System.Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenSettingsPreference"));
        }
        else
        {
            Screen.fullScreen = true;
        }
    }
}
