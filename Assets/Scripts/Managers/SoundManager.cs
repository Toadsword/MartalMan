using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{

    public static SoundManager _instance;
    List<AudioSource> emitters = new List<AudioSource>();

    public enum SoundList
    {
        WALK,
        PUNCH,
        IS_HIT,
        HAND_MOVEMENT,
        WIN_SOUND,
        LOSE_SOUND,
        TAKE_FLAG,
        FLAG_BACK
    }

    public struct LoopedSound
    {
        public AudioSource audioSource;
        public float timeUntilStop;
    }
    List<LoopedSound> loopedSoundList = new List<LoopedSound>();

    List<AudioClip> listWalkSounds = new List<AudioClip>();
    List<AudioClip> listPunchSounds = new List<AudioClip>();
    List<AudioClip> listHitSounds = new List<AudioClip>();

    [Header("VolumeSounds")]
    [SerializeField] AudioMixer audioMixer;

    [Header("Sounds")]
    [SerializeField] AudioClip winSoundClip;
    [SerializeField] AudioClip loseSoundClip;
    [SerializeField] AudioClip takeFlagSoundClip;
    [SerializeField] AudioClip flagBackSoundClip;
    [SerializeField] AudioClip handMovementSoundClip;

    [Header("WalkClips")]
    [SerializeField] AudioClip walkClip1;
    [SerializeField] AudioClip walkClip2;
    [SerializeField] AudioClip walkClip3;
    [SerializeField] AudioClip walkClip4;
    [SerializeField] AudioClip walkClip5;

    [Header("HitClips")]
    [SerializeField] AudioClip hitClip1;
    [SerializeField] AudioClip hitClip2;

    [Header("punchClips")]
    [SerializeField] AudioClip punchClip1;
    [SerializeField] AudioClip punchClip2;
    [SerializeField] AudioClip punchClip3;
    [SerializeField] AudioClip punchClip4;
    [SerializeField] AudioClip punchClip5;

    [Header("Emmiters")]
    [SerializeField] GameObject emitterPrefab;
    [SerializeField] int emitterNumber;
    [SerializeField] AudioSource musicEmitter;

    private void Awake()
    {
        if(_instance)
            Destroy(gameObject);
        else
            _instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(gameObject);

        for(int i = 0; i <= emitterNumber;i++)
        {
            GameObject audioObject = Instantiate(emitterPrefab, emitterPrefab.transform.position, emitterPrefab.transform.rotation);
            emitters.Add(audioObject.GetComponent<AudioSource>());
            DontDestroyOnLoad(audioObject);
        }

        listWalkSounds = new List<AudioClip> { walkClip1, walkClip2, walkClip3, walkClip4, walkClip5};
        listPunchSounds = new List<AudioClip> { punchClip1, punchClip2, punchClip3, punchClip4, punchClip5 };
        listHitSounds = new List<AudioClip> { hitClip1, hitClip2};
    }

    private void Update()
    {
        foreach(LoopedSound loopedSound in loopedSoundList)
        {
            if(Utility.IsOver(loopedSound.timeUntilStop))
            {
                loopedSound.audioSource.Stop();
                loopedSoundList.Remove(loopedSound);
                break;
            }
        }
    }

    public AudioSource PlaySound(SoundList sound, float timeToLoop = 0.0f)
    {
        //return null;
        AudioSource emitterAvailable = null;

        foreach(AudioSource emitter in emitters)
        {
            if(!emitter.isPlaying)
            {
                emitterAvailable = emitter;
            }
        }

        if (emitterAvailable != null)
        {
            emitterAvailable.loop = false;
            int index = 0;
            switch (sound)
            {
                case SoundList.WALK:
                    index = Random.Range(0, listWalkSounds.Count);
                    emitterAvailable.clip = listWalkSounds[index];
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Walk")[0];
                    break;
                case SoundList.PUNCH:
                    index = Random.Range(0, listPunchSounds.Count);
                    emitterAvailable.clip = listPunchSounds[index];
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Punch")[0];
                    break;
                case SoundList.IS_HIT:
                    index = Random.Range(0, listHitSounds.Count);
                    emitterAvailable.clip = listHitSounds[index];
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;
                case SoundList.HAND_MOVEMENT:
                    emitterAvailable.clip = handMovementSoundClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Effects")[0];
                    break;
                case SoundList.FLAG_BACK:
                    emitterAvailable.clip = flagBackSoundClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("LittleMusics")[0];
                    break;
                case SoundList.TAKE_FLAG:
                    emitterAvailable.clip = takeFlagSoundClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("LittleMusics")[0];
                    break;
                case SoundList.WIN_SOUND:
                    emitterAvailable.clip = winSoundClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("LittleMusics")[0];
                    break;
                case SoundList.LOSE_SOUND:
                    emitterAvailable.clip = loseSoundClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("LittleMusics")[0];
                    break;
            }

            if(timeToLoop > 0.0f)
            {
                emitterAvailable.loop = true;
                LoopedSound newLoopSound = new LoopedSound
                {
                    audioSource = emitterAvailable,
                    timeUntilStop = Utility.StartTimer(timeToLoop)
                };
                loopedSoundList.Add(newLoopSound);  
            }
            
            emitterAvailable.Play();
            return emitterAvailable;
        }
        else
        {
            Debug.Log("no emitter available");
            return null;
        }        
    }

    public void StopSound(AudioSource source)
    {
        source.Stop();
        foreach(LoopedSound looped in loopedSoundList)
        {
            if(looped.audioSource == source)
            {
                loopedSoundList.Remove(looped);
                break;
            }
        }
    }
}
