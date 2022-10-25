using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreBoardUIManager : MonoBehaviour
{
    public static ScoreBoardUIManager Instance;

    public ScoreBoard scoreBoardPrefab;
    [SerializeField] List<ScoreBoard> scoreBoards = new List<ScoreBoard>();

    public Dictionary<string, ScoreBoard> boardDic = new Dictionary<string, ScoreBoard>();
    Queue<ScoreBoard> availableScoreBoard = new Queue<ScoreBoard>();
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        InitializeQueue();
    }
    private void Start()
    {
        gameObject.SetActive(false);
    }

    void InitializeQueue()
    {
        foreach (var item in scoreBoards)
        {
            availableScoreBoard.Enqueue(item);
        }
    }

    public void AddScoreBoard(string name)
    {
        if (gameObject.activeSelf==false)
        {
            gameObject.SetActive(true);
        }
        var newScoreBoard = availableScoreBoard.Dequeue();
        newScoreBoard.gameObject.SetActive(true);
        newScoreBoard.name = name;
        newScoreBoard.playerName = name;
        newScoreBoard.playerNameTMP.text = name;
        newScoreBoard.scoreTMP.text = 0.ToString();
        if (!boardDic.ContainsKey(name))
        {
            boardDic.Add(name, newScoreBoard);
        }

    }

    public void UpdateScoreBoard(string name)
    {
        ScoreBoard scoreBoard;
        boardDic.TryGetValue(name, out scoreBoard);
        scoreBoard.score += 100;
        scoreBoard.scoreTMP.text = scoreBoard.score.ToString();
    }

    public void ClearScore()
    {
        foreach (KeyValuePair<string,ScoreBoard> item in boardDic)
        {
            item.Value.score = 0;
            item.Value.scoreTMP.text = item.Value.score.ToString();
        }
    }
}
