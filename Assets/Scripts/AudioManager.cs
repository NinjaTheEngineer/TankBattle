using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip shootAudio, impactAudio;
    [SerializeField] AudioSource audioSource;
    public static AudioManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.LogWarning("Duplicate AudioManager detected and destroyed.");
            DestroyImmediate(gameObject);
        }
    }

    public void PlayShootAudio()
    {
        audioSource.clip = shootAudio;
        audioSource.Play();
    }

    public void PlayImpactAudio()
    {
        audioSource.clip = impactAudio;
        audioSource.Play();
    }
}
