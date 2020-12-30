﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class GameManager : Agent
{
    // Start is called before the first frame update
    static int round = 0;
    float time;
    GameObject paddle;
    GameObject ball;
    public GameObject brick;
    private List<GameObject> bricks = new List<GameObject>();

    public int amountOfPlayersPerRound;
    public static List<GameObject> PlayerList;
    public GameObject personalityCompetitive; //TEMP, will be personality list later
    public GameObject personalityNewbie;
    public GameObject personalityExperienced;

    private float brickHeight;
    private float[] roundCharacteristics = new float[3] { 4.5f, 25f, 10f };

    private int episodeNumber = 0;
    private int episodeCount = 0;

    private bool stopped = true;

    private Observations latestObservations;


    void generatePlayerList()
    {
        List<GameObject> personalities = new List<GameObject>();
        PlayerList = new List<GameObject>();
        personalities.Add(personalityNewbie);
        personalities.Add(personalityCompetitive);
        personalities.Add(personalityExperienced);

        for (int i = 0; i < amountOfPlayersPerRound; i++)
        {
            GameObject player = Instantiate(personalities[i % personalities.Count]);
            PlayerList.Add(player);
            player.SetActive(false);
        }

        foreach (GameObject p in PlayerList)
            DontDestroyOnLoad(p);

        //register to csv ()
        // test
        // ML AGENTS HEREEEEEEEEEEEEE

    }


    void resetBricks()
    {
        for (int i = 0; i < bricks.Count; i++)
        {
            Destroy(bricks[i]);
        }

        bricks = new List<GameObject>();



        for (int i = 0; i < 9; i++)
        {
            GameObject tijolo = Instantiate(brick, new Vector3(i * 2 - 8f, brickHeight, 0), Quaternion.identity);
            bricks.Add(tijolo);
        }
        for (int i = 0; i < 9; i++)
        {
            GameObject tijolo = Instantiate(brick, new Vector3(i * 2 - 8f, brickHeight - 0.5f, 0), Quaternion.identity);
            bricks.Add(tijolo);
        }
        for (int i = 0; i < 9; i++)
        {
            GameObject tijolo = Instantiate(brick, new Vector3(i * 2 - 8f, brickHeight - 1f, 0), Quaternion.identity);
            bricks.Add(tijolo);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!stopped)
        {
            time += Time.deltaTime;
            if (ball.GetComponent<BallScript>().getHitFloor())
                ManagerLogs(0);

            if (destructionCheck())
                ManagerLogs(1);
        }
    }

    private bool destructionCheck()
    {
        bool a = true;
        foreach (GameObject brick in bricks)
        {
            if (brick != null)
                a = false;
        }
        return a;
    }

    private int bricksCount()
    {
        int a = 0;
        foreach (GameObject brick in bricks)
        {
            if (brick != null)
                a++;
        }
        return a;
    }

    private void SummonPlayer()
    {
        PlayerList[episodeNumber - 1].SetActive(true);
    }



    private void ManagerLogs(int win)
    {
        // Set dot as default for floats
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

        stopped = true;

        //call Personality, give game data to obtain satisfaction
        //write logs
        //restart scene with new player, for now just restart

        //float satisfaction = PlayerList[round-1].GetComponent<Personality>().CalculateSatisfaction(win, time);
        SetReward(1);
        float paddleDistance = paddle.GetComponent<PaddleScript>().distanceRan;
        float ballHits = paddle.GetComponent<PaddleScript>().ballHits;

        float[] playerVars = PlayerList[episodeNumber - 1].GetComponent<Personality>().GetVariables();
        float[] playerQED = PlayerList[episodeNumber - 1].GetComponent<Personality>().GetGEQ(paddleDistance, ballHits, time, bricksCount(), win);

        PlayerList[episodeNumber - 1].SetActive(false);



        string strFilePath = @"./data.csv";

        if (round == 1)
        {
            File.WriteAllText(strFilePath, "session id;brick height;paddle speed;ball speed;time; paddle distance; ballHits; amount of bricks;win/lose;type of personality;playerAPM;playerReactionTime;playerPaddleSafety;GEQ - content;GEQ - skillful;GEQ - occupied;GEQ - difficulty;satisfaction"); //COMMENT THIS IF YOU JUST WANT TO APPEND - last 5 are player attributes
            File.AppendAllText(strFilePath, Environment.NewLine);
        }
        //session id, time, type of personality, amount of bricks,win/lose


        //float[] outputarray = new float[] { round, roundCharacteristics[0], roundCharacteristics[1], roundCharacteristics[2], time, playerVars[0], bricksCount(), win, playerVars[1], playerVars[2], playerVars[3], playerQED[0], playerQED[1], playerQED[2], playerQED[3], playerQED[4] }; //valores das colunas
        float[] outputarray = new float[] { round, roundCharacteristics[0], roundCharacteristics[1], roundCharacteristics[2], time, paddleDistance, ballHits, bricksCount(), win, playerVars[0], playerVars[1], playerVars[2], playerVars[3], playerQED[0], playerQED[1], playerQED[2], playerQED[3], playerQED[4] }; //valores das colunas
        this.latestObservations = new Observations(time, paddleDistance, ballHits, bricksCount(), win, playerVars, playerQED);
        time = 0;
        paddle.GetComponent<PaddleScript>().resetValues();
        StringBuilder sbOutput = new StringBuilder();
        sbOutput.AppendLine(string.Join(";", outputarray));
        Debug.Log(sbOutput);

        // Create and write the csv file
        // File.WriteAllText(strFilePath, sbOutput.ToString());

        // To append more lines to the csv file
        File.AppendAllText(strFilePath, sbOutput.ToString());

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // 

        if(round == 10)
        {
            round = 0;
            EndEpisode();
        }
        else
        {

            Debug.Log("Game end");
            initGame();
            Debug.Log("Requesting parameters");
            RequestDecision();
        }
    }

    private void EndRound()
    {
        round = 0;
        //get new players, train based on ML, etc.
    }

    void initGame()
    {
        stopped = true;
        time = 0;
        paddle = GameObject.Find("Paddle");
        ball = GameObject.Find("Ball");
        paddle.transform.position = new Vector3(0, -4, 0);
        ball.transform.position = new Vector3(0, -3, 0);
        ball.GetComponent<BallScript>().Reset();
        paddle.SetActive(false);
        ball.SetActive(false);
    }

    private class Observations
    {
        public float time { get; set; }
        public float paddleDistance { get; set; }
        public float ballHits { get; set; }
        public int bricksCount { get; set; }
        public int win { get; set; }
        public float[] playerVars { get; set; }
        public float[] playerQED { get; set; }

        public Observations(float time, float paddleDistance, float ballHits, int bricksCount, int win, float[] playerVars, float[] playerQED)
        {
            this.time = time;
            this.paddleDistance = paddleDistance;
            this.ballHits = ballHits;
            this.bricksCount = bricksCount;
            this.win = win;
            this.playerVars = playerVars;
            this.playerQED = playerQED;
        }
    }














    public override void Initialize()
    {
        round = 0;
        generatePlayerList();
        this.latestObservations = new Observations(0, 0, 0, 0, 0, new float[4], new float[4]);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.latestObservations.time);
        sensor.AddObservation(this.latestObservations.paddleDistance);
        sensor.AddObservation(this.latestObservations.ballHits);
        sensor.AddObservation(this.latestObservations.bricksCount);
        sensor.AddObservation(this.latestObservations.win);
        sensor.AddObservation(this.latestObservations.playerVars[0]);
        sensor.AddObservation(this.latestObservations.playerVars[1]);
        sensor.AddObservation(this.latestObservations.playerVars[2]);
        sensor.AddObservation(this.latestObservations.playerVars[3]);
        sensor.AddObservation(this.latestObservations.playerQED[0]);
        sensor.AddObservation(this.latestObservations.playerQED[1]);
        sensor.AddObservation(this.latestObservations.playerQED[2]);
        sensor.AddObservation(this.latestObservations.playerQED[3]);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Debug.Log("Parameters received");
        brickHeight = vectorAction[0] + 1;
        resetBricks(); // Delete old bricks and create new ones
        paddle.SetActive(true);
        ball.SetActive(true);
        paddle.GetComponent<PaddleScript>().PaddleSpeed = vectorAction[1] + 1;
        ball.GetComponent<BallScript>().SetSpeed(vectorAction[2] + 1);
        stopped = false;
        PlayerList[episodeNumber - 1].SetActive(true);
        PlayerList[episodeNumber - 1].GetComponent<Personality>().Play();
        round++;
        Debug.Log("Starting the game");
    }

    public override void Heuristic(float[] actionsOut)
    {
    }

    public override void OnEpisodeBegin()
    {
        stopped = true;
        Debug.Log("episode begin");
        episodeNumber++;
        episodeCount++;
        if (episodeNumber >= PlayerList.Count)
            episodeNumber = 0;
        initGame(); // Find the ball and Reset the position of the ball and paddle and time
        SummonPlayer(); // Change the player
        Debug.Log("Requesting parameters");
        RequestDecision();
    }
}
