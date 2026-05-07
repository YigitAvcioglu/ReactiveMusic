using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;

public class VRFixer : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool invertMoveY = true;
    public float moveSpeed = 2.0f;

    [Header("Look Settings")]
    public float verticalLookSensitivity = 60f;
    public bool invertLookY = false;

    private XROrigin xrOrigin;
    private ContinuousMoveProvider continuousMoveProvider;
    private ContinuousTurnProvider continuousTurnProvider;
    private CharacterController characterController;
    private Transform cameraOffset;
    

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        characterController = GetComponent<CharacterController>();
        continuousMoveProvider = FindFirstObjectByType<ContinuousMoveProvider>();
        continuousTurnProvider = FindFirstObjectByType<ContinuousTurnProvider>();
        
        if (xrOrigin != null)
        {
            cameraOffset = xrOrigin.CameraFloorOffsetObject.transform;
            xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            // Also ensure the offset itself has zero local rotation so the world is level
            cameraOffset.localRotation = Quaternion.identity;
        }

        if (continuousMoveProvider != null) continuousMoveProvider.enabled = false;
        // Don't disable TeleportationActivator here if we want laser interaction, though maybe they are using it differently.
        // We will leave the laser logic to the XR components.
    }

    void Update()
    {
        // Handle Movement
        if (continuousMoveProvider != null && characterController != null && xrOrigin != null)
        {
            Vector2 mInput = continuousMoveProvider.leftHandMoveInput.ReadValue();
            if (mInput.sqrMagnitude > 0.01f)
            {
                if (invertMoveY) mInput.y = -mInput.y;
                Vector3 mDir = xrOrigin.Camera.transform.TransformDirection(new Vector3(mInput.x, 0, mInput.y));
                mDir.y = 0;
                mDir.Normalize();
                characterController.Move(mDir * moveSpeed * Time.deltaTime);
            }
            if (!characterController.isGrounded) characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
        
        // Remove vertical look manual rotation as it breaks XR tracking space (rotates the whole room/floor).
        // Horizontal turn is handled automatically by ContinuousTurnProvider or SnapTurnProvider.
    }
}
