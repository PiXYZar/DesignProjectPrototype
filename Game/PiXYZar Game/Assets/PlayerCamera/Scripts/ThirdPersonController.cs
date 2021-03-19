using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : PortalTraveller
{
    public class RigidbodyProperties
    {
        public float angularDrag;
        public Vector3 angularVelocity;
        public float drag;
        public Vector3 inertiaTensor;
        public Quaternion inertiaTensorRotation;
        //public double velocity;
    }

    public GameObject tower;

    public RigidbodyProperties rbProperties = new RigidbodyProperties();

    public float userInputDelay = 0.1f;

    public float walkingSpeed = 3;
    public float runningSpeed = 6;

    public float gravity = 0.75f;
    public float jumpingSpeed = 50f;

    public bool lockCursor = false;

    private Vector3 _towerCentre;

    private Rigidbody _rb;
    private CapsuleCollider _collider;

    private float _inputX, _inputY, _inputZ;

    private Vector3 _playerVel;
    private float _verticalVel;

    private int _layerMask;

    private bool _enteredPortal = false;
    private bool _exitedPortal = false;
    private bool _insidePortal = false;

    public bool EnteredPortal { get { return _enteredPortal; } set { _enteredPortal = value; } }
    public bool ExitedPortal { get { return _exitedPortal; } set { _exitedPortal = value; } }
    public bool InsidePortal { get { return _insidePortal; } set { _insidePortal = value; } }

    void Awake()
    {
        // check if asset has rigid body, if yes, store it 
        if (GetComponent<Rigidbody>())
            _rb = GetComponent<Rigidbody>();
        else
            Debug.LogError("Player asset requires a rigid body component.");

        // check if component has collider, if yes, store y bounds 
        if (GetComponentInChildren<CapsuleCollider>())
            _collider = GetComponentInChildren<CapsuleCollider>();
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

        _towerCentre = tower.GetComponent<Renderer>().bounds.center;
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

        // 
        Vector3 playerLocal = tower.transform.InverseTransformPoint(transform.position);
        //Debug.Log(playerRel2Tower);
        float hypotenuse = new Vector2(playerLocal.x, playerLocal.z).magnitude;
        float xDir = -playerLocal.z / hypotenuse;
        float zDir = playerLocal.x / hypotenuse;

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

        // if any of buttons WASD pressed, move, else don't move
        if (Mathf.Abs(_inputX) > userInputDelay || Mathf.Abs(_inputZ) > userInputDelay)
        {
            // create sideways and forward movement and combine them 
            Quaternion targetRotation = Quaternion.Euler(0.0f, Mathf.Abs(xVel) * 90.0f, 0.0f);
            Vector3 forward = new Vector3(xDir * zVel, 0.0f, zDir * zVel);
            Vector3 sideways = targetRotation * new Vector3(xDir * xVel, 0.0f, zDir * xVel);
            _playerVel = (forward + sideways).normalized * speed;
        } 
        else
            _playerVel = Vector3.zero;
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


    void onTriggerEnter(Collider collider)
    {
        if (collider.tag.Equals("Portal"))
        {
            _enteredPortal = true;
        }
    }

    void onTriggerExit(Collider collider)
    {
        if (collider.tag.Equals("Portal"))
        {
            _insidePortal = false;
            _exitedPortal = true;
        }
    }

    void onTriggerStay(Collider collider)
    {
        if (collider.tag.Equals("Portal"))
        {
            _insidePortal = true;
        }
    }

    void InterruptRigidbody()
    {
        rbProperties.angularDrag = _rb.angularDrag;
        rbProperties.angularVelocity = _rb.angularVelocity;
        rbProperties.drag = _rb.drag;
        rbProperties.inertiaTensor = _rb.inertiaTensor;
        rbProperties.inertiaTensorRotation = _rb.inertiaTensorRotation;
        //rbProperties.velocity = _rb.velocity;

        _rb.angularDrag = _rb.drag = 0.0f;
        _rb.angularVelocity = _rb.inertiaTensor = Vector3.zero;
        _rb.inertiaTensorRotation = Quaternion.identity;
        // _rb.velocity;
    }

    void ReturnRigidbodyProperties()
    {
        _rb.angularDrag = rbProperties.angularDrag;
        _rb.angularVelocity = rbProperties.angularVelocity;
        _rb.drag = rbProperties.drag;
        _rb.inertiaTensor = rbProperties.inertiaTensor;
        _rb.inertiaTensorRotation = rbProperties.inertiaTensorRotation;
        //_rb.velocity = rbProperties.velocity;
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
    {
        // first store values of rigid body then set them to zero 
        InterruptRigidbody();

        // perform teleportation
        _rb.position = pos;
        _rb.rotation = rot;
        _rb.velocity = toPortal.TransformVector(fromPortal.InverseTransformVector(_rb.velocity));
        _verticalVel = _rb.velocity.y;
        Physics.SyncTransforms();

        // return rigid body properties 
        ReturnRigidbodyProperties();
    }
}
