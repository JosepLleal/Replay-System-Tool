using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public CharacterController controller;
    public Animator animator;
    public Transform cam;

    public float gravity = -9.81f;
    Vector3 velocity;

    public float pushPower = 2.0f;
    public float speed = 6f;
    public float jumpSpeed = 4f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    float timerGrounded = 0;

    private bool isJumping = false;
    private bool isGrounded = false;

    public ReplayManager replay;

    private void Start()
    {
        if (replay == null)
            replay = GameObject.Find("ReplayManager").GetComponent<ReplayManager>();

        if (cam == null)
            cam = GameObject.Find("Camera").transform;

    }
    private void Update()
    {
        if (replay.ReplayMode())
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        Vector3 moveDir = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            if (controller.isGrounded)
                animator.SetBool("isWalking", true);

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            //controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }



        if (controller.isGrounded)
        {
            timerGrounded += Time.deltaTime;
            animator.SetBool("isGrounded", true);
            isGrounded = true;
            animator.SetBool("isJumping", false);
            isJumping = false;
            animator.SetBool("isFalling", false);

            if (Input.GetKeyDown(KeyCode.Space) && timerGrounded > 0.2f)
            {
                velocity.y = jumpSpeed;
                animator.SetBool("isJumping", true);
                isJumping = true;
            }
        }
        else
        {
            timerGrounded = 0f;
            animator.SetBool("isGrounded", false);
            if (isGrounded && isJumping == false)
                velocity.y = gravity * 4 * Time.deltaTime;

            isGrounded = false;

            if ((isJumping && velocity.y < 0f) || velocity.y < -2f)
            {
                animator.SetBool("isFalling", true);
            }
        }


        velocity.y += gravity * Time.deltaTime;

        //if (velocity.y > 30f)
        //    velocity.y = 30f;

        moveDir = moveDir.normalized * speed;
        moveDir.y = velocity.y;

        Debug.Log(moveDir);

        controller.Move(moveDir * Time.deltaTime);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
            return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f)
            return;

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower * speed;
    }

}
