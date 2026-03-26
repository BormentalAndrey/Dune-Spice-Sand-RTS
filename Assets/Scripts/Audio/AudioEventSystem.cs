using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Audio
{
    /// <summary>
    /// Dynamic audio event system for Dune-specific sounds
    /// References: Fremen chants, worm thumpers, shield impacts
    /// </summary>
    public class AudioEventSystem : MonoBehaviour
    {
        public static AudioEventSystem Instance { get; private set; }
        
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource ambientSource;
        public AudioSource sfxSource;
        public AudioSource voiceSource;
        
        [Header("Dynamic Audio")]
        public AudioLowPassFilter lowPassFilter;
        public AudioReverbFilter reverbFilter;
        
        [Header("Audio Clusters")]
        public AudioClip[] combatClips;
        public AudioClip[] ambientClips;
        public AudioClip[] fremenChants;
        public AudioClip[] wormSounds;
        public AudioClip[] shieldSounds;
        
        [Header("Audio Mixer")]
        public AudioMixerGroup masterMixer;
        public AudioMixerGroup musicMixer;
        public AudioMixerGroup sfxMixer;
        
        private Dictionary<string, AudioClip> audioLibrary = new Dictionary<string, AudioClip>();
        private List<AudioSource> activeVoices = new List<AudioSource>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAudioLibrary();
        }
        
        private void InitializeAudioLibrary()
        {
            // Combat sounds
            AddToLibrary("lasgun_fire", combatClips.Length > 0 ? combatClips[0] : null);
            AddToLibrary("shield_hit", shieldSounds.Length > 0 ? shieldSounds[0] : null);
            AddToLibrary("shield_activate", shieldSounds.Length > 1 ? shieldSounds[1] : null);
            
            // Ambient sounds
            AddToLibrary("wind_desert", ambientClips.Length > 0 ? ambientClips[0] : null);
            AddToLibrary("sand_dunes", ambientClips.Length > 1 ? ambientClips[1] : null);
            
            // Worm sounds
            AddToLibrary("worm_roar", wormSounds.Length > 0 ? wormSounds[0] : null);
            AddToLibrary("worm_approach", wormSounds.Length > 1 ? wormSounds[1] : null);
            AddToLibrary("worm_emergence", wormSounds.Length > 2 ? wormSounds[2] : null);
            
            // Fremen sounds
            AddToLibrary("fremen_chant", fremenChants.Length > 0 ? fremenChants[0] : null);
            AddToLibrary("crysknife_draw", fremenChants.Length > 1 ? fremenChants[1] : null);
        }
        
        private void AddToLibrary(string key, AudioClip clip)
        {
            if (clip != null && !audioLibrary.ContainsKey(key))
            {
                audioLibrary[key] = clip;
            }
        }
        
        public void PlaySound(string soundKey, Vector3 position, float volume = 1f)
        {
            if (!audioLibrary.ContainsKey(soundKey)) return;
            
            AudioSource.PlayClipAtPoint(audioLibrary[soundKey], position, volume * sfxSource.volume);
        }
        
        public void PlayOneShot(string soundKey)
        {
            if (!audioLibrary.ContainsKey(soundKey)) return;
            
            sfxSource.PlayOneShot(audioLibrary[soundKey], sfxSource.volume);
        }
        
        public void PlayVoiceLine(AudioClip clip, float delay = 0f)
        {
            if (clip == null) return;
            
            StartCoroutine(PlayVoiceRoutine(clip, delay));
        }
        
        private IEnumerator PlayVoiceRoutine(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            AudioSource voiceSource = CreateVoiceSource();
            voiceSource.clip = clip;
            voiceSource.Play();
            
            activeVoices.Add(voiceSource);
            StartCoroutine(CleanupVoiceSource(voiceSource, clip.length));
        }
        
        private AudioSource CreateVoiceSource()
        {
            GameObject go = new GameObject("VoiceSource");
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixer;
            source.volume = voiceSource.volume;
            return source;
        }
        
        private IEnumerator CleanupVoiceSource(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration);
            activeVoices.Remove(source);
            Destroy(source.gameObject);
        }
        
        public void PlayDynamicAmbient(float intensity)
        {
            // Adjust ambient based on game state
            float targetPitch = 0.8f + intensity * 0.4f;
            float targetVolume = 0.3f + intensity * 0.5f;
            
            StartCoroutine(CrossFadeAmbient(targetVolume, targetPitch, 2f));
        }
        
        private IEnumerator CrossFadeAmbient(float targetVolume, float targetPitch, float duration)
        {
            float startVolume = ambientSource.volume;
            float startPitch = ambientSource.pitch;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                ambientSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                ambientSource.pitch = Mathf.Lerp(startPitch, targetPitch, t);
                
                yield return null;
            }
        }
        
        public void PlayWormApproach(float distance)
        {
            // Pitch increases as worm gets closer
            float pitch = Mathf.Lerp(0.5f, 1.5f, 1f - (distance / 50f));
            float volume = Mathf.Lerp(0.2f, 1f, 1f - (distance / 50f));
            
            if (audioLibrary.ContainsKey("worm_approach"))
            {
                AudioSource.PlayClipAtPoint(audioLibrary["worm_approach"], Camera.main.transform.position, volume);
            }
        }
        
        public void PlayFremenChant(bool isRitual = false)
        {
            if (fremenChants.Length == 0) return;
            
            AudioClip chant = fremenChants[Random.Range(0, fremenChants.Length)];
            
            if (isRitual)
            {
                // Add reverb for ritual effect
                if (reverbFilter != null)
                {
                    reverbFilter.enabled = true;
                    StartCoroutine(DisableReverbAfterDelay(5f));
                }
            }
            
            PlayVoiceLine(chant);
        }
        
        private IEnumerator DisableReverbAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (reverbFilter != null) reverbFilter.enabled = false;
        }
        
        public void PlayShieldImpact(float intensity)
        {
            if (!audioLibrary.ContainsKey("shield_hit")) return;
            
            float pitch = 0.8f + intensity * 0.6f;
            AudioSource.PlayClipAtPoint(audioLibrary["shield_hit"], Camera.main.transform.position, sfxSource.volume);
        }
        
        public void PlayLasgunFire()
        {
            if (!audioLibrary.ContainsKey("lasgun_fire")) return;
            
            AudioSource.PlayClipAtPoint(audioLibrary["lasgun_fire"], Camera.main.transform.position, sfxSource.volume);
        }
        
        public void ApplyLowPassFilter(bool enable, float cutoff = 500f)
        {
            if (lowPassFilter == null) return;
            
            lowPassFilter.enabled = enable;
            if (enable)
            {
                lowPassFilter.cutoffFrequency = cutoff;
            }
        }
        
        public void SetMusicIntensity(float intensity)
        {
            // Intensity 0-1 affects music volume and pitch
            musicSource.volume = Mathf.Lerp(0.3f, 0.8f, intensity);
            musicSource.pitch = Mathf.Lerp(0.9f, 1.1f, intensity);
        }
        
        public void StopAllVoices()
        {
            foreach (var source in activeVoices)
            {
                if (source != null)
                {
                    source.Stop();
                    Destroy(source.gameObject);
                }
            }
            activeVoices.Clear();
        }
        
        private void Update()
        {
            // Update 3D audio for dynamic effects
            if (Camera.main != null && audioLibrary.ContainsKey("wind_desert"))
            {
                // Wind intensity based on camera movement
                float windIntensity = Mathf.Clamp01(Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y"));
                ambientSource.volume = Mathf.Lerp(0.3f, 0.6f, windIntensity);
            }
        }
    }
}
