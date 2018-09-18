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
        DEATH,
        MENU_SELECTION,
        MENU_VALIDATION,
        JUMPING,
        LANDING,
        BUTTON_ACTIVATION,
        BUTTON_DESACTIVATION,
        CASCADE,
        WIND,
        TOTEM_ACTIVATION,
        DOOR_SOUND
    }

    public enum MusicList
    {
        NONE,
        MENU,
        CAVE,
        FINAL,
        JOY,
        ELECTRIC_WORLD,
        STRANGE_MUSIC
    }

    public struct LoopedSound
    {
        public AudioSource audioSource;
        public float timeUntilStop;
    }
    List<LoopedSound> loopedSoundList = new List<LoopedSound>();

    MusicList currentMusicPlaying = MusicList.NONE;

    List<AudioClip> listWalkSounds = new List<AudioClip>();
    List<AudioClip> listDemonNoiseSounds = new List<AudioClip>();

    [Header("VolumeSounds")]
    [SerializeField] AudioMixer audioMixer;

    [Header("Sounds")]
    [SerializeField] AudioClip deathClip;
    [SerializeField] AudioClip menuSelectionClip;
    [SerializeField] AudioClip menuValidaitonClip;
    [SerializeField] AudioClip jumpingClip;
    [SerializeField] AudioClip landingClip;
    [SerializeField] AudioClip btnActivationClip;
    [SerializeField] AudioClip btnDesctivationClip;
    [SerializeField] AudioClip cascadeClip;
    [SerializeField] AudioClip totemActivationClip;
    [SerializeField] AudioClip doorSoundClip;
    [SerializeField] AudioClip windSoundClip;

    [Header("WalkClips")]
    [SerializeField] AudioClip walkClip1;
    [SerializeField] AudioClip walkClip2;
    [SerializeField] AudioClip walkClip3;

    [Header("Musics")]
    [SerializeField] AudioClip menuMusicClip;
    [SerializeField] AudioClip caveMusicClip;
    [SerializeField] AudioClip finalMusicClip;
    [SerializeField] AudioClip joyMusicClip;
    [SerializeField] AudioClip electricWorldClip;
    [SerializeField] AudioClip strangeMusicClip;

    [Header("Emmiters")]
    [SerializeField] GameObject emitterPrefab;
    [SerializeField] int emitterNumber;
    [SerializeField] AudioSource musicEmitter;

    private void Awake()
    {
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

        listWalkSounds = new List<AudioClip>{walkClip1, walkClip2, walkClip3};
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
            Debug.Log(sound.ToString());
            switch (sound)
            {
                case SoundList.WALK:
                    int indexWalk = Random.Range(0, listWalkSounds.Count);
                    emitterAvailable.clip = listWalkSounds[indexWalk];
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Player")[0];
                    break;
                case SoundList.BUTTON_ACTIVATION:
                    emitterAvailable.clip = btnActivationClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Environment")[0];
                    break;
                case SoundList.BUTTON_DESACTIVATION:
                    emitterAvailable.clip = btnDesctivationClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Environment")[0];
                    break;
                case SoundList.CASCADE:
                    emitterAvailable.clip = cascadeClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Environment")[0];
                    break;
                case SoundList.DEATH:
                    emitterAvailable.clip = deathClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Player")[0];
                    break;
                case SoundList.DOOR_SOUND:
                    emitterAvailable.clip = doorSoundClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Environment")[0];
                    break;
                case SoundList.JUMPING:
                    emitterAvailable.clip = jumpingClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Player")[0];
                    break;
                case SoundList.LANDING:
                    emitterAvailable.clip = landingClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Player")[0];
                    break;
                case SoundList.MENU_SELECTION:
                    emitterAvailable.clip = menuSelectionClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Menu")[0];
                    break;
                case SoundList.MENU_VALIDATION:
                    emitterAvailable.clip = menuValidaitonClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Menu")[0];
                    break;
                case SoundList.TOTEM_ACTIVATION:
                    emitterAvailable.clip = totemActivationClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Environment")[0];
                    break;
                case SoundList.WIND:
                    emitterAvailable.clip = windSoundClip;
                    emitterAvailable.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Environment")[0];
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

    public void PlayMusic(MusicList music)
    {
        if (currentMusicPlaying != music)
        {
            musicEmitter.loop = true;

            switch (music)
            {
                case MusicList.NONE:
                    musicEmitter.Stop();
                    break;
                case MusicList.MENU:
                    musicEmitter.clip = menuMusicClip;
                    musicEmitter.Play();
                    break;
                case MusicList.STRANGE_MUSIC:
                    musicEmitter.clip = strangeMusicClip;
                    musicEmitter.Play();
                    break;
                case MusicList.CAVE:
                    musicEmitter.clip = caveMusicClip;
                    musicEmitter.Play();
                    break;
                case MusicList.ELECTRIC_WORLD:
                    musicEmitter.clip = electricWorldClip;
                    musicEmitter.Play();
                    break;
                case MusicList.FINAL:
                    musicEmitter.clip = finalMusicClip;
                    musicEmitter.Play();
                    break;
                case MusicList.JOY:
                    musicEmitter.clip = joyMusicClip;
                    musicEmitter.Play();
                    break;
            }
            currentMusicPlaying = music;
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
