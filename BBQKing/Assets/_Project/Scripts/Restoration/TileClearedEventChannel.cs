using BBQKing.Core;
using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>An event channel announcing each cleared tile.</summary>
    [CreateAssetMenu(fileName = "TileClearedChannel", menuName = "BBQ King/Events/Tile Cleared Event Channel")]
    public class TileClearedEventChannel : EventChannel<TileCleared> { }
}
