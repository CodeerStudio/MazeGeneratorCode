using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour {
    
    //display
    [SerializeField] private Dropdown resolutionsDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    
    //audio
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private AudioMixer musicAudioMixer;
    
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private AudioMixer sfxAudioMixer;
    
    //controls
    [SerializeField] private Slider camSensitivitySlider;
    
    //colour scheme
    [SerializeField] private Slider colourSlider;
    [SerializeField] private Material sky;
    [SerializeField] private Material floor;
    
    private Resolution[] resolutions;
    private CameraController cameraController;

    private void Awake(){
        cameraController = FindObjectOfType<CameraController>();
        
        InitializeResolutions();
        LoadOptions();
    }

    private void Start(){
        LoadAudio();
    }
    
    //initializes possible resolutions and takes the saved resolution from player prefs
    private void InitializeResolutions(){
        resolutions = Screen.resolutions;
        
        resolutionsDropdown.ClearOptions();

        List<string> resOptions = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++){
            string resOption = resolutions[i].width + " x " + resolutions[i].height;
            resOptions.Add(resOption);

            Resolution cr = Screen.currentResolution;
            if(resolutions[i].width == cr.width && resolutions[i].height == cr.height)
                currentResolutionIndex = i;
        }
        
        resolutionsDropdown.AddOptions(resOptions);

        int savedResolution = PlayerPrefs.GetInt("resolution", -1);
        if(savedResolution != -1){
            currentResolutionIndex = savedResolution;
            SetResolution(savedResolution);
        }
        
        resolutionsDropdown.value = currentResolutionIndex;
        resolutionsDropdown.RefreshShownValue();
    }
    
    //loads saved options from player prefs
    private void LoadOptions(){
        //fullscreen
        bool fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;
        Screen.fullScreen = fullscreen;
        fullscreenToggle.isOn = fullscreen;
        
        //camera sensitivity
        float camSensitivity = PlayerPrefs.GetFloat("cam sensitivity", 0.4f);
        camSensitivitySlider.value = camSensitivity;
            
        if(cameraController != null)
            cameraController.SetSensitivityMultiplier(camSensitivity);
        
        //colour
        float colour = PlayerPrefs.GetFloat("colour", 0.6f);
        colourSlider.value = colour;
        ApplyColour(colour);
    }
    
    //loads audio options from player prefs
    private void LoadAudio(){
        //music volume
        float musicVolume = PlayerPrefs.GetFloat("music volume", 0.6f);
        musicVolumeSlider.value = musicVolume;
        
        musicAudioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
        
        //sfx volume
        float sfxVolume = PlayerPrefs.GetFloat("sfx volume", 1f);
        sfxVolumeSlider.value = sfxVolume;
        
        sfxAudioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
    }
    
    //set options through the UI and save new values to player prefs
    public void SetResolution(int resolutionIndex){
        Resolution res = resolutionIndex < resolutions.Length ? resolutions[resolutionIndex] : resolutions[0];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        
        PlayerPrefs.SetInt("resolution", resolutionIndex);
    }

    public void SetFullscreen(bool fullscreen){
        Screen.fullScreen = fullscreen;
        
        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
    }
    
    public void SetMusicAudio(float value){
        musicAudioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20); 
        PlayerPrefs.SetFloat("music volume", value);
    }
    
    public void SetSfxAudio(float value){
        sfxAudioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20); 
        PlayerPrefs.SetFloat("sfx volume", value);
    }
    
    public void SetCameraSensitivity(float sensitivity){
        if(cameraController != null)
            cameraController.SetSensitivityMultiplier(sensitivity);
        
        PlayerPrefs.SetFloat("cam sensitivity", sensitivity);
    }
    
    public void SetColour(float value){
        ApplyColour(value);
        
        PlayerPrefs.SetFloat("colour", value);
    }

    //uses the color picker value to calculate colors for the sky and the maze floor
    //might not be the best idea to modify materials directly but it works well enough
    private void ApplyColour(float value){
        Color color = Color.HSVToRGB(value, 0.4f, 0.7f);
        floor.color = color;
        
        Color top = Color.HSVToRGB(value, 0.55f, 0.8f);
        sky.SetColor("_Top", top);
        
        Color bottom = Color.HSVToRGB(value, 0.25f, 1f);
        sky.SetColor("_Bottom", bottom);
    }
}
