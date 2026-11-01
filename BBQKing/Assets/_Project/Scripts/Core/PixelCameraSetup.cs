using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BBQKing.Core
{
    /// <summary>
    /// Keeps the Pixel Perfect Camera's reference resolution in step with <see cref="DisplayConfig"/>.
    ///
    /// Unity concept: the Pixel Perfect Camera (part of URP) works out the camera's zoom from a
    /// reference resolution and the sprites' Pixels Per Unit, then snaps everything to whole art
    /// pixels. We only feed it the resolution; it does the rest every frame.
    /// </summary>
    [RequireComponent(typeof(PixelPerfectCamera))]
    public class PixelCameraSetup : MonoBehaviour
    {
        [SerializeField] DisplayConfig displayConfig;

        PixelPerfectCamera pixelCamera;

        void Awake() => pixelCamera = GetComponent<PixelPerfectCamera>();

        // LateUpdate runs after every Update, so a change made in the Inspector is applied
        // before the frame is drawn.
        void LateUpdate()
        {
            if (displayConfig == null) return;
            if (pixelCamera.refResolutionX != displayConfig.Width) pixelCamera.refResolutionX = displayConfig.Width;
            if (pixelCamera.refResolutionY != displayConfig.Height) pixelCamera.refResolutionY = displayConfig.Height;
        }
    }
}
