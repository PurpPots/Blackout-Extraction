
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 12f;
    public float gravity = -9.81f * 5;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;

    bool isGrounded;
    bool isMoving;

    // Dash Variables
    public float dashSpeed = 30f;
    public float dashDuration = 0.05f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float lastDashTime = -100f;

    private Vector3 lastPosition = new Vector3(0f, 0f, 0f);

    //  public TextMeshProUGUI dashCooldownText; // Reference to UI Text
    public Slider dashStaminaSlider;

    public Transform cameraTransform;

    private Vector3 knockbackVelocity = Vector3.zero; 
    private float knockbackDecay = 5f;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (dashStaminaSlider != null)
        {
            dashStaminaSlider.maxValue = dashCooldown;
            dashStaminaSlider.value = dashCooldown;
        }
    }

    // Update is called once per frame
    void Update()
    {

        UpdateDashCooldownUI();

        // if we're standing on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask) || CheckIfStandingOnRigidbody();

        // reset the default the velocity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //getting input for moving left right + front back
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //creating the moving vector
        Vector3 move = transform.right * x + transform.forward * z; //right - red, forward - blue

        move.y = 0; // Prevent movement from affecting Y-axis

        // Dash logic
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown && move != Vector3.zero)
        {
            //print("Dashing!");
            StartCoroutine(Dash(move));
        }
        if (move.magnitude > 1f) move.Normalize();

        // Move player (if not dashing)
        if (!isDashing)
        {
            controller.Move(move * speed * Time.deltaTime);
        }

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
       
 
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        if (knockbackVelocity.magnitude > 0.1f)
        {
            controller.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);
    }

    bool CheckIfStandingOnRigidbody()
    {
        Collider[] colliders = Physics.OverlapSphere(groundCheck.position, groundDistance, groundMask);

        foreach (Collider col in colliders)
        {
            // Check if the object has BOTH a Rigidbody AND a Collider
            if (col.attachedRigidbody != null && col.GetComponent<Collider>() != null)
            {
                return true; // Found a valid object
            }
        }

        return false; // No valid objects found
    }

    IEnumerator Dash(Vector3 moveDirection)
    {
        isDashing = true;
        lastDashTime = Time.time;

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            controller.Move(moveDirection.normalized * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
    }


    void UpdateDashCooldownUI()
    {
        float cooldownRemaining = Mathf.Clamp((lastDashTime + dashCooldown) - Time.time, 0, dashCooldown);


        // Update Slider UI
        if (dashStaminaSlider != null)
        {
            dashStaminaSlider.value = cooldownRemaining;
        }
    }

    public void ApplyKnockback(Vector3 direction, float force, float verticalBoost = 5f)
    {
        direction.y = 0f;
        direction.Normalize();

        knockbackVelocity = direction * force;
        knockbackVelocity.y = verticalBoost;

        Debug.Log("Knockback applied: " + knockbackVelocity);
    }
}