
using TMPro;
using UnityEngine;

public class CameraController:MonoBehaviour 
{
    #region Fields

    private float currentXAngle;
    private float currentYAngle;
    [Range(0f, 90f)] public float UpperVerticalLimit = 35f;
    [Range(0f,90f)] public float LowerVerticalLimit = 35f;
    public float CameraSpeed = 50f;

    public bool SmoothCameraRotation;

    [Range(1f, 50f)] public float cameraSmoothFactor = 25f;
    private Transform _tr;
    private Camera camera;
    [SerializeField] private InputReader _inputReader;

    #endregion
    public Vector3 GetUpDirection() => _tr.up;
    public Vector3 GetFacingDirection() => _tr.forward;
    
    private void Awake()
    {
        _tr = transform;
        camera = GetComponentInChildren<Camera>();
        currentXAngle = _tr.localRotation.eulerAngles.x;
        currentYAngle = _tr.localRotation.eulerAngles.y;
    }

    private void Update()
    {
        RotateCamera(_inputReader.LookDirection.x,-_inputReader.LookDirection.y);
    }

    void RotateCamera(float x, float y)
    {
        if (SmoothCameraRotation)
        {
            x=Mathf.Lerp(0, x, Time.deltaTime * cameraSmoothFactor);
            y=Mathf.Lerp(0, y, Time.deltaTime * cameraSmoothFactor);
        }
        currentXAngle += y * CameraSpeed * Time.deltaTime;
        currentYAngle += x * CameraSpeed * Time.deltaTime;
        currentXAngle=Mathf.Clamp(currentXAngle,-UpperVerticalLimit,LowerVerticalLimit);
        _tr.localRotation = Quaternion.Euler(currentXAngle,currentYAngle,0);
    }

}
