using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalCameraController : MonoBehaviour
{
    public float distanceFromPlayer;
    public float rotationalOffset;
    public GameObject player;
    public GameObject tower;
    public float smooth = 0.05f;
    
    private Vector3 _camVel;
    
    void Start()
    {
        
    }
    
    void FixedUpdate()
    {
        // vector from center of tower to player
        Vector3 playerLocal = tower.transform.InverseTransformPoint(player.transform.position);

        // horizontal distance from tower centre to target camera position 
        float dist = new Vector2(playerLocal.x, playerLocal.z).magnitude + distanceFromPlayer;

        // vertical offset 
        float height = dist * Mathf.Tan(rotationalOffset * Mathf.PI / 180.0f);

        // position of camera 
        Vector3 localCamera = playerLocal.normalized * dist;
        Vector3 target = tower.transform.position + tower.transform.TransformVector(localCamera);
        target.y = player.transform.position.y + height;
        transform.position = Vector3.SmoothDamp(transform.position, target, ref _camVel, smooth);

        // rotate camera 
        Quaternion targetRotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 100 * Time.deltaTime);
    }
}
