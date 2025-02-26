using UnityEngine;
using Random = UnityEngine.Random;

public class RandomSFX : MonoBehaviour {
    
    [SerializeField] private AudioSource effectAudio;
    [SerializeField] private AudioClip[] effectAudioClips;
    
    [SerializeField] private float effectMinPitch;
    [SerializeField] private float effectMaxPitch;

    [SerializeField] private float minVolume;
    [SerializeField] private float maxVolume;

    //overload method with pitch multiplier set to 1
    public void PlayRandom(){
        PlayRandom(1f);
    }

    //randomizes volume and pitch, then plays a random audio clip from the list
    public void PlayRandom(float pitchMultiplier){
        AudioSource targetSource = effectAudio;
        
        if(minVolume != 0 || maxVolume != 0)
            targetSource.volume = Random.Range(minVolume, maxVolume);
        
        targetSource.pitch = Random.Range(effectMinPitch, effectMaxPitch) * pitchMultiplier;
        targetSource.PlayOneShot(effectAudioClips[Random.Range(0, effectAudioClips.Length)]);
    }
}
