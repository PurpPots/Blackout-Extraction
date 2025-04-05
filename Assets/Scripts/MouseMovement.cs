using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float mouseSensitivity = 300f;
    float xRotation = 0f;
    float yRotatioin = 0f;

    public float topClamp = -40f;
    public float bottomClamp = 40f;

    public Transform playerBody;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //locking cursor to the middle of the screen and making it invisible
    }

    void Update()
    {
        //getting mouse inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;


        // Rotate player left/right (Y-axis rotation)
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down (X-axis rotation)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}