using UnityEngine;

namespace BBQKing.Core
{
    /// <summary>
    /// A short piece of in-game writing shown in a panel: a letter, a sign, a note on a door.
    /// Kept in Data/Messages so the text can be edited without touching code or scenes.
    /// Later, the relative's recipe cards (design.md section 4.4) can use the same idea.
    /// </summary>
    [CreateAssetMenu(fileName = "Message", menuName = "BBQ King/Message")]
    public class MessageData : ScriptableObject
    {
        public string title;

        [TextArea(6, 20)]
        public string body;
    }
}
