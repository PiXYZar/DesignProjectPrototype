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
        Vector2 towerCentreToPlayer = new Vector2(player.transform.position.x - tower.GetComponent<Renderer>().bounds.center.x,
           player.transform.position.z - tower.GetComponent<Renderer>().bounds.center.z);

        // distance from tower centre to target camera position (same level as player)
        float dist = towerCentreToPlayer.magnitude + distanceFromPlayer;

        // vertical offset 
        float height = dist * Mathf.Tan(rotationalOffset * Mathf.PI / 180.0f);

        // position of camera 
        Vector2 cameraXZ = towerCentreToPlayer.normalized * dist;
        Vector3 target = new Vector3(cameraXZ.x, player.transform.position.y + height, cameraXZ.y);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref _camVel, smooth);

        // rotate camera 
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 100 * Time.deltaTime);
    }
}
