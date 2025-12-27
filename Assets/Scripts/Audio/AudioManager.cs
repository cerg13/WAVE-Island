using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace WaveIsland.Audio
{
    /// <summary>
    /// Central audio management system
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";
        [SerializeField] private string uiVolumeParam = "UIVolume";

        [Header("Music")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private float musicFadeDuration = 1f;

        [Header("SFX")]
        [SerializeField] private int sfxPoolSize = 10;
        [SerializeField] private AudioSource sfxPrefab;

        [Header("Sound Library")]
        [SerializeField] private SoundLibrary soundLibrary;

        // Volume settings (0-1)
        private float masterVolume = 1f;
        private float musicVolume = 1f;
        private float sfxVolume = 1f;
        private float uiVolume = 1f;
        private bool isMuted = false;

        // SFX pool
        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int currentPoolIndex = 0;

        // Music state
        private AudioClip currentMusic;
        private Coroutine fadeCoroutine;

        // Events
        public event Action<string> OnMusicChanged;
        public event Action<bool> OnMuteChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudio()
        {
            // Create SFX pool
            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource source = CreateSFXSource();
                sfxPool.Add(source);
            }

            // Load saved settings
            LoadSettings();
        }

        private AudioSource CreateSFXSource()
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);

            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            if (audioMixer != null)
            {
                var groups = audioMixer.FindMatchingGroups("SFX");
                if (groups.Length > 0)
                    source.outputAudioMixerGroup = groups[0];
            }

            return source;
        }

        #region Settings

        private void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("Audio_Master", 1f);
            musicVolume = PlayerPrefs.GetFloat("Audio_Music", 1f);
            sfxVolume = PlayerPrefs.GetFloat("Audio_SFX", 1f);
            uiVolume = PlayerPrefs.GetFloat("Audio_UI", 1f);
            isMuted = PlayerPrefs.GetInt("Audio_Muted", 0) == 1;

            ApplyVolumeSettings();

            if (isMuted)
            {
                AudioListener.volume = 0;
            }
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("Audio_Master", masterVolume);
            PlayerPrefs.SetFloat("Audio_Music", musicVolume);
            PlayerPrefs.SetFloat("Audio_SFX", sfxVolume);
            PlayerPrefs.SetFloat("Audio_UI", uiVolume);
            PlayerPrefs.SetInt("Audio_Muted", isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplyVolumeSettings()
        {
            if (audioMixer == null) return;

            // Convert linear volume (0-1) to logarithmic dB (-80 to 0)
            audioMixer.SetFloat(masterVolumeParam, LinearToDecibel(masterVolume));
            audioMixer.SetFloat(musicVolumeParam, LinearToDecibel(musicVolume));
            audioMixer.SetFloat(sfxVolumeParam, LinearToDecibel(sfxVolume));
            audioMixer.SetFloat(uiVolumeParam, LinearToDecibel(uiVolume));
        }

        private float LinearToDecibel(float linear)
        {
            return linear > 0.001f ? Mathf.Log10(linear) * 20f : -80f;
        }

        private float DecibelToLinear(float dB)
        {
            return Mathf.Pow(10f, dB / 20f);
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            SaveSettings();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            SaveSettings();
        }

        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
            SaveSettings();
        }

        public void SetMuted(bool muted)
        {
            isMuted = muted;
            AudioListener.volume = muted ? 0 : 1;
            SaveSettings();
            OnMuteChanged?.Invoke(muted);
        }

        public void ToggleMute()
        {
            SetMuted(!isMuted);
        }

        // Getters
        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        public float GetUIVolume() => uiVolume;
        public bool IsMuted() => isMuted;

        #endregion

        #region Music

        /// <summary>
        /// Play background music with optional fade
        /// </summary>
        public void PlayMusic(string musicId, bool fade = true)
        {
            AudioClip clip = soundLibrary?.GetMusic(musicId);
            if (clip == null)
            {
                Debug.LogWarning($"AudioManager: Music '{musicId}' not found");
                return;
            }

            PlayMusic(clip, fade);
        }

        public void PlayMusic(AudioClip clip, bool fade = true)
        {
            if (clip == currentMusic && musicSource.isPlaying)
                return;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (fade && musicSource.isPlaying)
            {
                fadeCoroutine = StartCoroutine(CrossfadeMusic(clip));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.Play();
            }

            currentMusic = clip;
            OnMusicChanged?.Invoke(clip.name);
        }

        /// <summary>
        /// Stop music with optional fade
        /// </summary>
        public void StopMusic(bool fade = true)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (fade)
            {
                fadeCoroutine = StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicSource.Stop();
            }

            currentMusic = null;
        }

        /// <summary>
        /// Pause/unpause music
        /// </summary>
        public void PauseMusic(bool pause)
        {
            if (pause)
                musicSource.Pause();
            else
                musicSource.UnPause();
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip)
        {
            float startVolume = musicSource.volume;

            // Fade out
            float timer = 0;
            while (timer < musicFadeDuration / 2)
            {
                timer += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, timer / (musicFadeDuration / 2));
                yield return null;
            }

            // Switch clip
            musicSource.clip = newClip;
            musicSource.Play();

            // Fade in
            timer = 0;
            while (timer < musicFadeDuration / 2)
            {
                timer += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0, startVolume, timer / (musicFadeDuration / 2));
                yield return null;
            }

            musicSource.volume = startVolume;
        }

        private IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            float timer = 0;

            while (timer < musicFadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, timer / musicFadeDuration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }

        #endregion

        #region Ambient

        /// <summary>
        /// Play ambient sounds
        /// </summary>
        public void PlayAmbient(string ambientId)
        {
            AudioClip clip = soundLibrary?.GetAmbient(ambientId);
            if (clip == null) return;

            ambientSource.clip = clip;
            ambientSource.loop = true;
            ambientSource.Play();
        }

        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        #endregion

        #region SFX

        /// <summary>
        /// Play sound effect by ID
        /// </summary>
        public void PlaySFX(string sfxId)
        {
            AudioClip clip = soundLibrary?.GetSFX(sfxId);
            if (clip == null)
            {
                Debug.LogWarning($"AudioManager: SFX '{sfxId}' not found");
                return;
            }

            PlaySFX(clip);
        }

        /// <summary>
        /// Play sound effect with volume and pitch variation
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitchVariation = 0f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSFXSource();

            source.clip = clip;
            source.volume = volumeScale;
            source.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
            source.Play();
        }

        /// <summary>
        /// Play SFX at world position (3D sound)
        /// </summary>
        public void PlaySFXAtPosition(string sfxId, Vector3 position)
        {
            AudioClip clip = soundLibrary?.GetSFX(sfxId);
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }

        /// <summary>
        /// Play UI sound
        /// </summary>
        public void PlayUISound(string soundId)
        {
            AudioClip clip = soundLibrary?.GetUI(soundId);
            if (clip == null) return;

            AudioSource source = GetAvailableSFXSource();
            source.clip = clip;
            source.volume = 1f;
            source.pitch = 1f;
            source.Play();
        }

        private AudioSource GetAvailableSFXSource()
        {
            // Find non-playing source
            foreach (var source in sfxPool)
            {
                if (!source.isPlaying)
                    return source;
            }

            // Use round-robin if all playing
            AudioSource result = sfxPool[currentPoolIndex];
            currentPoolIndex = (currentPoolIndex + 1) % sfxPool.Count;
            return result;
        }

        #endregion

        #region Common Sounds

        // Convenience methods for common sounds
        public void PlayButtonClick() => PlayUISound("button_click");
        public void PlayButtonHover() => PlayUISound("button_hover");
        public void PlayPurchase() => PlaySFX("purchase");
        public void PlayReward() => PlaySFX("reward");
        public void PlayLevelUp() => PlaySFX("level_up");
        public void PlayAchievement() => PlaySFX("achievement");
        public void PlayHarvest() => PlaySFX("harvest");
        public void PlayPlant() => PlaySFX("plant");
        public void PlayCraft() => PlaySFX("craft");
        public void PlaySuccess() => PlaySFX("success");
        public void PlayError() => PlaySFX("error");
        public void PlayNotification() => PlaySFX("notification");

        #endregion
    }

    #region Sound Library

    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "WAVE Island/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        [Header("Music")]
        public SoundEntry[] musicTracks;

        [Header("Ambient")]
        public SoundEntry[] ambientSounds;

        [Header("SFX")]
        public SoundEntry[] sfxSounds;

        [Header("UI")]
        public SoundEntry[] uiSounds;

        private Dictionary<string, AudioClip> musicDict;
        private Dictionary<string, AudioClip> ambientDict;
        private Dictionary<string, AudioClip> sfxDict;
        private Dictionary<string, AudioClip> uiDict;

        private void OnEnable()
        {
            BuildDictionaries();
        }

        private void BuildDictionaries()
        {
            musicDict = BuildDict(musicTracks);
            ambientDict = BuildDict(ambientSounds);
            sfxDict = BuildDict(sfxSounds);
            uiDict = BuildDict(uiSounds);
        }

        private Dictionary<string, AudioClip> BuildDict(SoundEntry[] entries)
        {
            var dict = new Dictionary<string, AudioClip>();
            if (entries == null) return dict;

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.id) && entry.clip != null)
                {
                    dict[entry.id] = entry.clip;
                }
            }
            return dict;
        }

        public AudioClip GetMusic(string id) =>
            musicDict != null && musicDict.TryGetValue(id, out var clip) ? clip : null;

        public AudioClip GetAmbient(string id) =>
            ambientDict != null && ambientDict.TryGetValue(id, out var clip) ? clip : null;

        public AudioClip GetSFX(string id) =>
            sfxDict != null && sfxDict.TryGetValue(id, out var clip) ? clip : null;

        public AudioClip GetUI(string id) =>
            uiDict != null && uiDict.TryGetValue(id, out var clip) ? clip : null;
    }

    [Serializable]
    public struct SoundEntry
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    #endregion
}
