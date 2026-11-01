using System.Text;
using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.UI
{
    /// <summary>
    /// The jobs board (design.md section 6.2), opened with Tab: every job with its status, and a
    /// description of the ones the player can work on. While it is open the controls switch to the
    /// UI map, like the letter; Tab, Escape, E or the right mouse button close it.
    ///
    /// The list is written as one block of rich text (Unity's Text component understands simple
    /// tags such as &lt;b&gt; and &lt;color&gt;), which is plenty for a placeholder.
    /// </summary>
    public class JobsBoardUI : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] JobBook jobBook;

        [SerializeField] GameObject panel;
        [SerializeField] Text listLabel;

        [Header("Wording")]
        [SerializeField] string doneText = "Done";
        [SerializeField] string availableText = "To do";
        [SerializeField] string lockedText = "Locked";
        [Tooltip("Added after \"To do\" for clearing jobs. {0} is the number of tiles left.")]
        [SerializeField] string leftFormat = "{0} left";

        [Header("Colours")]
        [SerializeField] Color doneColour = new Color32(0x4f, 0x7a, 0x3a, 0xff);
        [SerializeField] Color lockedColour = new Color32(0x8f, 0x7d, 0x66, 0xff);

        public bool IsOpen => panel != null && panel.activeSelf;

        void OnEnable()
        {
            if (input != null)
            {
                input.JournalPressed += Open;
                input.UICancelPressed += Close;
                input.SubmitPressed += Close;
            }
            if (jobBook != null) jobBook.Changed += Refresh;
        }

        void OnDisable()
        {
            if (input != null)
            {
                input.JournalPressed -= Open;
                input.UICancelPressed -= Close;
                input.SubmitPressed -= Close;
            }
            if (jobBook != null) jobBook.Changed -= Refresh;
        }

        void Open()
        {
            if (panel == null || IsOpen) return;
            Refresh();
            panel.SetActive(true);
            if (input != null) input.EnableUIInput();
        }

        void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            if (input != null) input.EnablePlayerInput();
        }

        void Refresh()
        {
            if (listLabel == null || jobBook == null) return;

            var text = new StringBuilder();
            foreach (var job in jobBook.Jobs)
            {
                if (job == null) continue;
                var status = jobBook.StatusOf(job);
                if (text.Length > 0) text.Append("\n\n");

                switch (status)
                {
                    case JobStatus.Done:
                        text.Append(Coloured($"{job.title}: {doneText}", doneColour));
                        break;
                    case JobStatus.Locked:
                        text.Append(Coloured($"{job.title}: {lockedText}", lockedColour));
                        break;
                    default:
                        int left = jobBook.RemainingIn(job);
                        string progress = left > 0 ? ", " + string.Format(leftFormat, left) : string.Empty;
                        text.Append($"<b>{job.title}: {availableText}{progress}</b>\n{job.description}");
                        break;
                }
            }
            listLabel.text = text.ToString();
        }

        static string Coloured(string s, Color c) => $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{s}</color>";
    }
}
