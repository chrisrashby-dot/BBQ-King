using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// Lives in the Backyard scene and opens the cleaning close-up when something asks for it.
    ///
    /// Unity concept: loading a scene "additively" adds it on top of the scenes already open
    /// instead of replacing them (design.md section 9.2). The backyard keeps running underneath,
    /// so nothing in it is lost while the player scrubs. The close-up scene has its own camera,
    /// which draws over the backyard camera's picture, and its own UI.
    ///
    /// While the close-up is open the backyard's HUD and toolbar are hidden, and the controls
    /// switch to the CloseUp map. When the close-up asks to close, this unloads it, hands the
    /// controls back and, if the object was finished, marks its job done.
    /// </summary>
    public class CloseUpLoader : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] CloseUpSession session;
        [SerializeField] JobBook jobBook;

        [Tooltip("The scene opened on top of the backyard. It must be listed in File > Build Profiles (Scene List).")]
        [SerializeField] string closeUpScene = "CloseUp_Cleaning";

        [Tooltip("Canvases hidden while a close-up is open, such as the backyard HUD and toolbar.")]
        [SerializeField] Canvas[] hideWhileOpen = new Canvas[0];

        /// <summary>True from asking for the close-up until it has been closed again.</summary>
        public bool IsOpen { get; private set; }

        void Awake()
        {
            // This project keeps ScriptableObject data between Play sessions, so start afresh.
            if (session != null) session.SetCurrent(null);
        }

        void OnEnable()
        {
            if (session == null) return;
            session.OpenRequested += Open;
            session.CloseRequested += Close;
        }

        void OnDisable()
        {
            if (session == null) return;
            session.OpenRequested -= Open;
            session.CloseRequested -= Close;
        }

        void Open(CleanableData what)
        {
            if (IsOpen || what == null) return;
            IsOpen = true;
            session.SetCurrent(what);
            if (input != null) input.EnterCloseUp();
            SetBackyardUiVisible(false);
            SceneManager.LoadSceneAsync(closeUpScene, LoadSceneMode.Additive);
        }

        void Close(bool finished)
        {
            if (!IsOpen) return;
            var what = session.Current;
            IsOpen = false;
            SceneManager.UnloadSceneAsync(closeUpScene);
            session.SetCurrent(null);
            if (input != null) input.LeaveCloseUp();
            SetBackyardUiVisible(true);

            // After the UI is back, so the job's completion message can show.
            if (finished && what != null && what.job != null && jobBook != null) jobBook.MarkDone(what.job);
        }

        void SetBackyardUiVisible(bool visible)
        {
            // Turning off the Canvas component hides it but keeps its scripts listening.
            foreach (var canvas in hideWhileOpen)
                if (canvas != null) canvas.enabled = visible;
        }
    }
}
