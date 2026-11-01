using BBQKing.Core;
using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Top-down walking with WASD, the arrow keys or a gamepad stick (via the InputReader).
    ///
    /// Unity concept: a Rigidbody2D puts the player under 2D physics, so walls and fences block
    /// them automatically. We read input in Update (every frame) but move in FixedUpdate, which runs
    /// in step with the physics engine.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] PlayerConfig config;

        [Tooltip("Which way the player faces when the scene starts.")]
        [SerializeField] Facing startFacing = Facing.Down;

        public Facing Facing { get; private set; }
        public bool IsMoving { get; private set; }

        /// <summary>A one-tile vector pointing the way the player faces.</summary>
        public Vector2 FacingVector => FacingRules.ToVector(Facing);

        Rigidbody2D body;
        Vector2 move;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;     // top-down: nothing falls
            body.freezeRotation = true; // do not spin when bumping into things
            Facing = startFacing;
        }

        void Update()
        {
            // The InputReader reports zero while a panel is open, so the player stands still then.
            move = input != null ? input.Move : Vector2.zero;
            IsMoving = FacingRules.IsMoving(move);
            Facing = FacingRules.FromInput(move, Facing);
        }

        void FixedUpdate()
        {
            float speed = config != null ? config.moveSpeed : 0f;
            body.linearVelocity = IsMoving ? move * speed : Vector2.zero;
        }
    }
}
