using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private Transform _checkpoints;
    private CheckpointProperties[] _directionsList;

    private void Awake()
    {
        _directionsList = _checkpoints.GetComponentsInChildren<CheckpointProperties>();
    }

    public string OutputDirections()
    {
        string answer = "Directions\n";
        int directionNumber = 1;
        foreach(CheckpointProperties direction in _directionsList)
        {
            if (!direction.activated)
            {
                answer += $"{directionNumber}. {direction.HowToGetToThisCheckpoint}\n";
                directionNumber++;
            }
        }

        return answer;
    }
}
