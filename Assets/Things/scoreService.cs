using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Security.Cryptography;

[System.Serializable]
public class ScoreEntry
{
    public string id;
    public string time;
    public string player;
    public int score;
    public string gameId;
}

[System.Serializable]
public class ScoreList
{
    public ScoreEntry[] scores;
}

[System.Serializable]
public class ScorePost {
    public string player;
    public int score;
    public string time;
    public string validation;
}

public class ScoreService : MonoBehaviour
{
    string baseUrl = "http://localhost:3000";
    string gameId = "d33491cd-ecbd-4405-8b0a-dc89baab0893";
    string secret = "secret";

    public string playerName = "Player";
    public ScoreList scores = new ScoreList();

    public GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        RefreshScores();
        gm = GameManager.GetGameManager();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshScores()
    {
        StartCoroutine(GetScores());
    }

    public void ReportScore(int score)
    {
        StartCoroutine(PostScore(makeScorePost(score)));
    }

    public void ReportName(string newName) {
        playerName = newName;
    }

    IEnumerator GetScores()
    {
        UnityWebRequest req = UnityWebRequest.Get(baseUrl + "/game/" + gameId + "/scores");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(req.error);
        }
        else
        {
            // Show results as text
            Debug.Log(req.downloadHandler.text);
            // Not hacky at all
            scores = JsonUtility.FromJson<ScoreList>("{\"scores\":" + req.downloadHandler.text + "}");
            foreach (var score in scores.scores) {
                Debug.Log(score.player + ": " + score.score);
            }

            gm.ReportHighScoresAvailable();
        }
    }

    IEnumerator PostScore(ScorePost score)
    {
        WWWForm form = new WWWForm();
        form.AddField("player", score.player);
        form.AddField("score", score.score);
        form.AddField("time", score.time);
        form.AddField("validation", score.validation);
 
        UnityWebRequest req = UnityWebRequest.Post(baseUrl + "/game/" + gameId + "/score", form);

        yield return req.SendWebRequest();

        RefreshScores();
    }

    public ScorePost makeScorePost(int score) {
        ScorePost post = new ScorePost();
        post.player = playerName;
        post.score = score;
        post.time = System.DateTime.Now.ToString("o");

        string toValidate = $"{gameId}-{score}-{playerName}-{post.time}-{secret}";
        Debug.Log(toValidate);
        var encoding = new System.Text.UTF8Encoding();
        byte[] validateBytes = encoding.GetBytes(toValidate);
        byte[] hash = ((HashAlgorithm) CryptoConfig.CreateFromName("MD5")).ComputeHash(validateBytes);
        post.validation = System.BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();

        return post;
    }

}
