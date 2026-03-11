using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SoundMinigameController : MonoBehaviour
{
    private AudioSource _audioSource;

    [Header("Rhythm Sounds")]
    public List<AudioClip> rhythmSounds = new List<AudioClip>();

    [Header("Hit Sounds")]
    public List<AudioClip> hitSounds = new List<AudioClip>();

    [Header("Slat Sounds")]
    public List<AudioClip> SlatSounds = new List<AudioClip>();

    private MiniGameFManager _miniGameManager;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        // สมมติว่า SoundController เกาะอยู่ใกล้ๆ กับ Manager
        _miniGameManager = GetComponent<MiniGameFManager>();
    }

    void OnEnable()
    {
        if (_miniGameManager != null)
        {
            _miniGameManager.OnPlaySoundRhythm += HandlePlaySoundRhythm;
            _miniGameManager.OnPlaySoundHit += HandlePlaySoundHit;
            _miniGameManager.OnPlaySoundSlat += HandlePlaySoundSlat;
        }
    }

    void OnDisable()
    {
        if (_miniGameManager != null)
        {
            _miniGameManager.OnPlaySoundRhythm -= HandlePlaySoundRhythm;
            _miniGameManager.OnPlaySoundHit -= HandlePlaySoundHit;
            _miniGameManager.OnPlaySoundSlat -= HandlePlaySoundSlat;
        }
    }

    private void HandlePlaySoundRhythm()
    {
        PlayRandomSound(rhythmSounds);
    }
    private void HandlePlaySoundHit()
    {
        PlayRandomSound(hitSounds);
    }
    private void HandlePlaySoundSlat()
    {
        PlayRandomSound(SlatSounds);
    }

    private void PlayRandomSound(List<AudioClip> sounds)
    {
        if (sounds.Count > 0)
        {
            // สุ่ม Index เสียงใน List
            int randomIndex = Random.Range(0, sounds.Count);

            // เล่นเสียงแบบทับซ้อนได้ (เผื่อจังหวะมาเร็ว)
            _audioSource.PlayOneShot(sounds[randomIndex]);
        }
    }

}