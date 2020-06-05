using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb2d;

    public bool controlledLocally = false;
    public bool grounded = false;
    public float jumpStrength = 10.0f;
    public float moveSpeed = 10.0f;
    public long score = 0L;

    public Vector2 maxVelocity;

    public Animator animator;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if(controlledLocally)
        {
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Jump");

                Jump();
            }
            if(Input.GetAxisRaw("Horizontal") != 0.0f)
            {
                Debug.Log("Run");

                bool tmp = (Input.GetAxisRaw("Horizontal") == 1.0f) ? true : false;

                Move(tmp);
            }
        }

        Vector2 v = rb2d.velocity;

        if (v.x > maxVelocity.x) v.x = maxVelocity.x;
        if (v.x < -maxVelocity.x) v.x = -maxVelocity.x;

        if (v.y > maxVelocity.y) v.y = maxVelocity.y;
        if (v.y < -maxVelocity.y) v.y = -maxVelocity.y;

        rb2d.velocity = v;

        if (animator != null)
        {
            animator.SetFloat("velX", v.x);
            animator.SetFloat("velY", v.y);
        }
    }

    public void SetGrounded()
    {
        if (animator != null)
        {
            animator.SetBool("isGrounded", true);
        }
        grounded = true;
    }

    public void Jump()
    {
        if (grounded)
        {
            if (animator != null)
                animator.SetBool("isGrounded", false);
            rb2d.AddForce(Vector2.up * jumpStrength, ForceMode2D.Impulse);
            grounded = false;
            if (controlledLocally && MainGameManager.mgm != null)
                MainGameManager.mgm.QueueAction(0);
        }
    }

    public void Move(bool right)
    {
        rb2d.AddForce(Vector2.right * moveSpeed * (right ? 1f : -1f), ForceMode2D.Impulse);
        if (controlledLocally && MainGameManager.mgm != null)
            MainGameManager.mgm.QueueAction((right ? 2 : 1));
    }
}
