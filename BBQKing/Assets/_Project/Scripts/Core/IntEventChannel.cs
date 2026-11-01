using UnityEngine;

namespace BBQKing.Core
{
    /// <summary>An event channel that carries a whole number, such as the selected toolbar slot.</summary>
    [CreateAssetMenu(fileName = "IntEventChannel", menuName = "BBQ King/Events/Int Event Channel")]
    public class IntEventChannel : EventChannel<int> { }
}
