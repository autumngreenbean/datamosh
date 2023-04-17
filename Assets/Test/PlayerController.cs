using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerController : MonoBehaviour
{
    private float horizontal;
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float speed = 8f;       
    [SerializeField] [Tooltip("The maximum momentum value.")] private float maxMomentum = 10f;
    [SerializeField] [Tooltip("The time it takes to build up momentum.")]private float momentumTime = 2f;
    [SerializeField] [Tooltip("The amount to increment the speed by per second while moving to build momentum.")]private float momentumSpeedIncrement = 1f;
    private float momentum = 0f;
    private bool isMoving = false;
    private float moveTime = 0f;
    private float timeMoving = 0f;
   


    [SerializeField] private float jumpingPower = 16f;
    private bool isFacingRight = true;

    private bool 
    canDash = true;
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
    [SerializeField] private TMP_Text momentumText;
    [SerializeField] private TMP_Text dashCooldownText;
    [SerializeField] private TMP_Text ascendCooldownText;

    [SerializeField] private float blinkDistance = 10f;
    [SerializeField] private float fallSpeed = 50f;

    private bool isHovering = false;

    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        dashCooldownText = GameObject.Find("Canvas/dashCooldownText").GetComponent<TMP_Text>();
        ascendCooldownText = GameObject.Find("Canvas/ascendCooldownText").GetComponent<TMP_Text>();
        momentumText = GameObject.Find("Canvas/momentumText").GetComponent<TMP_Text>();

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

    // Added code for blink ability
    if (Input.GetMouseButton(0) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
    {
        StartCoroutine(Blink());
    }

    if (horizontal != 0)
    {
        if (!isMoving)
        {
            isMoving = true;
            timeMoving = 0f;
        }
        else
        {
            timeMoving += Time.deltaTime;
            if (timeMoving >= momentumTime)
            {
                speed = Mathf.Clamp(speed + momentumSpeedIncrement, 0f, maxMomentum);
            }
        }
    }
    else
    {
        isMoving = false;
        speed = baseSpeed;
    }

    // Apply horizontal movement
    Vector3 movement = new Vector3(horizontal * speed * Time.fixedDeltaTime, 0f, 0f);
    rb.MovePosition(transform.position + movement);

    // Apply gravity
    rb.AddForce(Physics.gravity * fallSpeed * rb.mass, ForceMode.Force);

    Flip();
    UpdateCooldowns();

    // Update UI elements
    dashCooldownText.text = "Dash Cooldown: " + dashTimer.ToString("F1");
    momentumText.text = "Momentum: " + speed.ToString("F1");
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

    private IEnumerator Blink()
{
    // Don't blink if already dashing or ascending
    if (isDashing || isAscending)
    {
        yield break;
    }

    isDashing = true;

    // Calculate blink direction
    Vector3 blinkDirection = Vector3.right * (Input.GetKey(KeyCode.D) ? 1f : -1f);

    // Move player forward by blink distance
    Vector3 startPosition = transform.position;
    Vector3 endPosition = transform.position + blinkDirection * blinkDistance;
    float elapsedTime = 0f;
    Debug.Log("Blink triggered!");
    while (elapsedTime < dashingTime)
    {
        rb.MovePosition(Vector3.Lerp(startPosition, endPosition, elapsedTime / dashingTime));
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    // Stop player momentum after blink
    rb.velocity = Vector3.zero;
    isDashing = false;

    // Add delay after blink
    yield return new WaitForSeconds(1.0f);

    // Start dash cooldown
    canDash = false;
    yield return new WaitForSeconds(dashingCooldown);
    canDash = true;
}



    private void ToggleHover()
{
    isHovering = !isHovering;
}
}

