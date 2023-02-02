using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using DG.Tweening;
using System.Threading.Tasks;

namespace CaptainHindsight
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        public AudioMixerGroup audioMixerGroup;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioClip[] soundClips;
        [SerializeField] private Dictionary<string, AudioSource> sourceList;
        [SerializeField] private AudioSource[] backgroundMusic;
        private float[] startVolume;
        private float countdown;
        private float lengthOfCurrentMusicTrack;

        #region Awake, Start & initialisation
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Load all files of type audio clip from \Resouces\Audio
            soundClips = Resources.LoadAll<AudioClip>("Audio");

            // Create dictionary for audio sources which will be used later to find sounds (string) and play the source (audio source)
            sourceList = new Dictionary<string, AudioSource>();

            // Add audio source for each clip and add to sourceList
            foreach (AudioClip s in soundClips)
            {
                AudioSource source = this.gameObject.AddComponent<AudioSource>();
                source.clip = s;
                source.outputAudioMixerGroup = audioMixerGroup; // All effects are using the SFX mixer group
                sourceList.Add(s.name, source);
            }

            // For trouble shooting sounds that don't play
            //foreach (KeyValuePair<string, AudioSource> kvp in sourceList) Helper.Log(kvp.Key + kvp.Value);
            //AudioSource a = sourceList["ButtonOff"];
            //a.Play();

            // Play background music
            PlayBackgroundMusic();

            // Store starting volume so music can be turned down later
            startVolume = new float[backgroundMusic.Length];
            for (int i = 0; i < backgroundMusic.Length; i++)
            {
                startVolume[i] = backgroundMusic[i].volume;
            }
        }

        // Load audio mixer settings (must loaded at start or later; known Unity bug)
        private void Start() => TryLoadAudioPlayerSettings();
        #endregion

        #region Different play methods for other scripts to call
        public void Play(string name)
        {
            if (sourceList.ContainsKey(name) == false || sourceList[name] == null)
            {
                Helper.LogWarning("AudioManager: Sound '" + name + "' not found.");
                return;
            }
            else sourceList[name].Play();
        }

        public void StopPlaying(string name)
        {
            if (sourceList.ContainsKey(name) == false || sourceList[name] == null)
            {
                Helper.LogWarning("AudioManager: Sound '" + name + "' not found.");
                return;
            }
            else if (sourceList[name].isPlaying)
            {
                sourceList[name].Stop();
            }
            //else Helper.Log("AudioManager: Tried to stop playing " + name + " but it's not playing.");
        }

        public void PlayWithoutOverlap(string name)
        {
            if (sourceList.ContainsKey(name) == false || sourceList[name] == null)
            {
                Helper.LogWarning("AudioManager: Sound '" + name + "' not found.");
                return;
            }
            else
            {
                if (!sourceList[name].isPlaying) sourceList[name].Play();
            }
        }

        public AudioClip RetrieveClipToPlayLocally(string name)
        {
            AudioClip clip = null;
            if (sourceList.ContainsKey(name) == false)
            {
                Helper.LogWarning("AudioManager: Sound '" + name + "' not found.");
                return clip;
            }
            else
            {
                int n = System.Array.IndexOf(soundClips, name);
                clip = soundClips[n];
                return clip;
            }
        }

        public void AddSpatialAudioSource(string name, Vector2 position, Transform t, GameObject parent)
        {
            // This method adds the audio source in a child game object to the object from which it is called.
            // You need to initialise it and play it from the object for which it is created - use the below script:
            // AudioManager.Instance.AddSpatialAudioSource("[NameOfSoundInAudioManager]", transform.position, transform, this.gameObject);
            // audioSource = GetComponentInChildren<AudioSource>();

            if (sourceList.ContainsKey(name) == false)
            {
                Helper.LogWarning("'" + name + "' (audio file) not found. Neither game object nor audio source were added.");
                return;
            }
            else if (sourceList.ContainsKey(name))
            {
                GameObject audioGameObject = new GameObject("Audio-" + name);
                audioGameObject.transform.position = position;
                audioGameObject.transform.SetParent(t);
                AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
                audioSource.clip = sourceList[name].clip;
                audioSource.volume = sourceList[name].volume;
                audioSource.spatialBlend = 1f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.maxDistance = 25f;
                audioSource.dopplerLevel = 0f;
                audioSource.outputAudioMixerGroup = audioMixerGroup;
            }
        }
        #endregion

        #region Manage background music
        private void Update()
        {
            countdown += Time.deltaTime;
            if (countdown >= lengthOfCurrentMusicTrack) PlayBackgroundMusic();
        }

        private void PlayBackgroundMusic()
        {
            countdown = 0f;
            int number = Random.Range(0, backgroundMusic.Length);
            backgroundMusic[number].Play();
            lengthOfCurrentMusicTrack = backgroundMusic[number].clip.length;
            Helper.Log("AudioManager: Now playing track no. " + number + " (out of " + backgroundMusic.Length + "), length: " + backgroundMusic[number].clip.length + " seconds.");
        }

        public void ReduceBackgroundMusicVolume()
        {
            Helper.Log("AudioManager: Background music volume will be reduced.");
            for (int i = 0; i < backgroundMusic.Length; i++)
            {
                backgroundMusic[i].DOFade(0.15f, 1f);
            }
        }

        public async void IncreaseBackgroundMusicVolume()
        {
            await Task.Delay(System.TimeSpan.FromSeconds(1f));
            Helper.Log("AudioManager: Background music volume will be increased again.");
            for (int i = 0; i < backgroundMusic.Length; i++)
            {
                backgroundMusic[i].DOFade(startVolume[i], 2f);
            }
        }
        #endregion

        #region Get player audio settings from PlayerPrefs
        private void TryLoadAudioPlayerSettings()
        {
            if (PlayerPrefs.HasKey("SoundVolume"))
            {
                float value = PlayerPrefs.GetFloat("SoundVolume");
                audioMixer.SetFloat("SFX", Mathf.Log10(value) * 30f);

                // For trouble shooting
                //Helper.Log("Sound value found: " + value);
                //audioMixer.GetFloat("SFX", out float number);
                //Helper.Log("Sound value set to: " + number);
            }

            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                float value = PlayerPrefs.GetFloat("MusicVolume");
                audioMixer.SetFloat("Music", Mathf.Log10(value) * 30f);

                // For trouble shooting
                //Helper.Log("Music value found and set to: " + value);
                //audioMixer.GetFloat("Music", out float number);
                //Helper.Log("Music value set to: " + number);
            }
        }
        #endregion
    }
}