using UnityEngine;

public class audiomanager : MonoBehaviour
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioSource engineAudioSource;
    [Header("도어 소리")]
    [SerializeField] private AudioClip doorOpenSound;
    [Header("엔진 소리")]
    [SerializeField] private AudioClip engineStartSound;

    [Header("볼륨 설정")]
    [SerializeField] private float doorSoundVolume = 0.7f;

    [SerializeField] private float engineSoundVolume = 0.8f;

    private static audiomanager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public static audiomanager Instance => instance;

    private void Start()
    {
        ValidateAudioSources();
    }


    private void ValidateAudioSources()
    {
        if (doorAudioSource == null)
            Debug.LogWarning("[CarAudioManager] doorAudioSource가 할당되지 않았습니다!");
        if (engineAudioSource == null)
            Debug.LogWarning("[CarAudioManager] engineAudioSource가 할당되지 않았습니다!");
    }


    public void PlayDoorOpenSound()
    {
        PlaySound(doorAudioSource, doorOpenSound, doorSoundVolume);
    }



    public void PlayEngineStartSound()
    {
        PlaySound(engineAudioSource, engineStartSound, engineSoundVolume);
    }


    private void PlaySound(AudioSource audioSource, AudioClip clip, float volume)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[CarAudioManager] AudioSource가 할당되지 않았습니다!");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("[CarAudioManager] 재생할 AudioClip이 할당되지 않았습니다!");
            return;
        }

        audioSource.volume = volume;
        audioSource.PlayOneShot(clip);
    }

    public void SetMasterVolume(float volume)
    {
        if (doorAudioSource != null) doorAudioSource.volume = volume * doorSoundVolume;
        if (engineAudioSource != null) engineAudioSource.volume = volume * engineSoundVolume;
    }
}