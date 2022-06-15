using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public float maxSpeed;
    public Vector3 Speed;

    public int XDivisions;
    public int YDivisions;

    public GameManager gm;

    void Start()
    {
        ResetVelocity();
    }


    void FixedUpdate()
    {
        transform.position += Speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Vertical":
                Speed.x = -Speed.x;
                break;
            case "Horizontal":
                Speed.y = -Speed.y;
                break;
            case "Paddle":
                Speed.x = -Random.Range(maxSpeed / 2, maxSpeed);
                Speed.y = Random.Range(maxSpeed / 2, maxSpeed) * (Random.Range(0f, 1f) < 0.5f ? 1f : -1f);
                gm.Training.SetPongState(PongState.PaddleHitBall);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.gameObject.tag)
        {
            case "Catcher":
                transform.position = Vector3.zero;
                gm.Training.SetPongState(PongState.PaddleMissedBall);
                ResetVelocity();
                break;
        }
    }

    void ResetVelocity()
    {
        float xSpeed = Random.Range(maxSpeed / 2, maxSpeed) * (Random.Range(0f, 1f) < 0.5f ? 1f : -1f);
        float ySpeed = Random.Range(maxSpeed / 2, maxSpeed) * (Random.Range(0f, 1f) < 0.5f ? 1f : -1f);
        Speed = new Vector3(xSpeed, ySpeed, 0);
    }
}
