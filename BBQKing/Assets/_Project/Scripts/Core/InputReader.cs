using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BBQKing.Core
{
    /// <summary>
    /// The one place the game reads the player's input from.
    ///
    /// Unity concept: a ScriptableObject is a script whose data lives in an asset file (here
    /// Data/Input/InputReader.asset) rather than on an object in a scene. Every script that needs
    /// input is given a reference to the same asset, so they all share one set of controls.
    ///
    /// The actions themselves are defined in Input/BBQKingControls.inputactions. Unity generates
    /// the BBQKingControls C# class from that file (see "Generate C# Class" on the asset), and this
    /// reader turns it into simple values and C# events, so gameplay scripts never deal with
    /// Input System details or care whether a keyboard or a gamepad is in use.
    ///
    /// Three action maps are used:
    /// - Player: walking about the backyard (move, interact, tools and so on).
    /// - UI: while a panel such as a letter is open (submit and cancel only).
    /// - CloseUp: while a close-up such as cleaning the grill is open (scrub, aim and leave).
    /// Only one map is active at a time: a panel wins over a close-up, which wins over walking.
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "BBQ King/Input Reader")]
    public class InputReader : ScriptableObject, BBQKingControls.IPlayerActions, BBQKingControls.IUIActions,
        BBQKingControls.ICloseUpActions
    {
        // ----- Player map
        /// <summary>Interact key (E) pressed: pick up, use, open a close-up.</summary>
        public event Action InteractPressed;
        /// <summary>Right mouse button pressed: cancel or put down.</summary>
        public event Action CancelPressed;
        /// <summary>Escape pressed: open the pause menu or leave a close-up.</summary>
        public event Action PausePressed;
        /// <summary>Tab pressed: jobs board and journal.</summary>
        public event Action JournalPressed;
        /// <summary>Space pressed: fast-forward to the next check-in during long cooks.</summary>
        public event Action FastForwardPressed;
        /// <summary>A number key 1 to 6 pressed. The value is the slot number.</summary>
        public event Action<int> ToolSlotSelected;
        /// <summary>Mouse wheel or shoulder buttons: +1 for next tool, -1 for previous.</summary>
        public event Action<int> ToolCycled;

        // ----- UI map
        /// <summary>Confirm in a panel (E, Enter, Space or gamepad A).</summary>
        public event Action SubmitPressed;
        /// <summary>Close a panel (Escape, right mouse button or gamepad B).</summary>
        public event Action UICancelPressed;

        // ----- CloseUp map
        /// <summary>Leave the close-up (Escape or gamepad B).</summary>
        public event Action LeaveCloseUpPressed;

        /// <summary>True while the scrub button (left mouse) is held in a close-up.</summary>
        public bool ScrubHeld { get; private set; }

        /// <summary>Movement direction, length 0 to 1. Zero while a panel is open.</summary>
        public Vector2 Move { get; private set; }

        /// <summary>True while the use-tool button (left mouse) is held.</summary>
        public bool UseToolHeld { get; private set; }

        /// <summary>
        /// Where the mouse pointer is, in screen pixels. Only meaningful when <see cref="HasAim"/>
        /// is true; with a gamepad there is no pointer and tools aim where the player faces.
        /// </summary>
        public Vector2 AimScreenPosition { get; private set; }

        /// <summary>
        /// True while the mouse is the device in use: set when the mouse moves, cleared when a
        /// gamepad is used, so tools go back to aiming where the player faces.
        /// </summary>
        public bool HasAim { get; private set; }

        /// <summary>True while the Player map is on (no panel or close-up open).</summary>
        public bool IsPlayerInputEnabled => controls != null && controls.Player.enabled;

        /// <summary>True while the CloseUp map is on (a close-up is open and no panel is over it).</summary>
        public bool IsCloseUpInputEnabled => controls != null && controls.CloseUp.enabled;

        /// <summary>
        /// The key shown in prompts for Interact, such as "E". Read from the input asset, so the
        /// prompt stays right if the binding changes. Keyboard only for now.
        /// </summary>
        public string InteractKeyLabel => interactKeyLabel ??=
            Controls.Player.Interact.GetBindingDisplayString(InputBinding.MaskByGroup(KeyboardMouseGroup));

        string interactKeyLabel; // worked out once, then reused every frame

        // The binding group (control scheme) name used in BBQKingControls.inputactions.
        const string KeyboardMouseGroup = "Keyboard and Mouse";

        BBQKingControls controls;

        // How many panels currently need the UI map (a letter, the jobs board). Walking, or the
        // close-up, only comes back when the last of them closes.
        int openPanels;

        // True while a close-up scene is open over the backyard.
        bool inCloseUp;

        /// <summary>
        /// Creates the controls the first time anything asks for them. Done lazily rather than in
        /// OnEnable, because Unity also loads ScriptableObjects while you are editing, and we only
        /// want input running in Play mode.
        /// </summary>
        BBQKingControls Controls
        {
            get
            {
                if (controls == null)
                {
                    controls = new BBQKingControls();
                    // Tell the generated class to call this object's OnMove, OnInteract and so on.
                    controls.Player.SetCallbacks(this);
                    controls.UI.SetCallbacks(this);
                    controls.CloseUp.SetCallbacks(this);
                }
                return controls;
            }
        }

        /// <summary>
        /// Normal play: walking, interacting and using tools. Called when a scene starts; it also
        /// forgets any panels or close-up left open, since this asset keeps its state between Play
        /// sessions.
        /// </summary>
        public void EnablePlayerInput()
        {
            openPanels = 0;
            inCloseUp = false;
            ApplyMaps();
        }

        /// <summary>
        /// A panel has opened: only submit and cancel work, and the player stands still. Every call
        /// must be matched by one <see cref="ClosePanel"/>, so panels opened on top of each other
        /// do not hand walking back while one of them is still showing.
        /// </summary>
        public void OpenPanel()
        {
            openPanels++;
            ApplyMaps();
        }

        /// <summary>A panel has closed. Walking (or the close-up) comes back once no panel is open.</summary>
        public void ClosePanel()
        {
            if (openPanels > 0) openPanels--;
            ApplyMaps();
        }

        /// <summary>A close-up has opened over the backyard: scrubbing replaces walking.</summary>
        public void EnterCloseUp()
        {
            inCloseUp = true;
            ApplyMaps();
        }

        /// <summary>The close-up has closed: back to walking (unless a panel is open).</summary>
        public void LeaveCloseUp()
        {
            inCloseUp = false;
            ApplyMaps();
        }

        /// <summary>Turns all input off, for example while a scene is loading.</summary>
        public void DisableAllInput()
        {
            openPanels = 0;
            inCloseUp = false;
            HasAim = false;
            if (controls == null) return;
            controls.Disable();
            ClearHeldInput();
        }

        /// <summary>Switches on the one map that should be active now, and the others off.</summary>
        void ApplyMaps()
        {
            bool panel = openPanels > 0;
            bool closeUp = !panel && inCloseUp;
            bool player = !panel && !inCloseUp;

            // Turn maps off before turning the new one on, so no key belongs to two live maps.
            if (!panel) Controls.UI.Disable();
            if (!closeUp) Controls.CloseUp.Disable();
            if (!player) Controls.Player.Disable();
            if (panel) Controls.UI.Enable();
            if (closeUp) Controls.CloseUp.Enable();
            if (player) Controls.Player.Enable();

            // A disabled map stops reporting, so forget anything that was held down in it.
            if (!player) { Move = Vector2.zero; UseToolHeld = false; }
            if (!closeUp) ScrubHeld = false;
        }

        void ClearHeldInput()
        {
            Move = Vector2.zero;
            UseToolHeld = false;
            ScrubHeld = false;
        }

        // Called when the asset is unloaded (leaving Play mode with a domain reload, or quitting).
        void OnDisable()
        {
            if (controls == null) return;
            controls.Disable();
            controls.Dispose();
            controls = null;
        }

        // ------------------------------------------------------------------
        // Callbacks from the generated BBQKingControls class. Each action calls its method at the
        // "started", "performed" and "canceled" phases; buttons only need "performed".

        public void OnMove(InputAction.CallbackContext context)
        {
            NoteDevice(context);
            Move = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f); // diagonals are not faster
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            NoteDevice(context);
            if (context.performed) InteractPressed?.Invoke();
        }

        public void OnUseTool(InputAction.CallbackContext context)
        {
            NoteDevice(context);
            if (context.performed) UseToolHeld = true;
            else if (context.canceled) UseToolHeld = false;
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            AimScreenPosition = context.ReadValue<Vector2>();
            HasAim = true;
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            // This method is shared by the Player map and the UI map, because both define an
            // action called Cancel. Tell them apart by which map the action belongs to.
            if (!context.performed) return;
            if (context.action.actionMap == controls.UI.Get()) UICancelPressed?.Invoke();
            else CancelPressed?.Invoke();
        }

        public void OnSelectToolSlot(InputAction.CallbackContext context)
        {
            // Each number key has a Scale processor in the input asset, so key 3 reads as 3.
            if (context.performed) ToolSlotSelected?.Invoke(Mathf.RoundToInt(context.ReadValue<float>()));
        }

        public void OnCycleTool(InputAction.CallbackContext context)
        {
            // Only the direction matters: mouse wheels report different step sizes on different systems.
            if (!context.performed) return;
            float value = context.ReadValue<float>();
            if (value > 0f) ToolCycled?.Invoke(1);
            else if (value < 0f) ToolCycled?.Invoke(-1);
        }

        public void OnJournal(InputAction.CallbackContext context)
        {
            if (context.performed) JournalPressed?.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed) PausePressed?.Invoke();
        }

        public void OnFastForward(InputAction.CallbackContext context)
        {
            if (context.performed) FastForwardPressed?.Invoke();
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.performed) SubmitPressed?.Invoke();
        }

        public void OnScrub(InputAction.CallbackContext context)
        {
            NoteDevice(context);
            if (context.performed) ScrubHeld = true;
            else if (context.canceled) ScrubHeld = false;
        }

        public void OnLeave(InputAction.CallbackContext context)
        {
            if (context.performed) LeaveCloseUpPressed?.Invoke();
        }

        public void OnPoint(InputAction.CallbackContext context) { }  // read directly by the UI when needed
        public void OnClick(InputAction.CallbackContext context) { }  // read directly by the UI when needed

        /// <summary>
        /// Using a gamepad hands aiming back to the player's facing. The keyboard does not, because
        /// keyboard players walk with WASD and aim with the mouse at the same time.
        /// </summary>
        void NoteDevice(InputAction.CallbackContext context)
        {
            if (context.control != null && context.control.device is Gamepad) HasAim = false;
        }
    }
}
