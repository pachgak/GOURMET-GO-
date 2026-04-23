using UnityEngine;

public class PlayerParticleController : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement _playerMovement;

    [Header("Sprint Particle")]
    [SerializeField] private ParticleSystem sprintParticle;
    [SerializeField] private Vector3 sprintOffSet;

    [Header("Dash Particle")]
    [SerializeField] private ParticleSystem dashParticle;
    [SerializeField] private Vector3 dashOffSet;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();

        sprintParticle.Stop();
        dashParticle.Stop();
    }

    private void OnEnable()
    {
        if (_playerMovement != null)
        {
            _playerMovement.OnSprinteStateChange += HandleSprintParticle;
            _playerMovement.OnLastMoveDirectionChange += HandleSprintRotation;
            _playerMovement.OnDashStateChange += HandleDashParticle;
        }
    }

    private void OnDisable()
    {
        if (_playerMovement != null)
        {
            _playerMovement.OnSprinteStateChange -= HandleSprintParticle;
            _playerMovement.OnLastMoveDirectionChange -= HandleSprintRotation;
            _playerMovement.OnDashStateChange -= HandleDashParticle;
        }
    }

    private void HandleSprintParticle(bool isSprinting)
    {
        if (sprintParticle == null) return;

        if (isSprinting)
        {
            sprintParticle.Play();
        }
        else
        {
            sprintParticle.Stop();
        }
    }

    private void HandleSprintRotation(Vector3 direction)
    {
        if (sprintParticle == null) return;

        if (direction != Vector3.zero)
        {
            sprintParticle.transform.rotation = Quaternion.LookRotation(-direction);
            sprintParticle.transform.position = UpdatePosition(-direction, sprintOffSet);
        }
    }

    private void HandleDashParticle(bool isDashing, Vector3 direction)
    {
        if (dashParticle == null) return;

        if (isDashing)
        {
            if (direction != Vector3.zero)
            {
                dashParticle.transform.rotation = Quaternion.LookRotation(-direction);
            }

            dashParticle.Play();
            dashParticle.transform.position = UpdatePosition(-direction, dashOffSet);
        }
    }

    private Vector3 UpdatePosition(Vector3 direction, Vector3 offset)
    {
        if (direction == Vector3.zero) return transform.position;

        Vector3 rotatedOffset = Quaternion.LookRotation(direction) * offset;
        return transform.position + rotatedOffset;
    }
}