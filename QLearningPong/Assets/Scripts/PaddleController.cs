using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public int PaddlePositionCount;

    [SerializeField]
    private float MoveSpeed;
    public List<float> PaddlePositions { get; set; }
    private float TargetPosition { get; set; }
    private Vector3 TargetVector { get; set; }
    private const float Bounds = 4;

    public GameManager GameManager;

    void Start()
    {
        PaddlePositions = new List<float>();
        for(int i = 0; i < PaddlePositionCount; i++)
        {
            PaddlePositions.Add(-Bounds + i * 2 * Bounds / (PaddlePositionCount - 1));
        }

        //switch first and middle positiions so paddle stays towards the middle
        int middleIndex = (int)Mathf.Round(PaddlePositionCount / 2);
        float temp = PaddlePositions[0];
        PaddlePositions[0] = PaddlePositions[middleIndex];
        PaddlePositions[middleIndex] = temp;

        TargetPosition = PaddlePositions[0];
        SetTargetPosition(TargetPosition);
    }

    void Update()
    {
        transform.position = TargetVector;// Vector3.Lerp(transform.position, TargetVector, MoveSpeed * Time.deltaTime);
    }

    public void SetTargetPosition(float p)
    {
        TargetPosition = p;
        TargetVector = new Vector3(transform.position.x, TargetPosition, 0);
    }
}
