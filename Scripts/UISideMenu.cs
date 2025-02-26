using DG.Tweening;
using UnityEngine;

public class UISideMenu : MonoBehaviour{

    [SerializeField] private RectTransform sidePanel;
    [SerializeField] private RectTransform arrowIcon;
    
    [SerializeField] private float moveDuration;

    [SerializeField] private ButtonSounds buttonSounds;

    //show the side menu panel from the start
    private bool show = true;

    //toggle the panel when this object is enabled
    private void OnEnable(){
        TogglePanel(false);
    }

    //toggles the panel so it folds in/out of the side of the screen
    public void TogglePanel(bool playSound = true){
        //if the panel is currently moving we need to wait for it to finish
        if(DOTween.IsTweening(sidePanel))
            return;

        //toggle panel
        show = !show;
        
        //tween to the new target x location
        float target = show ? 0 : -sidePanel.rect.width;
        sidePanel.DOAnchorPosX(target, moveDuration);

        //flip the little arrow icon
        Quaternion arrowTarget = arrowIcon.rotation * Quaternion.Euler(Vector3.forward * 180);
        arrowIcon.DORotateQuaternion(arrowTarget, moveDuration);
        
        //play button sound
        if(playSound && buttonSounds != null)
            buttonSounds.PlayButtonSound(0);
    }
}
