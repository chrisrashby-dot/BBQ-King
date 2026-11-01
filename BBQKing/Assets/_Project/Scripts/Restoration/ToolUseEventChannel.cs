using BBQKing.Core;
using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>An event channel for tool uses, so the player and the clearing system need no reference to each other.</summary>
    [CreateAssetMenu(fileName = "ToolUseChannel", menuName = "BBQ King/Events/Tool Use Event Channel")]
    public class ToolUseEventChannel : EventChannel<ToolUse> { }
}
