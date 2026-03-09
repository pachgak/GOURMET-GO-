using UnityEngine;

public class PlayerParticleController : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement _playerMovement; // ลาก Player ที่มีสคริปต์ PlayerMovement มาใส่
    private AudioSource _audioSource; // ลาก Player ที่มีสคริปต์ PlayerMovement มาใส่
    [SerializeField] private ParticleSystem sprintParticle; // ลาก Particle System มาใส่
    [SerializeField] private Vector3 sprintOffSet;
    [SerializeField] private AudioClip sprintClip;
    [SerializeField] private ParticleSystem dashParticle;   // เอฟเฟกต์ตอนพุ่ง (Dash)
    [SerializeField] private Vector3 dashOffSet;
    [SerializeField] private AudioClip dashClip;


    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _audioSource = GetComponent<AudioSource>();

        sprintParticle.Stop();
        dashParticle.Stop();
    }

    private void OnEnable()
    {
        if (_playerMovement != null)
        {
            // Subscribe Sprint & Direction Events
            _playerMovement.OnSprinteStateChange += HandleSprintParticle;
            _playerMovement.OnLastMoveDirectionChange += HandleSprintRotation;

            // Subscribe Dash Event (เพิ่มส่วนนี้)
            _playerMovement.OnDashStateChange += HandleDashParticle;
        }
    }

    private void OnDisable()
    {
        if (_playerMovement != null)
        {
            _playerMovement.OnSprinteStateChange -= HandleSprintParticle;
            _playerMovement.OnLastMoveDirectionChange -= HandleSprintRotation;

            // Unsubscribe Dash Event (เพิ่มส่วนนี้)
            _playerMovement.OnDashStateChange -= HandleDashParticle;
        }
    }

    // ฟังก์ชันจัดการการเล่น/หยุด Particle เมื่อวิ่ง
    private void HandleSprintParticle(bool isSprinting)
    {
        if (sprintParticle == null) return;

        if (isSprinting)
        {
            _audioSource.clip = sprintClip;
            _audioSource.loop = true;
            _audioSource.Play();
            // ถ้าวิ่งอยู่ และ Particle ยังไม่เล่น ให้เล่น
            //if (!sprintParticle.isPlaying)
            //{
            sprintParticle.Play();
            
            //}
        }
        else
        {
            // ถ้าหยุดวิ่ง ให้หยุด Particle
            sprintParticle.Stop();
            if(_audioSource.clip == sprintClip) _audioSource.Stop();

        }
    }

    // ฟังก์ชันจัดการการหมุน Particle ตามทิศทางล่าสุด
    private void HandleSprintRotation(Vector3 direction)
    {
        if (sprintParticle == null) return;

        // ป้องกัน Error กรณี Vector เป็น 0
        if (direction != Vector3.zero)
        {
            // หมุนตัว Particle System ไปตามทิศทางที่ได้รับมา
            sprintParticle.transform.rotation = Quaternion.LookRotation(-direction);
            sprintParticle.transform.position = UpdatePosition(-direction, sprintOffSet);
        }
    }

    // --- ส่วนของ Dash (ใหม่) ---
    private void HandleDashParticle(bool isDashing, Vector3 direction)
    {
        if (dashParticle == null) return;

        if (isDashing)
        {
            // 1. หมุน Dash Particle ไปตามทิศทางที่จะพุ่ง
            if (direction != Vector3.zero)
            {
                dashParticle.transform.rotation = Quaternion.LookRotation(-direction);
            }

            // 2. สั่งเล่น Particle
            dashParticle.Play();
            dashParticle.transform.position = UpdatePosition(-direction,dashOffSet);

            _audioSource.clip = null;
            //_audioSource.clip = dashClip;
            _audioSource.loop = false;
            //_audioSource.Play();

            _audioSource.PlayOneShot(dashClip);
        }
        else
        {
            // เมื่อหยุด Dash ก็สั่งหยุด (Particle จะค่อยๆ จางหายไปตาม Lifetime ของมัน)
            //dashParticle.Stop();
        }
    }

    private Vector3 UpdatePosition(Vector3 direction, Vector3 offset)
    {
        if (direction == Vector3.zero) return transform.position;

        // คำนวณ Offset ตามทิศทาง
        // เอาทิศทางมาทำเป็นมุมหมุน * ค่า Offset ที่ตั้งไว้
        Vector3 rotatedOffset = Quaternion.LookRotation(direction) * offset;

        Vector3 pos = transform.position + rotatedOffset;

        return pos;
    }
}