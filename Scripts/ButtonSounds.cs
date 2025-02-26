using UnityEngine;

public class ButtonSounds : MonoBehaviour {
    
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] clips;

    //plays the sound at index clipIndex without altering pitch/volume
    public void PlayButtonSound(int clipIndex){
        if(source.isPlaying)
            return;
        
        source.PlayOneShot(clips[clipIndex]);
    }
}
