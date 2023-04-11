using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerController : MonoBehaviour
{
   private float horizontal;
    private float speed = 8f;
    [SerializeField] private float jumpingPower = 16f;
    private bool isFacingRight = true;

    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.01f;
    [SerializeField] private float dashingCooldown = 2f;
    private float dashTimer = 0f;

    private bool isAscending;
    [SerializeField] private float ascendingPower = 340f;
    [SerializeField] private float ascendingCooldown = 10f;
    private float ascendTimer = 0f;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private ParticleSystem particleTrail;

    [Header("UI")]
    [SerializeField] private TMP_Text dashCooldownText;
    [SerializeField] private TMP_Text ascendCooldownText;

    [SerializeField] private float fallSpeed = 10f;

    private bool isHovering = false;


    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        dashCooldownText = GameObject.Find("Canvas/dashCooldownText").GetComponent<TMP_Text>();
    ascendCooldownText = GameObject.Find("Canvas/ascendCooldownText").GetComponent<TMP_Text>();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            isHovering = !isHovering;
        }

        if (isDashing)
        {
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpingPower, rb.velocity.z);
        }

        if (Input.GetKey(KeyCode.LeftShift) && canDash && dashTimer <= 0)
        {
            StartCoroutine(Dash());
            dashTimer = dashingCooldown;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && ascendTimer <= 0)
        {
            StartCoroutine(Ascend());
            ascendTimer = ascendingCooldown;
        }

        Flip();
        UpdateCooldowns();
    }
private void UpdateCooldowns()
{
    dashCooldownText.SetText("Dash Cooldown: " + Mathf.Max(0f, dashTimer).ToString("0.00"));
    ascendCooldownText.SetText("Ascend Cooldown: " + Mathf.Max(0f, ascendTimer).ToString("0.00"));
}

    private void FixedUpdate()
{
    if (!isHovering)
    {
        rb.AddForce(Physics.gravity * fallSpeed * rb.mass, ForceMode.Force);
    }

    
    if (isDashing)
    {
        return;
    }

    if (isAscending)
    {
        // calculate the force required to ascend based on current velocity and ascending power
        Vector3 velocity = rb.velocity;
        velocity.y = 0f; // set vertical component of velocity to zero to only apply ascending force upwards
        Vector3 force = Vector3.up * (ascendingPower - velocity.magnitude * rb.mass);
        rb.AddForce(force, ForceMode.Force);
    }

    // Apply horizontal movement
    Vector3 movement = new Vector3(horizontal * speed * Time.fixedDeltaTime, 0f, 0f);
    rb.MovePosition(transform.position + movement);

    // Apply gravity
rb.AddForce(Physics.gravity * fallSpeed * rb.mass, ForceMode.Force);


    dashTimer -= Time.fixedDeltaTime;
    ascendTimer -= Time.fixedDeltaTime;
}


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canDash = true;
        }
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rb.velocity = new Vector3(transform.localScale.x * dashingPower, 0f, 0f);
        particleTrail.Play();

        yield return new WaitForSeconds(dashingTime);

        particleTrail.Stop();
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);

        canDash = true;
        particleTrail.Play();

        dashTimer = 0f;
    }

    private IEnumerator Ascend()
    {
        isAscending = true;

        yield return new WaitForSeconds(0.1f);

        isAscending = false;
        ascendTimer = 0f;
    }
    private void ToggleHover()
{
    isHovering = !isHovering;
}
}

