using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    public GameObject player;
    public Vector3 offSet;
    public float rotationOffsetX;
    public float occlusionSmoothFactor = 0.05f;
    public float rotationSmoothFactor = 0.05f;

    //private Vector3[] _clipPoints;
    private int _layerMask;

    private Vector3 _positionalOffset;
    private Quaternion _rotationalOffset;

    private Vector3 _targetPosition; // position of camera if no collision / occlusion
    private Vector3[] _targetClipPoints;

    private Camera _cam;
    private Vector3 _camVel;
    private float _rotationVel;
    private bool _colliding;
    private float _adjustmentDistance;

    void Start()
    {
        //_clipPoints = new Vector3[5];
        _targetClipPoints = new Vector3[5];
        _layerMask = ~(1 << player.layer);
        _cam = Camera.main;
        _camVel = Vector3.zero;
        _colliding = false;
        _adjustmentDistance = 0.0f;
    }

    void GetClipPoints(Vector3 position, Quaternion rotation, ref Vector3[] clipArray)
    {
        float z = _cam.nearClipPlane;
        float x = Mathf.Tan(_cam.fieldOfView / Mathf.PI) * z;
        float y = x / _cam.aspect;

        clipArray[0] = (rotation * new Vector3(-x, y, z)) + position; // top left
        clipArray[1] = (rotation * new Vector3(x, y, z)) + position; // top right
        clipArray[2] = (rotation * new Vector3(-x, -y, z)) + position; // bottom left
        clipArray[3] = (rotation * new Vector3(x, -y, z)) + position; // bottom right 
        clipArray[4] = position; // camera centre 
    }

    bool IsOccluded(Vector3 position)
    {
        foreach (Vector3 clipPoint in _targetClipPoints)
        {
            if (Physics.Raycast(position, clipPoint - position, Vector3.Distance(position, clipPoint), _layerMask))
            {
                Debug.Log("occluded");
                return true;
            }
        }
        return false;
    }

    float GetAdjustmentDistance(Vector3 position)
    {
        float minDistance = float.MaxValue;

        foreach (Vector3 clipPoint in _targetClipPoints)
        {
            RaycastHit hit;
            if (Physics.Raycast(position, clipPoint - position, out hit, Vector3.Distance(position, clipPoint), _layerMask))
            {
                if (hit.distance < minDistance)
                    minDistance = hit.distance;
            }
        }
        if (minDistance == float.MaxValue)
            return 0;
        else
            return minDistance;
    }

    void MoveTowardsPlayer()
    {
        _targetPosition = player.transform.position + offSet;

        if (_colliding)
        {
            float adjustedDeltaY = offSet.y * _adjustmentDistance / offSet.z;
            Vector3 adjustedPosition = player.transform.position + player.transform.rotation * new Vector3(0.0f, adjustedDeltaY, _adjustmentDistance);
            transform.position = Vector3.SmoothDamp(transform.position, adjustedPosition, ref _camVel, occlusionSmoothFactor);
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _camVel, occlusionSmoothFactor);
        }
    }

    void LookAtPlayer()
    {
        //float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, player.transform.eulerAngles.y, ref _rotationVel, rotationSmoothFactor);
        //float rotationX = Mathf.SmoothDampAngle(transform.eulerAngles.x, rotationOffsetX * Mathf.PI / 180.0f, ref _rotationVel, rotationSmoothFactor);
        //transform.rotation = Quaternion.Euler(rotationOffsetX * Mathf.PI / 180.0f, rotationY, 0);
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 100 * Time.deltaTime);
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        MoveTowardsPlayer();
        LookAtPlayer();

        //GetClipPoints(transform.position, transform.rotation, ref _clipPoints);
        GetClipPoints(_targetPosition, transform.rotation, ref _targetClipPoints);

        _adjustmentDistance = -1 * GetAdjustmentDistance(player.transform.position);
        _colliding = IsOccluded(player.transform.position);
    }
}
