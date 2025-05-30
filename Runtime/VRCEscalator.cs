
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace Asriels.Escalators.Base
{
    // A nicety for the inspector :)
    public enum Direction : int
    {
        up = 1,
        down = 2
    }

    // Yet another nicety :)
    public enum Axis : int
    {
        x = 0,
        y = 1,
        z = 2
    }

    [RequireComponent(typeof(Collider))]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRCEscalator : UdonSharpBehaviour
    {
        [Tooltip("The entrance at the bottom of the escalator.")]
        [SerializeField] Transform lowerLevelPosition;
        [Tooltip("The entrance at the top of the escalator.")]
        [SerializeField] Transform upperLevelPosition;

        [Header("Configuration")]
        [SerializeField, Range(0.1f, 5f)] float speed = 1f;
        [Tooltip("The direction of the escalator")]
        [SerializeField] Direction direction = Direction.up;
        [Tooltip("The local axis on which the platform aligns with the player, perpendicular to the direction of travel")]
        [SerializeField] Axis axis = Axis.x;
        [Tooltip("Whether to use Update() or FixedUpdate() to handle movement.")]
        [SerializeField] bool useFixedUpdate = true;
        [Tooltip("Whether or not to immobilize the player when standing still to prevent IK walking whilst moving.")]
        [SerializeField] bool immobilizeOnStandStill = true;

        // Determines whether player is currently riding the escalator
        bool currentlyInUse = false;

        // Whether or not the player is currently immobilized
        bool playerIsImmobile = false;

        // Stores the input values
        Vector2 inputMovement = Vector2.zero;

        // Cached reference to local player
        VRCPlayerApi localPlayer;

        // Target to move player towards
        Transform targetLevel
        {
            get
            {
                switch (direction)
                {
                    case Direction.up:
                        return upperLevelPosition;
                    case Direction.down:
                        return lowerLevelPosition;
                    default:
                        return upperLevelPosition;
                }
            }
        }


        // Aligns entry position to player on specified axis
        void AlignPosition()
        {
            var startingLevel = (direction == Direction.up) ? lowerLevelPosition : upperLevelPosition;

            startingLevel.position = localPlayer.GetPosition();

            Vector3 localStartingPosition = startingLevel.localPosition;

            Vector3 targetPosition = targetLevel.localPosition;
            switch (axis)
            {
                case Axis.x:
                    targetLevel.localPosition = new Vector3(localStartingPosition.x, targetPosition.y, targetPosition.z);
                    break;
                case Axis.y:
                    targetLevel.localPosition = new Vector3(targetPosition.x, localStartingPosition.y, targetPosition.z);
                    break;
                case Axis.z:
                    targetLevel.localPosition = new Vector3(targetPosition.x, targetPosition.y, localStartingPosition.z);
                    break;
            }


        }

        void OnEnable()
        {
            localPlayer = Networking.LocalPlayer;

            // Destroys script if variables are unset
            if (!lowerLevelPosition || !upperLevelPosition)
                Destroy(this);
        }

        // Used for external triggers
        public void OnEnter()
        {
            OnPlayerTriggerEnter(localPlayer);
        }

        // Used for triggers from self
        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            base.OnPlayerTriggerEnter(player);

            // Prevents trigger from non-local players
            if (!player.isLocal)
                return;

            AlignPosition();
            currentlyInUse = true;
        }

        // Used for external triggers & end of ride
        public void OnExit()
        {
            OnPlayerTriggerExit(localPlayer);
        }

        // Used for triggers from self
        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            base.OnPlayerTriggerExit(player);

            // Prevents trigger from non-local players
            if (!player.isLocal)
                return;

            currentlyInUse = false;

            if (!playerIsImmobile)
                return;

            player.SetVelocity(Vector3.zero);
            playerIsImmobile = false;
            player.Immobilize(false);
        }

        // Store movement in a 2D Vector for fixing player IK
        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            base.InputMoveHorizontal(value, args);
            inputMovement = new Vector2((value * 0.1f), inputMovement.y);
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            base.InputMoveVertical(value, args);
            inputMovement = new Vector2(inputMovement.x, value);
        }

        // Update cycle when using FixedUpdate()
        void FixedUpdate()
        {
            // Exit early if not in use or if set to use Update() or if player is not grounded
            if (!currentlyInUse || !useFixedUpdate || !localPlayer.IsPlayerGrounded())
                return;

            // Get positional values
            var playerVelocity = localPlayer.GetVelocity();
            var origin = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
            var offset = localPlayer.GetPosition() - origin.position;
            var targetPosition = Vector3.MoveTowards(localPlayer.GetPosition(), targetLevel.position, Time.fixedDeltaTime * speed) - offset;

            // Keep exit in alignment with player
            AlignPosition();
            // Prevent lazy legs
            playerIsImmobile = (immobilizeOnStandStill
            && inputMovement == Vector2.zero
            && (Mathf.Abs(playerVelocity.x) + Mathf.Abs(playerVelocity.z)) < 0.01f);
            localPlayer.Immobilize(playerIsImmobile);

#if UNITY_EDITOR
            // None of that is required in client sim.
            localPlayer.TeleportTo(Vector3.MoveTowards(localPlayer.GetPosition(), targetLevel.position, Time.fixedDeltaTime * speed), localPlayer.GetRotation());
#else
            // Lerp movement for improved network performance; treats teleport like standard movement
            localPlayer.TeleportTo(targetPosition, origin.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint, true);
#endif
        }


        // Update cycle when using Update()
        void Update()
        {
            // Exit early if not in use or set to use FixedUpdate() or if player is not grounded
            if (!currentlyInUse || useFixedUpdate || !localPlayer.IsPlayerGrounded())
                return;

            // Get positional values
            var playerVelocity = localPlayer.GetVelocity();
            var origin = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
            var offset = localPlayer.GetPosition() - origin.position;
            var targetPosition = Vector3.MoveTowards(localPlayer.GetPosition(), targetLevel.position, Time.fixedDeltaTime * speed) - offset;

            // Keep exit in alignment with player
            AlignPosition();
            // Prevent lazy legs
            playerIsImmobile = (immobilizeOnStandStill
            && inputMovement == Vector2.zero
            && (Mathf.Abs(playerVelocity.x) + Mathf.Abs(playerVelocity.z)) < 0.01f);
            localPlayer.Immobilize(playerIsImmobile);

#if UNITY_EDITOR
            // None of that is required in client sim.
            localPlayer.TeleportTo(Vector3.MoveTowards(localPlayer.GetPosition(), targetLevel.position, Time.fixedDeltaTime * speed), localPlayer.GetRotation());
#else
            // Lerp movement for improved network performance; treats teleport like standard movement
            localPlayer.TeleportTo(targetPosition, origin.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint, true);
#endif
        }

        // Gizmos!
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmos()
        {

            // Return if variables are unset
            if (!lowerLevelPosition || !upperLevelPosition)
                return;

            // Convenience variables
            var upperPosition = upperLevelPosition.position;
            var lowerPosition = lowerLevelPosition.position;
            var cubeSize = new Vector3(0.2f, 0.2f, 0.2f);
            var stepSize = new Vector3(0.5f, 0.075f, 0.5f);
            var sphereSize = 0.1f;

            // Set the line color
            Gizmos.color = new Color(0f, 0.5f, 1f, 1f); // Blue

            // Draw a line between the two points of the escalator
            Gizmos.DrawLine(lowerPosition, upperPosition);


            // Set the endpoint color
            Gizmos.color = new Color(1f, 1f, 0f, 1f); // Yellow

            switch (direction)
            {
                case Direction.up:
                    // Draw a cube at the start
                    Gizmos.DrawCube(lowerPosition, cubeSize);
                    // Draw a sphere at the end
                    Gizmos.DrawSphere(upperPosition, sphereSize);
                    // UnityEditor.Handles.ArrowHandleCap are a [ BEAUTY ] to use. Ima just draw a sphere...
                    break;
                case Direction.down:
                    // Draw a cube at the start
                    Gizmos.DrawCube(upperPosition, cubeSize);
                    // Draw a sphere at the end
                    Gizmos.DrawSphere(lowerPosition, sphereSize);
                    break;
            }

            // Calculate and display the midpoint
            Vector3 midpoint = (lowerPosition + upperPosition) / 2f;

            // Highlight this object
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Translucent Red
            Gizmos.DrawSphere(transform.position, sphereSize * 2f);

            // Display the distance
            float distance = Vector3.Distance(lowerPosition, upperPosition);
            UnityEditor.Handles.Label(midpoint, $"Distance: {distance:F2}");
        }
#endif
    }
}