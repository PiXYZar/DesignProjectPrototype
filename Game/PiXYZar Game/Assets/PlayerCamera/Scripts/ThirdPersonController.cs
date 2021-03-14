﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public float userInputDelay = 0.1f;

    public float walkingSpeed = 3;
    public float runningSpeed = 6;

    public float gravity = 0.75f;
    public float jumpingSpeed = 50f;

    public bool lockCursor = false;

    private Rigidbody _rb;
    private CapsuleCollider _collider;

    private float _inputX, _inputY, _inputZ;

    private Vector3 _playerVel;
    private float _verticalVel;

    private int _layerMask;

    void Awake()
    {
        // check if asset has rigid body, if yes, store it 
        if (GetComponent<Rigidbody>())
            _rb = GetComponent<Rigidbody>();
        else
            Debug.LogError("Player asset requires a rigid body component.");

        // check if component has collider, if yes, store y bounds 
        if (GetComponent<CapsuleCollider>())
            _collider = GetComponent<CapsuleCollider>();
        else
            Debug.LogError("Player asset requires a capsule collider component.");
    }

    void Start()
    {
        // lock the cursor 
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // initialize global velocities and inputs 
        _playerVel = Vector3.zero;
        _verticalVel = _playerVel.y;
        _inputX = _inputZ = _inputY = 0;

        // ignore player, detect anything else 
        _layerMask = ~(1 << LayerMask.NameToLayer("Player"));
    }

    bool IsGrounded()
    {
        // distance from center of collider to each of the centres of the spheres that make up the capsule 
        float distToSpheres = _collider.height / 2 - _collider.radius;

        // sphere centres in world coordinates 
        Vector3 centre1 = transform.position + _collider.center + Vector3.up * distToSpheres;
        Vector3 centre2 = transform.position + _collider.center + Vector3.up * distToSpheres;

        // capsule collider excluding its own collider and with smaller radius to avoid hitting walls 
        RaycastHit[] hits = Physics.CapsuleCastAll(centre1, centre2, _collider.radius * 0.95f, Vector3.down, 0.85f, _layerMask);

        // if there is a hit, it is grounded 
        if (hits.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void Translate()
    {
        // check whether player is running or walking 
        float speed = (Input.GetKey(KeyCode.LeftShift)) ? runningSpeed : walkingSpeed;

        float xVel, zVel;
        if (Mathf.Abs(_inputX) > userInputDelay)
        {
            xVel = _inputX;
        }
        else
        {
            xVel = 0.0f;
        }

        if (Mathf.Abs(_inputZ) > userInputDelay)
        {
            zVel = _inputZ;
        }
        else
        {
            zVel = 0.0f;
        }

        // set velocity, normalizing it for diagonal movement 
        _playerVel = new Vector3(xVel, 0.0f, zVel).normalized * speed;
    }

    void Jump()
    {
        // collider edge needs to lower than the origin of the player asset 
        // don't put gravity, modify it from script 
        // freeze rotation x y z so character doesn't stumble on its own collider

        // player is grounded and jump button pressed -> jump
        if (_inputY > 0 && IsGrounded())
        {
            _playerVel.y = _verticalVel = jumpingSpeed;
        }
        // player is grounded and jump button not pressed -> don't jump
        else if (_inputY == 0 && IsGrounded())
        {
            _playerVel.y = _verticalVel = 0.0f;
        }
        // player is in the air -> decrease vertical velocity
        else
        {
            _verticalVel -= gravity;
            _playerVel.y = _verticalVel;
        }
    }

    void Update()
    {
        _inputX = Input.GetAxis("Horizontal"); // left and right arrow keys or A/D
        _inputY = Input.GetAxisRaw("Jump"); // no need for interpolation, either -1, 0 or 1
        _inputZ = Input.GetAxis("Vertical"); // up and down arrow keys or W/S
    }

    void FixedUpdate()
    {
        Translate();
        Jump();

        _rb.velocity = transform.TransformDirection(_playerVel);
    }
}