using UnityEngine;

    public class RayCastSensor
    {
        public float castLength = 1f;
        public LayerMask layerMask = 255;

        private Vector3 _origin = Vector3.zero;
        private Transform _tr;
        
        public enum CastDirection { forward, backward, right, left, up, down }
        public enum CastType { ray, sphere, box }

        private CastDirection _castDirection;
        private RaycastHit _hitInfo;

        public RayCastSensor(Transform playerTransform)
        {
            _tr=playerTransform;
        }

        public void Cast()
        {
            Vector3 origin=_tr.TransformPoint(_origin);
            Vector3 worldDirection = GetCastDirection();
            Physics.Raycast(origin,worldDirection,out _hitInfo,castLength,layerMask,QueryTriggerInteraction.Ignore);
        }
        public bool HasDetectedHit() => _hitInfo.collider != null;
        public float GetHitDistance() => _hitInfo.distance;
        public Vector3 GetNormal() => _hitInfo.normal;
        public Vector3 GetHitPoint() => _hitInfo.point;
        public Collider GetHitCollider() => _hitInfo.collider;
        public Transform GetHitTransform() => _hitInfo.transform;
        public void SetCastDirection(CastDirection castDirection)=> _castDirection = castDirection;
        public void SetOrigin(Vector3 pos)=> _origin = _tr.InverseTransformPoint(pos);

        private Vector3 GetCastDirection()
        {
            return _castDirection switch
            {
                CastDirection.forward => _tr.forward,
                CastDirection.backward => -_tr.forward,
                CastDirection.right => _tr.right,
                CastDirection.left => -_tr.right,
                CastDirection.up => _tr.up,
                CastDirection.down => -_tr.up,
                _ => Vector3.one
            };
        }
        public void DrawDebug() {
            if (!HasDetectedHit()) return;

            Debug.DrawRay(_hitInfo.point, _hitInfo.normal, Color.red, Time.deltaTime);
            float markerSize = 0.2f;
            Debug.DrawLine(_hitInfo.point + Vector3.up * markerSize, _hitInfo.point - Vector3.up * markerSize, Color.green, Time.deltaTime);
            Debug.DrawLine(_hitInfo.point + Vector3.right * markerSize, _hitInfo.point - Vector3.right * markerSize, Color.green, Time.deltaTime);
            Debug.DrawLine(_hitInfo.point + Vector3.forward * markerSize, _hitInfo.point - Vector3.forward * markerSize, Color.green, Time.deltaTime);
        }
        
    }
