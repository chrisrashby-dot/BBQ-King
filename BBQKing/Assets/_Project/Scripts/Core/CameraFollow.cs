using UnityEngine;
using UnityEngine.Tilemaps;

namespace BBQKing.Core
{
    /// <summary>
    /// Keeps the camera centred on the player without showing anything beyond the edge of the map.
    /// The map edge is read from a tilemap, so it follows the backyard automatically if the map
    /// changes size. The Pixel Perfect Camera on the same object handles snapping to whole pixels.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Tooltip("What to follow, usually the player.")]
        [SerializeField] Transform target;

        [Tooltip("The camera never shows past the edges of this tilemap (usually Ground).")]
        [SerializeField] Tilemap boundsTilemap;

        Camera cam;
        Bounds worldBounds;

        void Awake()
        {
            cam = GetComponent<Camera>();
            RefreshBounds();
        }

        /// <summary>Call this if tiles are added beyond the old edge of the map.</summary>
        public void RefreshBounds()
        {
            if (boundsTilemap == null) return;
            boundsTilemap.CompressBounds(); // shrink to the tiles actually painted
            Bounds local = boundsTilemap.localBounds;
            worldBounds = new Bounds(boundsTilemap.transform.TransformPoint(local.center), local.size);
        }

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = Clamp(target.position);
        }

        /// <summary>Jumps straight to the target, for example after loading a scene.</summary>
        public void SnapToTarget()
        {
            if (target != null) transform.position = Clamp(target.position);
        }

        Vector3 Clamp(Vector3 wanted)
        {
            if (boundsTilemap == null) return new Vector3(wanted.x, wanted.y, transform.position.z);

            // An orthographic camera's size is half the visible height in world units (tiles).
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            return new Vector3(
                ClampAxis(wanted.x, worldBounds.min.x, worldBounds.max.x, halfWidth),
                ClampAxis(wanted.y, worldBounds.min.y, worldBounds.max.y, halfHeight),
                transform.position.z);
        }

        /// <summary>Keeps the view inside min..max. If the map is smaller than the view, centres on the map.</summary>
        static float ClampAxis(float wanted, float min, float max, float halfView)
        {
            if (max - min <= halfView * 2f) return (min + max) * 0.5f;
            return Mathf.Clamp(wanted, min + halfView, max - halfView);
        }
    }
}
