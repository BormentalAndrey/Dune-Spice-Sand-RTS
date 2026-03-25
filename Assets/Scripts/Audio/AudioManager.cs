using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Audio
{
    /// <summary>
    /// Audio manager with Dune-inspired sounds
    /// References: Fremen chants, worm thumpers, Voice effects
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource ambientSource;
        public AudioSource voiceSource;
        
        [Header("Music Tracks")]
        public AudioClip mainMenuMusic;
        public AudioClip desertAmbience;
        public AudioClip combatMusic;
        public AudioClip fremenChant;
        public AudioClip wormApproach;
        
        [Header("Sound Effects")]
        public AudioClip spiceHarvest;
        public AudioClip wormAttack;
        public AudioClip shieldActivate;
        public AudioClip lasgunFire;
        public AudioClip voiceCommand;
        public AudioClip thumperSound;
        public AudioClip sandstormWind;
        
        [Header("Fremen Chants - Dune, Book I")]
        public AudioClip[] fremenChants;
        public AudioClip waterRitual;
        
        [Header("Voice Lines")]
        public AudioClip[] paulLines;
        public AudioClip[] stilgarLines;
        public AudioClip[] baronLines;
        
        [Header("Settings")]
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 0.7f;
        [Range(0f, 1f)] public float ambientVolume = 0.4f;
        [Range(0f, 1f)] public float voiceVolume = 0.8f;
        
        private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();
        private bool isCombatMode = false;
        private Coroutine combatTransitionRoutine;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSoundLibrary();
        }
        
        private void InitializeSoundLibrary()
        {
            // Populate sound library
            soundLibrary["spice_harvest"] = spiceHarvest;
            soundLibrary["worm_attack"] = wormAttack;
            soundLibrary["shield"] = shieldActivate;
            soundLibrary["lasgun"] = lasgunFire;
            soundLibrary["voice"] = voiceCommand;
            soundLibrary["thumper"] = thumperSound;
            soundLibrary["sandstorm"] = sandstormWind;
        }
        
        public void PlayMusic(AudioClip music, bool loop = true)
        {
            if (musicSource == null) return;
            
            musicSource.clip = music;
            musicSource.loop = loop;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
        
        public void PlaySFX(string soundName)
        {
            if (sfxSource == null) return;
            
            if (soundLibrary.ContainsKey(soundName) && soundLibrary[soundName] != null)
            {
                sfxSource.PlayOneShot(soundLibrary[soundName], sfxVolume);
            }
        }
        
        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
            }
        }
        
        public void PlayAmbient(AudioClip ambient)
        {
            if (ambientSource == null) return;
            
            ambientSource.clip = ambient;
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
        }
        
        public void PlayVoiceLine(AudioClip voiceLine)
        {
            if (voiceSource != null && voiceLine != null)
            {
                voiceSource.PlayOneShot(voiceLine, voiceVolume);
            }
        }
        
        public void PlayFremenChant()
        {
            if (fremenChants.Length > 0)
            {
                AudioClip chant = fremenChants[Random.Range(0, fremenChants.Length)];
                PlayVoiceLine(chant);
            }
        }
        
        public void StartCombatMusic()
        {
            if (isCombatMode) return;
            
            isCombatMode = true;
            if (combatTransitionRoutine != null)
                StopCoroutine(combatTransitionRoutine);
            combatTransitionRoutine = StartCoroutine(TransitionToCombat());
        }
        
        public void StopCombatMusic()
        {
            if (!isCombatMode) return;
            
            isCombatMode = false;
            if (combatTransitionRoutine != null)
                StopCoroutine(combatTransitionRoutine);
            combatTransitionRoutine = StartCoroutine(TransitionFromCombat());
        }
        
        private IEnumerator TransitionToCombat()
        {
            // Fade out current music
            float startVolume = musicSource.volume;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                musicSource.volume = Mathf.Lerp(startVolume, 0, t);
                yield return null;
            }
            
            // Start combat music
            if (combatMusic != null)
            {
                musicSource.clip = combatMusic;
                musicSource.Play();
                musicSource.volume = musicVolume;
            }
        }
        
        private IEnumerator TransitionFromCombat()
        {
            // Fade out combat music
            float startVolume = musicSource.volume;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                musicSource.volume = Mathf.Lerp(startVolume, 0, t);
                yield return null;
            }
            
            // Return to desert ambience
            if (desertAmbience != null)
            {
                musicSource.clip = desertAmbience;
                musicSource.Play();
                musicSource.volume = musicVolume;
            }
        }
        
        public void PlayWormThumper()
        {
            // Thumper sound - Dune, Book I, Chapter 18
            PlaySFX("thumper");
            
            // Play distant worm sound
            StartCoroutine(WormApproachSound());
        }
        
        private IEnumerator WormApproachSound()
        {
            yield return new WaitForSeconds(2f);
            
            if (wormApproach != null)
            {
                AudioSource.PlayClipAtPoint(wormApproach, Camera.main.transform.position, sfxVolume);
            }
        }
        
        public void PlayWaterRitual()
        {
            // Water of Life ritual - Dune, Book I, Chapter 36
            if (waterRitual != null)
                PlayVoiceLine(waterRitual);
            
            // Play Fremen chanting
            StartCoroutine(ChantRoutine());
        }
        
        private IEnumerator ChantRoutine()
        {
            for (int i = 0; i < 3; i++)
            {
                PlayFremenChant();
                yield return new WaitForSeconds(5f);
            }
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
        
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            if (ambientSource != null)
                ambientSource.volume = ambientVolume;
        }
        
        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
        }
    }
}
