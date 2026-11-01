using UnityEngine;

namespace BBQKing.Core
{
    /// <summary>An event channel asking the UI to show a message panel.</summary>
    [CreateAssetMenu(fileName = "MessageEventChannel", menuName = "BBQ King/Events/Message Event Channel")]
    public class MessageEventChannel : EventChannel<MessageData> { }
}
