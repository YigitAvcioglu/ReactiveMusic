using UnityEngine;

public class ReactiveRotation : MonoBehaviour
{
    public AudioSpectrumData data;

    [Header("Continuous Rotation")]
    public Vector3 baseRotationSpeed = new Vector3(0, 10f, 0);
    public Vector3 maxAddedSpeed = new Vector3(0, 100f, 0);
    public float smoothness = 8f;

    [Header("Snap Rotation on Beat")]
    public bool useSnap = true;
    public float beatThreshold = 0.8f;
    public Vector3 snapAngle = new Vector3(0, 45f, 0);
    
    private float currentSpeedMultiplier;
    private bool wasBeat;
    private Quaternion targetSnapRotation;

    void Start()
    {
        targetSnapRotation = transform.localRotation;
    }

    void Update()
    {
        if (data == null) return;

        // Smooth speed multiplier
        currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, data.amplitude, Time.deltaTime * smoothness);
        
        // Continuous rotation
        Vector3 currentSpeed = baseRotationSpeed + (maxAddedSpeed * currentSpeedMultiplier);
        transform.Rotate(currentSpeed * Time.deltaTime, Space.Self);

        // Snap logic
        if (useSnap)
        {
            if (data.bass > beatThreshold && !wasBeat)
            {
                wasBeat = true;
                targetSnapRotation *= Quaternion.Euler(snapAngle);
            }
            else if (data.bass < beatThreshold - 0.2f)
            {
                wasBeat = false;
            }

            // Smoothly apply snap rotation locally
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetSnapRotation, Time.deltaTime * smoothness);
        }
    }
}
