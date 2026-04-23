using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement _playerMovement;

    [Header("Audio Sources (ลากใส่ใน Inspector)")]
    [SerializeField] private AudioSource _movementAudioSource; // เสียงเดิน/วิ่ง (Loop)
    [SerializeField] private AudioSource _sfxAudioSource;      // เสียง Dash/สกิล/ฟัน (OneShot)

    [Header("Audio Clips")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip sprintClip;
    [SerializeField] private AudioClip dashClip;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        if (_playerMovement != null)
        {
            _playerMovement.OnWalkStateChange += HandleWalkSound;
            _playerMovement.OnSprinteStateChange += HandleSprintSound;
            _playerMovement.OnDashStateChange += HandleDashSound;
        }
    }

    private void OnDisable()
    {
        if (_playerMovement != null)
        {
            _playerMovement.OnWalkStateChange -= HandleWalkSound;
            _playerMovement.OnSprinteStateChange -= HandleSprintSound;
            _playerMovement.OnDashStateChange -= HandleDashSound;
        }
    }

    private void HandleWalkSound(bool isWalking)
    {
        if (isWalking)
        {
            if (!_movementAudioSource.isPlaying || _movementAudioSource.clip != walkClip)
            {
                _movementAudioSource.clip = walkClip;
                _movementAudioSource.loop = true;
                _movementAudioSource.Play();
            }
        }
        else
        {
            if (_movementAudioSource.isPlaying && _movementAudioSource.clip == walkClip)
            {
                _movementAudioSource.Stop();
            }
        }
    }

    private void HandleSprintSound(bool isSprinting)
    {
        if (isSprinting)
        {
            _movementAudioSource.clip = sprintClip;
            _movementAudioSource.loop = true;
            _movementAudioSource.Play();
        }
        else
        {
            if (_movementAudioSource.clip == sprintClip)
            {
                _movementAudioSource.Stop();
            }
        }
    }

    private void HandleDashSound(bool isDashing, Vector3 direction)
    {
        if (isDashing)
        {
            // ให้ SFX Source เป็นคนเล่นเสียง Dash (จะไม่โดนคำสั่ง Stop จากฝั่ง Movement กวน)
            _sfxAudioSource.PlayOneShot(dashClip);
        }
    }
}