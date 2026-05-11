using TMPro;
using UnityEngine;

public class TurnTowardsController:MonoBehaviour
{
    [SerializeField] PlayerController _playerController;
    public float turnSpeed = 50f;
    Transform _tr;
    private float currentYRotation;
    private const float fallOffAngle = 90f;

    void Start()
    {
        _tr = transform;
        currentYRotation = _tr.localEulerAngles.y;
    }

    void LateUpdate()
    {
        Vector3 velocity = Vector3.ProjectOnPlane(_playerController.GetMovementVelocity(), _tr.parent.up);
        if(velocity.magnitude<0.001f) return;
        float angledifference=GetAngle(_tr.forward,velocity.normalized,_tr.parent.up);
        float step=Mathf.Sign(angledifference)*Mathf.InverseLerp(0f,fallOffAngle,Mathf.Abs(angledifference))*turnSpeed*Time.deltaTime;
        currentYRotation+=Mathf.Abs(step)>Mathf.Abs(angledifference)?angledifference:step;
        _tr.localRotation=Quaternion.Euler(0f,currentYRotation,0f); 
        
    }
    public static float GetAngle(Vector3 vector1, Vector3 vector2, Vector3 planeNormal) {
        var angle = Vector3.Angle(vector1, vector2);
        var sign = Mathf.Sign(Vector3.Dot(planeNormal, Vector3.Cross(vector1, vector2)));
        return angle * sign;
    }

}