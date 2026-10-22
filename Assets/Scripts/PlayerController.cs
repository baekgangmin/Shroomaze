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
