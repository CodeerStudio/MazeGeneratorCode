using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    
    //references to drawers and camera controller so they don't have to know about the UI
    [SerializeField] private MazeDrawer mazeDrawer;
    [SerializeField] private SolutionDrawer solutionDrawer;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private ButtonSounds buttonSounds;

    [Space]
    [SerializeField] private Button solutionButton;
    [SerializeField] private Text solutionButtonText;
    [SerializeField] private CanvasGroup chunkSizeGroup;
    
    [Header("Screens")]
    [SerializeField] private CanvasGroup startScreen;
    [SerializeField] private GameObject generationScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject controlsOverlay;

    [Header("Maze controls")] 
    [SerializeField] private Slider widthSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider cellSizeSlider;
    [SerializeField] private Slider wallThicknessSlider;
    [SerializeField] private Slider wallHeightSlider;
    [SerializeField] private Slider drawEffectSlider;
    [SerializeField] private Slider chunkSizeSlider;
    [SerializeField] private Toggle customChunkSizeToggle;

    private void Start(){
        //disable anything other than the start screen
        settingsScreen.SetActive(false);
        controlsOverlay.SetActive(false);
        generationScreen.SetActive(false);

        //subscribe to maze drawer actions to show controls overlay panel
        mazeDrawer.OnStartDrawing += () => {
            controlsOverlay.SetActive(true);
        };
        
        mazeDrawer.OnEndDrawing += () => {
            controlsOverlay.SetActive(false);
        };
        
        //initialize controls and solution button
        InitializeMazeControls();
        SetSolutionButton();
    }
    
    private void Update(){
        //toggle settings when user presses escape key
        if(Input.GetButtonDown("Cancel"))
            Settings(!settingsScreen.activeSelf);
    }
    
    //initialize all the sliders using values from the maze drawer
    private void InitializeMazeControls(){
        SetSliderValue(widthSlider, mazeDrawer.GetSize().x);
        SetSliderValue(heightSlider, mazeDrawer.GetSize().y);
        SetSliderValue(cellSizeSlider, mazeDrawer.GetCellSize());
        SetSliderValue(wallThicknessSlider, mazeDrawer.GetWallThickness());
        SetSliderValue(wallHeightSlider, mazeDrawer.GetWallHeight());
        SetSliderValue(drawEffectSlider, mazeDrawer.GetDrawEffect());
        SetSliderValue(chunkSizeSlider, mazeDrawer.GetCustomChunkSize());

        //also initialize the custom chunk controls
        bool useCustomChunksSize = mazeDrawer.GetUseCustomChunkSize();
        customChunkSizeToggle.isOn = useCustomChunksSize;
        chunkSizeGroup.alpha = useCustomChunksSize ? 1f : 0.4f;
        chunkSizeSlider.interactable = useCustomChunksSize;
    }

    //sets the slider value along with the text label
    private void SetSliderValue(Slider slider, float value, bool initialization = true){
        slider.value = value;

        Text sliderLabel = slider.GetComponentInChildren<Text>();
        string format = slider.wholeNumbers ? "f0" : "f2";
        sliderLabel.text = value.ToString(format);

        if(!initialization)
            return;
        
        //makes sure to update the text label when slider value changes, also plays sound
        slider.onValueChanged.AddListener(delegate {
            sliderLabel.text = slider.value.ToString(format);
            buttonSounds.PlayButtonSound(3);
        });
    }

    #region MainControls

    //hide start screen and show controls
    public void StartGenerator(){
        startScreen.DOFade(0f, 0.6f).OnComplete(() => {
            startScreen.gameObject.SetActive(false);
            generationScreen.SetActive(true);
        });
        
        buttonSounds.PlayButtonSound(1);
    }

    //clear current solution and draw a new maze
    public void Generate(){
        solutionDrawer.ClearSolution();
        
        float mazeSize = mazeDrawer.DrawMaze();
        
        //reset camera so it looks at the new maze
        if(cameraController != null)
            cameraController.ResetCameraOrientation(mazeSize);

        SetSolutionButton();
        buttonSounds.PlayButtonSound(1);
    }

    //toggles the solution so it either starts drawing the solution or clears the solution
    public void ToggleSolution(){
        if(solutionDrawer.IsShowingSolution()){
            solutionDrawer.ClearSolution();
        }
        else{
            solutionDrawer.ShowSolution(mazeDrawer);
        }

        SetSolutionButton();
        buttonSounds.PlayButtonSound(1);
    }
    
    //quits the app, in editor it exits playmode
    public void Quit(){
        buttonSounds.PlayButtonSound(1);
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    //toggles the settings screen
    public void Settings(bool show){
        if(show){
            settingsScreen.SetActive(true);
        }
        else{
            StartCoroutine(HideSettings());
        }
        
        buttonSounds.PlayButtonSound(0);
    }
    
    //waits for the settings screen to be hidden before deactivating it
    private IEnumerator HideSettings(){
        settingsScreen.GetComponent<Animator>().SetBool("Show", false);

        yield return new WaitForSeconds(1f/3f);
        
        settingsScreen.SetActive(false);
    }

    //updates the solution button interact-ability and label text
    //based on whether there exists a solution and if it's currently showing
    private void SetSolutionButton(){
        bool canShowSolution = mazeDrawer.HasMaze();
        solutionButton.interactable = canShowSolution;

        bool showingSolution = solutionDrawer.IsShowingSolution();
        solutionButtonText.text = showingSolution ? "HIDE SOLUTION" : "SHOW SOLUTION";
    }

    #endregion

    #region MazeControls

    //simple bridge functions to pass UI input to the maze drawer
    public void SetMazeWidth(float value){
        mazeDrawer.SetWidth((int)value);

        UpdateChunkSizeSlider();
    }

    public void SetMazeHeight(float value){
        mazeDrawer.SetHeight((int)value);
        
        UpdateChunkSizeSlider();
    }
    
    public void SetCellSize(float value){
        mazeDrawer.SetCellSize(value);
    }
    
    public void SetWallThickness(float value){
        mazeDrawer.SetWallThickness(value);
    }
    
    public void SetWallHeight(float value){
        mazeDrawer.SetWallHeight(value);
    }
    
    public void SetDrawEffect(float value){
        mazeDrawer.SetDrawEffect(value);
    }
    
    public void SetChunkSize(float value){
        mazeDrawer.SetCustomChunkSize((int)value);
    }

    //after updating the toggle this will also show/hide the chunk size controls
    public void SetUseCustomChunkSize(bool useCustomChunkSize){
        mazeDrawer.SetUseCustomChunkSize(useCustomChunkSize);
        
        chunkSizeGroup.alpha = useCustomChunkSize ? 1f : 0.4f;
        chunkSizeSlider.interactable = useCustomChunkSize;
        
        buttonSounds.PlayButtonSound(1);
        
        //if user doesn't use custom chunks size it needs to display the calculated size
        if(!useCustomChunkSize)
            UpdateChunkSizeSlider();
    }

    #endregion

    //get the calculated chunks size and apply it to the chunks size slider
    private void UpdateChunkSizeSlider(){
        int calculatedChunkSize = mazeDrawer.CalculateChunkSize();
        SetSliderValue(chunkSizeSlider, calculatedChunkSize);
    }
}
