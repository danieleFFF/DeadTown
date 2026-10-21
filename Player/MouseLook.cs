using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour {
    public enum RotationAxes {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityHor = 5.0f;
    public float sensitivityVer = 5.0f;
    public float minimumVer = -45.0f;
    public float maximumVer = 45.0f;
    public float rotationX = 0;
    [Header("Mouse Acceleration")]
    [Tooltip("Mouse acceleration for more responsive aiming")]
    public bool useAcceleration = true;
    [Tooltip("How much faster movements are amplified")]
    [Range(1f, 2f)] public float accelerationPower = 1.3f;
    [Tooltip("Smoothing applied to mouse input (0 = none, higher = smoother)")]
    [Range(0f, 0.3f)] public float smoothing = 0.05f;
    private Vector2 smoothedInput;
    private Vector2 currentVelocity;
    
    void Start() {
        Rigidbody body = GetComponent<Rigidbody>();
        
        if(body != null ) body.freezeRotation = true;
    } 

    void Update() {
        //Invalid sensibility check
        if (float.IsNaN(sensitivityHor) || sensitivityHor <= 0) sensitivityHor = 6.0f;
        if (float.IsNaN(sensitivityVer) || sensitivityVer <= 0) sensitivityVer = 6.0f;
        
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        //Makes fast movements faster, slow movements stay precise
        if (useAcceleration) {
            mouseX = ApplyAcceleration(mouseX);
            mouseY = ApplyAcceleration(mouseY);
        }
        
        //Apply smoothing for less laggy movements
        if (smoothing > 0) {
            smoothedInput.x = Mathf.SmoothDamp(smoothedInput.x, mouseX, ref currentVelocity.x, smoothing);
            smoothedInput.y = Mathf.SmoothDamp(smoothedInput.y, mouseY, ref currentVelocity.y, smoothing);
            mouseX = smoothedInput.x;
            mouseY = smoothedInput.y;
        }
        
        //Other checks
        if (float.IsNaN(mouseX)) mouseX = 0;
        if (float.IsNaN(mouseY)) mouseY = 0;
        
        if (axes == RotationAxes.MouseX) { 
            transform.Rotate(0, mouseX * sensitivityHor, 0); 
        } 
        else if(axes == RotationAxes.MouseY) {
            rotationX -= mouseY * sensitivityVer;
            rotationX = Mathf.Clamp(rotationX, minimumVer, maximumVer);
            float rotationY = transform.localEulerAngles.y;
            transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
        } 
        else {
            rotationX -= mouseY * sensitivityVer;
            rotationX = Mathf.Clamp(rotationX, minimumVer, maximumVer);
            float delta = mouseX * sensitivityHor;
            float rotationY = transform.localEulerAngles.y + delta;
            transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
        }
    }
    
    //Applies acceleration curve to mouse input
    float ApplyAcceleration(float input) {
        if (Mathf.Abs(input) < 0.0001f) return 0;
        
        float absInput = Mathf.Abs(input);
        float sign = Mathf.Sign(input);
        
        //Applies power curve for acceleration
        float accelerated = Mathf.Pow(absInput, accelerationPower);
        
        //Other check
        if (float.IsNaN(accelerated)) return input;
        
        return accelerated * sign;
    }
}
