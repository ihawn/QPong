using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float TimeScale;

    public GameState State;
    public Training Training;
    public BallController BallController;
    public PaddleController PaddleController;

    public bool IsTraining;
    public int EpochCount;
    public int QueueLength;
    public float DistanceReward;

    void Start()
    {
        IsTraining = true;
        Training = new Training(PaddleController, BallController);
    }

    void Update()
    {
        Time.timeScale = TimeScale;
        Training.Train();
    }
}
