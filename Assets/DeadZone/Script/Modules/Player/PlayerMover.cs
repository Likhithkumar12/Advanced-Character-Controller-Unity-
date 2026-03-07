using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMover:MonoBehaviour
{
    #region Fields
    [Header("Collider Settings")]
    [Range(0f,1f)] [SerializeField] private float _stepHeightRatio = 0.5f;

    [SerializeField] float colliderHeight = 2f;
    [SerializeField] float colliderThikness = 1f;
    [SerializeField] Vector3 colliderOffset = Vector3.zero;
    Rigidbody _rb;
    CapsuleCollider _collider;
    private Transform _tr;
    RayCastSensor _rayCastSensor;
    bool isGrounded;
    private float basesensorRange;
    Vector3 currentGroundAdjustmentVelocity;
    private int currentLayer;
    
    [Header("Sensor Settings")]
    [SerializeField] bool inGameDebug;

    private bool isUsingExtendedSensorRnage = false;
    
    #endregion

    private void Awake()
    {
        Setup();
        ReCalculateColliderDimensions();

    }

    private void OnValidate()
    {
        if (gameObject.activeInHierarchy)
        {
            ReCalculateColliderDimensions();
        }
    }
    void LateUpdate()
    {
        if (inGameDebug)
        {
            _rayCastSensor.DrawDebug();
        }
    }
    

    void Setup()
    {
        _tr = transform;
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _rb.freezeRotation = true;
        _rb.useGravity = false;
    }
    void ReCalculateSensorLayerMask()
    {
        int objectLayer = gameObject.layer;
        int layerMask = Physics.AllLayers;
        for(int i=0; i<32; i++)
        {
            if (Physics.GetIgnoreLayerCollision(objectLayer, i))
            {
                layerMask &= ~(1 << i);
            }
        }
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        layerMask &= ~(1 << ignoreLayer);
        _rayCastSensor.layerMask = layerMask;
        currentLayer= objectLayer;
    }
    void RecalibrateSensor()
    {
        _rayCastSensor??=new RayCastSensor(_tr);
        _rayCastSensor.SetOrigin(_collider.bounds.center);
        _rayCastSensor.SetCastDirection(RayCastSensor.CastDirection.down);
        ReCalculateSensorLayerMask();

        const float safetyDistanceFactor = 0.001f; // small value used to prevent clipping issues
        float length = colliderHeight * (1f - _stepHeightRatio) * 0.5f + colliderHeight * _stepHeightRatio;
        basesensorRange = length * (1f + safetyDistanceFactor) * _tr.localScale.x;
        _rayCastSensor.castLength = length * _tr.localScale.x;
        
    }

    void ReCalculateColliderDimensions()
    {
        if (_collider == null)
        {
            Setup();
        }
        _collider.height=colliderHeight*(1f-_stepHeightRatio);
        _collider.radius = colliderThikness / 2f;
        _collider.center = colliderOffset* colliderHeight + new Vector3(0f,_stepHeightRatio * _collider.height / 2f,0f);
        if(_collider.height /2f < _collider.radius)
        {
            _collider.radius = _collider.height / 2f;
        }
        RecalibrateSensor();
    }

    public void CheckForGround()
    {
        if(currentLayer!= gameObject.layer)
        {
            ReCalculateSensorLayerMask();
        }

        currentGroundAdjustmentVelocity = Vector3.zero;
        _rayCastSensor.castLength = isUsingExtendedSensorRnage
            ? basesensorRange + colliderHeight * _tr.localScale.x * _stepHeightRatio:basesensorRange;
        _rayCastSensor.Cast();
        isGrounded = _rayCastSensor.HasDetectedHit();
        if (!isGrounded) return;
        float distance = _rayCastSensor.GetHitDistance();
        
        float upperlimit=colliderHeight*_tr.localScale.x*(1f-_stepHeightRatio)*0.5f;
        float middle=upperlimit+colliderHeight*_tr.localScale.x*_stepHeightRatio;
        float distancetoGo = middle - distance;
        currentGroundAdjustmentVelocity=_tr.up * (distancetoGo/Time.fixedDeltaTime);
    }
    
    public bool IsGrounded()=>isGrounded;
    public Vector3 GroundNormal() => _rayCastSensor.GetNormal();
    public void SetVelocity(Vector3 velocity)=>_rb.linearVelocity=velocity+ currentGroundAdjustmentVelocity;
    public void SetExtendSensorRange(bool isExtended) => isUsingExtendedSensorRnage = isExtended;
    
}
