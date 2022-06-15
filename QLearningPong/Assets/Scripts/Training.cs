using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Training
{
    float[,,] QTable { get; set; }
    PaddleController PaddleController { get; set; }
    BallController BallController { get; set; }
    GameState GameState { get; set; }
    PongState CurrentPongState { get; set; }
    int CurrentAction { get; set; }
    bool PongStateWasUpdatedThisEpoch { get; set; }
    int LastBallPosX { get; set; }
    int LastBallPosY { get; set; }
    int LastPaddlePos { get; set; }
    float RandomExploreProbability { get { return Mathf.Exp(-EpochCount / (PaddleController.PaddlePositionCount * BallController.XDivisions * BallController.YDivisions * 2f)); } }
    int EpochCount { get; set; }
    bool GameStateChanged
    {
        get
        {
            return LastBallPosX != GameState.BallPositionX ||
                   LastBallPosY != GameState.BallPositionY ||
                   LastPaddlePos != GameState.PaddlePosition;
        }
    }

    public bool IsTraining { get; set; }
    public float LearningRate { get; set; }
    public float DiscountRate { get; set; }
    public Dictionary<PongState, float> Rewards { get; set; }

    public Training(PaddleController paddleController, BallController ballController)
    {
        IsTraining = true;
        PaddleController = paddleController;
        BallController = ballController;
        GameState = new GameState(paddleController, ballController);
        LearningRate = 0.1f;
        DiscountRate = 0.95f;
        EpochCount = 0;
        Rewards = new Dictionary<PongState, float>()
        {
            { PongState.NoEvent, 0 },
            { PongState.PaddleHitBall, 10 },
            { PongState.PaddleMissedBall, -500 }
        };

        QTable = new float[paddleController.PaddlePositionCount, ballController.XDivisions, ballController.YDivisions];
    }

    public void Train()
    {
        if(GameStateChanged)
        {

            EpochCount++;

            if(EpochCount >= BallController.gm.EpochCount)
            {
                BallController.gm.IsTraining = false;
                BallController.gm.TimeScale = 1f;
            }
                

            if(!PongStateWasUpdatedThisEpoch) //nothing happened this epoch
            {
                SetPongState(PongState.NoEvent);
            }
            PongStateWasUpdatedThisEpoch = false;
                

            int nextAction;

            //random action
            if (Random.Range(0f , 1f) < 0.3f && BallController.gm.IsTraining)
            {
                nextAction = Random.Range(0, PaddleController.PaddlePositionCount);
            }
            else
            {
                //pick action with max expected reward from q table
                float maxQ = float.MinValue;
                int maxPositionIndex = 0;
                for(int i = 0; i < PaddleController.PaddlePositionCount; i++)
                {
                    float score = QTable[i, GameState.BallPositionX, GameState.BallPositionY];
                    if(score > maxQ)
                    {
                        maxQ = score;
                        maxPositionIndex = i;
                    }
                }

                nextAction = maxPositionIndex;
            }

            //Do the action
            PaddleController.SetTargetPosition(PaddleController.PaddlePositions[nextAction]);
            CurrentAction = nextAction;
        }

        LastBallPosX = GameState.BallPositionX;
        LastBallPosY = GameState.BallPositionY;
        LastPaddlePos = GameState.PaddlePosition;
    }

    public void SetPongState(PongState state)
    {
        PongStateWasUpdatedThisEpoch = state != PongState.NoEvent;
        CurrentPongState = state;
        UpdateReward();
    }

    void UpdateReward()
    {
        float maxExpectedReward = float.MinValue;
        for (int i = 0; i < PaddleController.PaddlePositionCount; i++)
            if (QTable[i, GameState.BallPositionX, GameState.BallPositionY] > maxExpectedReward)
                maxExpectedReward = QTable[i, GameState.BallPositionX, GameState.BallPositionY];

        float currentQValue = QTable[GameState.PaddlePosition, GameState.BallPositionX, GameState.BallPositionY];

        //Bellman equation
        float update = currentQValue + LearningRate * (GetCurrentReward() + DiscountRate * maxExpectedReward - currentQValue);
        QTable[GameState.PaddlePosition, GameState.BallPositionX, GameState.BallPositionY] = update;          
    }

    float GetCurrentReward()
    {
        float reward = CurrentPongState == PongState.NoEvent ? DistanceReward() : Rewards[CurrentPongState];
        return reward;
    }

    float DistanceReward()
    {
        float reward = Mathf.Abs(BallController.transform.position.x - PaddleController.transform.position.x) < 20f ? Mathf.Min(2, 1f / (Mathf.Abs(BallController.transform.position.y - PaddleController.transform.position.y)) + 0.1f) : 0f;
        BallController.gm.DistanceReward = reward;
        return reward;
    }
}

public class GameState
{
    const float MinX = -10;
    const float MaxX = 9;
    const float MinY = -5;
    const float MaxY = 5;
    const float MinPaddleY = -4;
    const float MaxPaddleY = 4;
    PaddleController PaddleController { get; set; }
    BallController BallController { get; set; }
    public int PaddlePosition
    {
        get
        {
            return (int)Mathf.Clamp(Mathf.Floor((PaddleController.transform.position.y - MinPaddleY) * (PaddleController.PaddlePositionCount / (MaxPaddleY - MinPaddleY))), 0, PaddleController.PaddlePositionCount - 1);
        }
    }
    public int BallPositionX
    {
        get
        {
            return (int)Mathf.Clamp(Mathf.Floor((BallController.transform.position.x - MinX) * (BallController.XDivisions / (MaxX - MinX))), 0, BallController.XDivisions - 1);
        }
    }
    public int BallPositionY
    {
        get
        {
            return (int)Mathf.Clamp(Mathf.Floor((BallController.transform.position.y - MinY) * (BallController.YDivisions / (MaxY - MinY))), 0, BallController.YDivisions - 1);
        }
    }

    public GameState(PaddleController paddleController, BallController ballController)
    {
        PaddleController = paddleController;
        BallController= ballController;
    }
}

public enum PongState
{
    NoEvent = 0,
    PaddleHitBall = 1,
    PaddleMissedBall = 2,
}