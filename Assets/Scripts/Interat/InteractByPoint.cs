using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class InteractByPoint : MonoBehaviour
{
    public LayerMask whatInteractable;
    [Header("Setup")]
    public Vector3 overlapBoxHalfExtents; // <-- เพิ่มตัวแปรนี้เพื่อกำหนดขนาดกล่อง
    public float overlapBoxFart;

    [Header("Prompt")]
    [SerializeField] public GameObject interactUI;
    [SerializeField] public TMP_Text interactText;
    [SerializeField] public Image progressImg;

    //[HideInInspector] private InteractPrompt interactPrompt;
    //[HideInInspector] private Outline outline;

    [Header("System")]

    [SerializeField] private bool _isInteractable;
    [SerializeField] private InteractableBase _nearInteractable;
    private Vector3 _mousePosition;

    //[HideInInspector] protected KdTree<Collider> collidersCheck = new KdTree<Collider>();
    [HideInInspector] private Collider newCollider;
    [HideInInspector] private Collider oldCollider;

    private bool _isUiOpening = false;

    private void OnEnable()
    {
        PlayerInputActionsManager.instance.OnInteractInputDown += HandleInteractInputDown;
        PlayerInputActionsManager.instance.OnInteractInputUp += HandleInteractInputUp;
        PlayerInputActionsManager.instance.OnMountPosition += HandleGetMountPos;

        OpenUiManager.instance.OnUiOpeningStateChange += HandleUiOpeningStateChange;

        if (_nearInteractable != null) interactUI.SetActive(true);
    }

    private void OnDisable()
    {
        PlayerInputActionsManager.instance.OnInteractInputDown -= HandleInteractInputDown;
        PlayerInputActionsManager.instance.OnInteractInputUp -= HandleInteractInputUp;
        PlayerInputActionsManager.instance.OnMountPosition += HandleGetMountPos;

        OpenUiManager.instance.OnUiOpeningStateChange -= HandleUiOpeningStateChange;

        if (interactUI != null) interactUI.SetActive(false);
    }

    private void HandleInteractInputDown()
    {
        if (_nearInteractable == null || _isUiOpening) return;

        _nearInteractable.Interact();
        if (_nearInteractable.hasDuration) _nearInteractable.Progress.AddListener(onProgress);
    }

    private void HandleInteractInputUp()
    {
        if (_nearInteractable == null || _isUiOpening) return;

        _nearInteractable.EndInteract();
        _nearInteractable.Progress?.Invoke(0f);
        if (_nearInteractable.hasDuration) _nearInteractable.Progress.RemoveListener(onProgress);

        if (_nearInteractable.hasDuration && !_nearInteractable.isResetTimer) progressImg.fillAmount = _nearInteractable.timer / _nearInteractable.duration;
    }

    private void HandleGetMountPos(Vector3 mousePosition)
    {
        _mousePosition = mousePosition;
    }

    private void Start()
    {
        interactText.gameObject.SetActive(false);
        interactUI.SetActive(false);
    }

    private void HandleUiOpeningStateChange(bool isUiOpeningState)
    {
        _isUiOpening = isUiOpeningState;

        if(_isUiOpening && _nearInteractable != null) interactUI.SetActive(false);
        else if (_nearInteractable != null) interactUI.SetActive(true);
    }

    private void Update()
    {

        //interact
        CheckInteract();

        if (_nearInteractable == null) return;

        if (interactUI.gameObject.activeSelf)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(_nearInteractable.transform.position + _nearInteractable.offSetInteractText);
            if (interactUI.transform.position != pos) interactUI.transform.position = pos;
        }
    }

    private void CheckInteract()
    {
        if (_isUiOpening) return;

        // คำนวณทิศทางจากผู้เล่นไปยังเมาส์ และ คำนวณตำแหน่งกึ่งกลางของกล่อง OverlapBox
        Vector3 directionToMouse = (_mousePosition - transform.position).normalized;
        Vector3 overlapCenter = transform.position + directionToMouse * (overlapBoxFart);

        Collider[] hitColliders = Physics.OverlapBox(overlapCenter, overlapBoxHalfExtents, Quaternion.LookRotation(directionToMouse), whatInteractable);


        if (hitColliders.Length > 0)
        {
            _isInteractable = true;

            Collider newCollider = hitColliders.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).FirstOrDefault();

            //ผู้เล่นไม่สามารถ interact กับสิ่งนี้ได้ถ้าอยู่หลังกำแพง    //if (!GameManager.instance.player.CheckedPlayerNotBehindWall(newCollider.transform)) return; 

            if (oldCollider != newCollider) //����ҵ�ʹ���� ��� ����ѹ���������ѧ�����ѹ���������� ���������繤����������ҨШҡ�����ҧ�����ѹ��ҡ���
            {
                _nearInteractable = newCollider.GetComponent<InteractableBase>();
                if (_nearInteractable != null)
                {
                    //��� Prompt �Ѻ Outline
                    interactText.text = _nearInteractable.message;
                    interactText.gameObject.SetActive(true);
                    interactUI.SetActive(true);

                    if (!_nearInteractable.hasDuration) progressImg.fillAmount = 1; 
                    else progressImg.fillAmount = _nearInteractable.timer / _nearInteractable.duration;
                }

                if (oldCollider != null)
                {
                    InteractableBase oldInteract = oldCollider.GetComponent<InteractableBase>();
                    oldInteract.Progress?.Invoke(0f);
                    oldInteract.Progress.RemoveListener(onProgress);
                }

                //Set End
                oldCollider = newCollider;
            }
        }
        else //reset inteact �������� interact
        {
            if (_isInteractable == true)
            {
                interactText.gameObject.SetActive(false);
                interactUI.SetActive(false);

                if (oldCollider != null)
                {
                    InteractableBase oldInteract = oldCollider.GetComponent<InteractableBase>();
                    oldInteract.EndInteract();
                    oldInteract.Progress?.Invoke(0f);
                    oldInteract.Progress.RemoveListener(onProgress);
                }
                oldCollider = new Collider();
                newCollider = new Collider();
                _nearInteractable = null;

                _isInteractable = false;
            }
        }



    }

    private void onProgress(float progress)
    {
        progressImg.fillAmount = progress;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 overlapCenter = transform.position + Vector3.forward * (overlapBoxFart);
        Gizmos.DrawWireCube(overlapCenter, overlapBoxHalfExtents * 2);
    }

}
