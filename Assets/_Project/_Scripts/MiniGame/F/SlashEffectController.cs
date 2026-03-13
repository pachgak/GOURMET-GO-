using UnityEngine;
using System.Collections;

public class SlashEffectController : MonoBehaviour
{
    [Header("Effects")]
    public GameObject slashEffect1;
    public GameObject slashEffect2;
    public float slashDuration = 0.15f;

    [Header("Rotation Settings")]
    public bool randomizeRotation = true;
    public float minRotationOffset = -20f;
    public float maxRotationOffset = 20f;

    private float _originRotationZ1;
    private float _originRotationZ2;

    // เพิ่มตัวแปรเก็บ Manager
    private MiniGameFManager _miniGameManager;

    void Awake()
    {
        if (slashEffect1 != null)
        {
            _originRotationZ1 = slashEffect1.transform.localEulerAngles.z;
            slashEffect1.SetActive(false);
        }

        if (slashEffect2 != null)
        {
            _originRotationZ2 = slashEffect2.transform.localEulerAngles.z;
            slashEffect2.SetActive(false);
        }

        // หา Manager แบบไม่ต้องลากใส่
        _miniGameManager = FindFirstObjectByType<MiniGameFManager>();
    }

    void OnEnable()
    {
        // เริ่มดักฟัง
        if (_miniGameManager != null)
        {
            _miniGameManager.OnSlashTriggered += PlaySlashEffect;
        }
    }

    void OnDisable()
    {
        // ยกเลิกดักฟัง
        if (_miniGameManager != null)
        {
            _miniGameManager.OnSlashTriggered -= PlaySlashEffect;
        }
    }

    public void PlaySlashEffect()
    {
        if (slashEffect1 != null) slashEffect1.SetActive(true);
        if (slashEffect2 != null) slashEffect2.SetActive(true);

        if (randomizeRotation)
        {
            float randomOffset = Random.Range(minRotationOffset, maxRotationOffset);

            if (slashEffect1 != null)
                slashEffect1.transform.localRotation = Quaternion.Euler(0, 0, _originRotationZ1 + randomOffset);

            if (slashEffect2 != null)
                slashEffect2.transform.localRotation = Quaternion.Euler(0, 0, _originRotationZ2 + randomOffset);
        }

        StopAllCoroutines();
        StartCoroutine(HideSlashEffectRoutine());
    }

    private IEnumerator HideSlashEffectRoutine()
    {
        yield return new WaitForSeconds(slashDuration);

        if (slashEffect1 != null) slashEffect1.SetActive(false);
        if (slashEffect2 != null) slashEffect2.SetActive(false);
    }
}