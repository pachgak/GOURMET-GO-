using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemySoundController : MonoBehaviour
{
    [Header("References")]
    // เปลี่ยนเป็น protected เพื่อให้คลาสลูกมองเห็น
    protected BaseEnemyMovement _enemyMovement;
    protected AudioSource _audioSource;

    [Header("Audio Clips")]
    [SerializeField] protected AudioClip walkClip;

    // ใส่ virtual เพื่อให้คลาสลูกสามารถ override (เพิ่มเติมการทำงาน) ได้
    protected virtual void Awake()
    {
        _enemyMovement = GetComponent<BaseEnemyMovement>();
        _audioSource = GetComponent<AudioSource>();
    }

    protected virtual void OnEnable()
    {
        if (_enemyMovement != null)
        {
            _enemyMovement.OnMoveStateChange += HandleWalkSound;
        }
    }

    protected virtual void OnDisable()
    {
        if (_enemyMovement != null)
        {
            _enemyMovement.OnMoveStateChange -= HandleWalkSound;
        }
    }

    protected virtual void HandleWalkSound(bool isMoving)
    {
        if (isMoving)
        {
            if (!_audioSource.isPlaying || _audioSource.clip != walkClip)
            {
                _audioSource.clip = walkClip;
                _audioSource.loop = true;
                _audioSource.Play();
            }
        }
        else
        {
            if (_audioSource.isPlaying && _audioSource.clip == walkClip)
            {
                _audioSource.Stop();
            }
        }
    }
}