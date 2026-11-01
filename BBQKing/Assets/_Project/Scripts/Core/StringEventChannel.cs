using UnityEngine;

namespace BBQKing.Core
{
    /// <summary>An event channel that carries a piece of text, such as the interaction prompt.</summary>
    [CreateAssetMenu(fileName = "StringEventChannel", menuName = "BBQ King/Events/String Event Channel")]
    public class StringEventChannel : EventChannel<string> { }
}
