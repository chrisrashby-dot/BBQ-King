using BBQKing.Restoration;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.UI
{
    /// <summary>
    /// The current job in the top-left corner, for example "Clear the patio" with "38 left" below
    /// (design.md section 6.2: the HUD shows the current job). Updates whenever the job book changes.
    /// </summary>
    public class JobHudUI : MonoBehaviour
    {
        [SerializeField] JobBook jobBook;
        [SerializeField] Text titleLabel;
        [SerializeField] Text progressLabel;

        [Tooltip("Progress text for clearing jobs. {0} is replaced with the number of tiles left.")]
        [SerializeField] string leftFormat = "{0} left";

        [Tooltip("Shown when every job is done.")]
        [SerializeField] string allDoneText = "All jobs done";

        void OnEnable()
        {
            if (jobBook != null) jobBook.Changed += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (jobBook != null) jobBook.Changed -= Refresh;
        }

        void Refresh()
        {
            var job = jobBook != null ? jobBook.CurrentJob : null;
            if (titleLabel != null) titleLabel.text = job != null ? job.title : allDoneText;

            int left = job != null ? jobBook.RemainingIn(job) : -1;
            if (progressLabel != null) progressLabel.text = left > 0 ? string.Format(leftFormat, left) : string.Empty;
        }
    }
}
