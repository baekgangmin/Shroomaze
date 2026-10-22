using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 12f;
    public float gravity = -9.81f;

    public bool movementEnabled = true;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    // Used to drop the player back at the hub without CharacterController fighting the move —
    // it must be disabled while we set the transform directly, or it silently resists the teleport.
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        velocity = Vector3.zero;
        controller.enabled = true;
    }

    // Drives the character toward a world point through normal CharacterController.Move calls
    // (so it collides with walls like a real player would) rather than teleporting. Returns true
    // once close enough to the target. Used by the automated demo recording.
    public bool StepTowards(Vector3 targetPosition, float speed, float deltaTime)
    {
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;
        if (dist < 0.15f)
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            return true;
        }

        Vector3 dir = toTarget / dist;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), rotateSpeed * deltaTime);

        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * deltaTime;

        controller.Move((dir * speed + new Vector3(0f, velocity.y, 0f)) * deltaTime);
        if (animator != null) animator.SetFloat("Speed", 1f);
        return false;
    }

    public void SetIdle()
    {
        if (animator != null) animator.SetFloat("Speed", 0f);
    }

    void Update()
    {
        if (!movementEnabled)
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        Vector3 move = inputDir * moveSpeed;

        if (inputDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        Vector3 motion = move * Time.deltaTime + velocity * Time.deltaTime;
        controller.Move(motion);

        if (animator != null)
        {
            animator.SetFloat("Speed", inputDir.magnitude);
        }
    }
}
