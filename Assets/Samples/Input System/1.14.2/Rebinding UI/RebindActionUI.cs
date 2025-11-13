using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

////TODO: localization support

////TODO: deal with composites that have parts bound in different control schemes

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    /// <summary>
    /// A reusable component with a self-contained UI for rebinding a single action.
    /// </summary>
    public class RebindActionUI : MonoBehaviour
    {
        private const string nopePath = "<Touchscreen>/touch9/tap";
        /// <summary>
        /// Reference to the action that is to be rebound.
        /// </summary>
        public InputActionReference actionReference
        {
            get => m_Action;
            set
            {
                m_Action = value;
                UpdateActionLabel();
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// ID (in string form) of the binding that is to be rebound on the action.
        /// </summary>
        /// <seealso cref="InputBinding.id"/>
        public string bindingId
        {
            get => m_BindingId;
            set
            {
                m_BindingId = value;
                UpdateBindingDisplay();
            }
        }

        public InputBinding.DisplayStringOptions displayStringOptions
        {
            get => m_DisplayStringOptions;
            set
            {
                m_DisplayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Text component that receives the name of the action. Optional.
        /// </summary>
        public TMP_Text actionLabel
        {
            get => m_ActionLabel;
            set
            {
                m_ActionLabel = value;
                UpdateActionLabel();
            }
        }

        /// <summary>
        /// Text component that receives the display string of the binding. Can be <c>null</c> in which
        /// case the component entirely relies on <see cref="updateBindingUIEvent"/>.
        /// </summary>
        public TMP_Text bindingText
        {
            get => m_BindingText;
            set
            {
                m_BindingText = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Optional text component that receives a text prompt when waiting for a control to be actuated.
        /// </summary>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindOverlay"/>
        public TMP_Text rebindPrompt
        {
            get => m_RebindText;
            set => m_RebindText = value;
        }

        /// <summary>
        /// Optional UI that is activated when an interactive rebind is started and deactivated when the rebind
        /// is finished. This is normally used to display an overlay over the current UI while the system is
        /// waiting for a control to be actuated.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="rebindPrompt"/> nor <c>rebindOverlay</c> is set, the component will temporarily
        /// replaced the <see cref="bindingText"/> (if not <c>null</c>) with <c>"Waiting..."</c>.
        /// </remarks>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindPrompt"/>
        public GameObject rebindOverlay
        {
            get => m_RebindOverlay;
            set => m_RebindOverlay = value;
        }

        /// <summary>
        /// Event that is triggered every time the UI updates to reflect the current binding.
        /// This can be used to tie custom visualizations to bindings.
        /// </summary>
        public UpdateBindingUIEvent updateBindingUIEvent
        {
            get
            {
                if (m_UpdateBindingUIEvent == null)
                    m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
                return m_UpdateBindingUIEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind is started on the action.
        /// </summary>
        public InteractiveRebindEvent startRebindEvent
        {
            get
            {
                if (m_RebindStartEvent == null)
                    m_RebindStartEvent = new InteractiveRebindEvent();
                return m_RebindStartEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind has been completed or canceled.
        /// </summary>
        public InteractiveRebindEvent stopRebindEvent
        {
            get
            {
                if (m_RebindStopEvent == null)
                    m_RebindStopEvent = new InteractiveRebindEvent();
                return m_RebindStopEvent;
            }
        }

        /// <summary>
        /// When an interactive rebind is in progress, this is the rebind operation controller.
        /// Otherwise, it is <c>null</c>.
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

        /// <summary>
        /// Return the action and binding index for the binding that is targeted by the component
        /// according to
        /// </summary>
        /// <param name="action"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;

            action = m_Action?.action;
            if (action == null)
                return false;

            if (string.IsNullOrEmpty(m_BindingId))
                return false;

            // Look up binding index.
            var bindingId = new Guid(m_BindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Trigger a refresh of the currently displayed binding.
        /// </summary>
        public void UpdateBindingDisplay()
        {
            var displayString = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            // Get display string from action.
            var action = m_Action?.action;
            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
                if (bindingIndex != -1)
                {
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);

                    // [!!! LOGIC ใหม่เริ่มตรงนี้ !!!]
                    if (action.bindings[bindingIndex].isComposite)
                    {
                        var parts = new List<string>();
                        bool allPartsAreNope = true;

                        // วนลูปเช็ค "Part" ที่อยู่ถัดไป (เช่น Up, Down, Left, Right)
                        for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
                        {
                            // 1. ตรวจสอบ Path ดิบของ Part (index i)
                            string partPath = GetBindingPath(action, i);

                            if (partPath == nopePath)
                            {
                                // 2. ถ้าเป็น nopePath ให้ใช้ "..." (หรือ "" ถ้าไม่อยากให้เห็น)
                                parts.Add("...");
                            }
                            else
                            {
                                // 3. ถ้ามี Key (เช่น Arrow Up) ให้ดึง Display String ของ *Part* นั้น (index i)
                                // (เราเรียก GetBindingDisplayString บน Part Index ไม่ใช่ Composite Index)
                                string partDisplayString = action.GetBindingDisplayString(i, out _, out _, displayStringOptions);
                                parts.Add(partDisplayString);
                                allPartsAreNope = false; // มีอย่างน้อยหนึ่ง Part ที่ถูกตั้งค่า
                            }
                        }

                        // 4. ประกอบร่าง String
                        if (allPartsAreNope && parts.Count > 0)
                        {
                            displayString = ""; // ถ้าทุกช่องเป็น "..." ให้แสดงค่าว่าง
                        }
                        else if (parts.Count > 0)
                        {
                            // เชื่อมกัน: ผลลัพธ์ที่ได้คือ "Up / ... / ... / ..."
                            displayString = string.Join(" / ", parts);
                        }
                    }
                    else
                    {
                        if (GetBindingPath(action, bindingIndex) == nopePath)
                        {
                            displayString = ""; // ตั้งค่า Display String เป็นค่าว่าง
                        }
                    }
                    // [!!! LOGIC ใหม่สิ้นสุดตรงนี้ !!!]


                    ////if (displayString == nopePath) displayString = "";
                    //if (GetBindingPath(action, bindingIndex) == nopePath)
                    //{
                    //    displayString = ""; // ตั้งค่า Display String เป็นค่าว่าง
                    //}

                    //Debug.Log($"displayString :{displayString}");
                    //Debug.Log($"BindingPath :{GetBindingPath(action, bindingIndex)}");

                    //if (action.bindings[bindingIndex].isPartOfComposite)
                    //{

                    //}
                }


            }



            // Set on label (if any).
            if (m_BindingText != null)
                m_BindingText.text = displayString;

            // Give listeners a chance to configure UI in response.
            m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        private string GetBindingPath(InputAction action, int bindingIndex)
        {
            var currentBinding = action.bindings[bindingIndex];

            string pathToCheck = currentBinding.overridePath; // ตรวจสอบ Override Path ก่อน

            // ถ้าไม่มี overridePath ให้ใช้ effectivePath หรือ path ดั้งเดิม
            if (string.IsNullOrEmpty(pathToCheck))
            {
                pathToCheck = currentBinding.effectivePath;
            }
            // ถ้ายังไม่มี effectivePath ก็ใช้ path ดั้งเดิมที่กำหนดใน Asset
            if (string.IsNullOrEmpty(pathToCheck))
            {
                pathToCheck = currentBinding.path;
            }

            return pathToCheck;
        }

        /// <summary>
        /// Remove currently applied binding overrides.
        /// </summary>
        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;


            if (action.bindings[bindingIndex].isComposite)
            {
                // It's a composite. Remove overrides from part bindings.
                for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    ResetBinding(action, i);    //action.RemoveBindingOverride(i);
            }
            else
            {
                ResetBinding(action, bindingIndex);   //action.RemoveBindingOverride(bindingIndex);
            }

            UpdateBindingDisplay();
        }

        private void ResetBinding(InputAction action, int bindinfIndex)
        {
            InputBinding newBinding = action.bindings[bindinfIndex];
            string oldOverridePath = newBinding.overridePath;

            action.RemoveBindingOverride(bindinfIndex);

            foreach (InputAction otherAction in action.actionMap.actions)
            {
                if (otherAction == action)
                {
                    continue;
                }

                for (int i = 0; i < otherAction.bindings.Count; i++)
                {
                    InputBinding binding = otherAction.bindings[i];
                    if (binding.overridePath == newBinding.path)
                    {
                        otherAction.ApplyBindingOverride(i, oldOverridePath);
                    }
                }
            }
        }

        /// <summary>
        /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
        /// for the action.
        /// </summary>
        public void StartInteractiveRebind()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.

            string oldPath = GetBindingPath(action, bindingIndex); // ใช้เมธอด GetBindingPath ที่เราสร้างไว้

            void CleanUp()
            {
                m_RebindOperation?.Dispose();
                m_RebindOperation = null;

                action.actionMap.Enable();
                m_UIInputActionMap?.Enable();
            }

            // An "InvalidOperationException: Cannot rebind action x while it is enabled" will
            // be thrown if rebinding is attempted on an action that is enabled.
            //
            // On top of disabling the target action while rebinding, it is recommended to
            // disable any actions (or action maps) that could interact with the rebinding UI
            // or gameplay - it would be undesirable for rebinding to cause the player
            // character to jump.
            //
            // In this example, we explicitly disable both the UI input action map and
            // the action map containing the target action.
            action.actionMap.Disable();
            m_UIInputActionMap?.Disable();

            // Configure the rebind.
            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                //.WithControlsExcluding("<Mouse>/leftbutton") // เพิ่มเม้าที่ไม่ให้ตั้งค่า Key Builing ได้
                //.WithControlsExcluding("<Keyboard>/delete") // เพิ่มเม้าที่ไม่ให้ตั้งค่า Key Builing ได้
                .WithCancelingThrough("<Keyboard>/escape") // เพิ่มปุ่มที่ไม่ให้ตั้งค่า Key Builing ได้ (ปุ่ม escape)
                .OnCancel(
                    operation =>
                    {
                        m_RebindStopEvent?.Invoke(this, operation);
                        if (m_RebindOverlay != null)
                            m_RebindOverlay.SetActive(false);
                        UpdateBindingDisplay();
                        CleanUp();
                    })
                .OnComplete(
                    operation =>
                    {
                        if (m_RebindOverlay != null)
                            m_RebindOverlay.SetActive(false);
                        m_RebindStopEvent?.Invoke(this, operation);

                        if (CheckDuplicateBindings(action, bindingIndex, allCompositeParts))
                        {
                            //action.RemoveBindingOverride(bindingIndex);
                            //CleanUp();

                            action.ApplyBindingOverride(bindingIndex, oldPath);
                            PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
                            if (m_RebindText != null)
                            {
                                m_RebindText.text += $"\nKey already in use! Try again...";
                            }
                            return;
                        }

                        UpdateBindingDisplay();
                        CleanUp();

                        // If there's more composite parts we should bind, initiate a rebind
                        // for the next part.
                        if (allCompositeParts)
                        {
                            var nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                        }
                    });

            // If it's a part binding, show the name of the part in the UI.
            var partName = default(string);

            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";
            else
                partName = $"Binding '{action.name}'. ";

            Debug.Log($"partName : {partName}");

            // Bring up rebind overlay, if we have one.
            m_RebindOverlay?.SetActive(true);
            if (m_RebindText != null)
            {
                var text = !string.IsNullOrEmpty(m_RebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {m_RebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                m_RebindText.text = text;
            }

            // If we have no rebind overlay and no callback but we have a binding text label,
            // temporarily set the binding text label to "<Waiting>".
            if (m_RebindOverlay == null && m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
                m_BindingText.text = "<Waiting...>";

            // Give listeners a chance to act on the rebind starting.
            m_RebindStartEvent?.Invoke(this, m_RebindOperation);

            m_RebindOperation.Start();
        }

        private bool CheckDuplicateBindings(InputAction action, int bindingIndex, bool allCompoiteParts = false)
        {
            InputBinding newBinding = action.bindings[bindingIndex];

            foreach (InputBinding binding in action.actionMap.bindings)
            {
                //if (binding.action == newBinding.action)
                //{
                //    continue;
                //} 

                // [แก้ไข] 2. ข้ามเฉพาะถ้ามันคือ "Binding เดียวกัน" (เทียบด้วย ID)
                if (binding.id == newBinding.id)
                {
                    continue;
                }

                // [แก้ไข] 3. ข้าม Binding อื่นๆ ที่เป็นค่าว่าง (nopePath) ด้วย
                if (string.IsNullOrEmpty(binding.effectivePath) || binding.effectivePath == nopePath)
                {
                    continue;
                }

                if (binding.effectivePath == newBinding.effectivePath)
                {
                    Debug.Log($"Dup1icate binding found: {newBinding.effectivePath}");
                    return true;
                }
            }

            if (allCompoiteParts)
            {
                for (int i = 0; i < bindingIndex; i++)
                {
                    if (action.bindings[i].effectivePath == newBinding.overridePath)
                    {
                        Debug.Log($"Dup1icate binding found: {newBinding.effectivePath}");
                        return true;
                    }
                }
            }


            return false;
        }

        protected void OnEnable()
        {
            if (s_RebindActionUIs == null)
                s_RebindActionUIs = new List<RebindActionUI>();
            s_RebindActionUIs.Add(this);
            if (s_RebindActionUIs.Count == 1)
                InputSystem.onActionChange += OnActionChange;
            if (m_DefaultInputActions != null && m_UIInputActionMap == null)
                m_UIInputActionMap = m_DefaultInputActions.FindActionMap("UI");
        }

        protected void OnDisable()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            s_RebindActionUIs.Remove(this);
            if (s_RebindActionUIs.Count == 0)
            {
                s_RebindActionUIs = null;
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        // When the action system re-resolves bindings, we want to update our UI in response. While this will
        // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
        // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
        // will update our UI to reflect the current keyboard layout.
        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            for (var i = 0; i < s_RebindActionUIs.Count; ++i)
            {
                var component = s_RebindActionUIs[i];
                var referencedAction = component.actionReference?.action;
                if (referencedAction == null)
                    continue;

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                    component.UpdateBindingDisplay();
            }
        }

        [Tooltip("Reference to action that is to be rebound from the UI.")]
        [SerializeField]
        private InputActionReference m_Action;

        [SerializeField]
        private string m_BindingId;

        [SerializeField]
        private InputBinding.DisplayStringOptions m_DisplayStringOptions;

        [Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the "
            + "rebind UI not show a label for the action.")]
        [SerializeField]
        private TMP_Text m_ActionLabel;

        [Tooltip("Text label that will receive the current, formatted binding string.")]
        [SerializeField]
        private TMP_Text m_BindingText;

        [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
        [SerializeField]
        private GameObject m_RebindOverlay;

        [Tooltip("Optional text label that will be updated with prompt for user input.")]
        [SerializeField]
        private TMP_Text m_RebindText;


        [Tooltip("OptionaI boot field which allows you to OVERRIDE the action label with your own text")]
        public bool m_OverRideActionLabel;

        [Tooltip("What text shoudld be displayed for the action Label?")]
        [SerializeField]
        private string m_ActionLabelString;

        [Tooltip("Optional reference to default input actions containing the UI action map. The UI action map is "
            + "disabled when rebinding is in progress.")]
        [SerializeField]
        private InputActionAsset m_DefaultInputActions;
        private InputActionMap m_UIInputActionMap;

        [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
            + "bindings in custom ways, e.g. using images instead of text.")]
        [SerializeField]
        private UpdateBindingUIEvent m_UpdateBindingUIEvent;

        [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
            + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
            + "customize the rebind.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStartEvent;

        [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

        private static List<RebindActionUI> s_RebindActionUIs;

        // We want the label for the action name to update in edit mode, too, so
        // we kick that off from here.
#if UNITY_EDITOR
        protected void OnValidate()
        {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }

#endif

        private void UpdateActionLabel()
        {
            if (m_ActionLabel != null)
            {
                var action = m_Action?.action;

                if (m_OverRideActionLabel)
                {
                    m_ActionLabel.text = m_ActionLabelString;
                }
                else
                {
                    m_ActionLabel.text = action != null ? action.name : string.Empty;
                    m_ActionLabelString = string.Empty;
                }

            }
        }

        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
        {
        }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
        {
        }
    }
}
