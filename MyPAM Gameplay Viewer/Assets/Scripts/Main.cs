using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using Crosstales.FB;
using UnityEngine.UI;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Diagnostics;
using System.Threading;
public class Main : MonoBehaviour
{
    // Start is called before the first frame update

    static bool mlcMode = true;
    static string MyPAM_ID = "1";

    static string storedDBFilePath = (Directory.GetCurrentDirectory() + @"\dbLocation.txt");
    public Dropdown usernames;
    public Text uName;
    public Dropdown sessions;
    public Text sName;
    public Dropdown games;
    public Text gName;

    static IDbConnection connect;
    static IDbCommand command;

    static DateTime currentConTime;
    static DateTime preConTime;

    static DateTime currentGameTime;
    static DateTime preGameTime;

    static string choosesnSession;
    static string choosesnGame;
    static string choosesnGameSession;

    public GameObject player;
    public GameObject activeTarget;
    public GameObject oldTarget;
    public GameObject Trail;

    public static int playerCount = 1;
    public static int gameCount = 1;

    static Stopwatch controllerStopWatch;
    static Stopwatch gameStopWatch;

    bool playGameplay = false;

    static List<string> gameDataList;
    static List<string> controllerDataList;
    static Vector3 playerPos;
    static Vector3 targetPos;
    static Vector3 prePlayerPos;
    static Vector3 preTargetPos;
    

    public Slider gameplayPostion;
    public Slider trailLength;
    static public float trailLengthVal;

    static float conTime = 0;
    static float gametime = 0;

    static float playerMoveTime = 0;
    static float targetMoveTime = 0;

    public static bool gameSet = false;

    void Start()
    {
        var proc = new ProcessStartInfo();
        proc.UseShellExecute = true;
        proc.WorkingDirectory = @"C:\Windows\System32";
        proc.FileName = @"C:\Windows\System32\cmd.exe";
        proc.Verb = "runas";

        controllerStopWatch = new Stopwatch();
        gameStopWatch = new Stopwatch();

        gameDataList = new List<string>();
        controllerDataList = new List<string>();
        if (mlcMode)
        {
            string databasePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\MLC Application Data\MLC Settings and Database\MyPAM_Database_" + MyPAM_ID + ".sqlite3";
            string connectionName = "URI=file:" + databasePath;
            connect = new SqliteConnection(connectionName);
        }

        else
        {
            if (File.Exists(storedDBFilePath))
            {

            }

            else
            {
                TextWriter dbLocation = new StreamWriter(storedDBFilePath);
                dbLocation.WriteLine("Database_Directory");
                dbLocation.Close();
            }

            string[] pathDataArray = File.ReadAllLines(storedDBFilePath);
            if (File.Exists(pathDataArray[0]))
            {

            }
            else
            {
                string path = FileBrowser.OpenSingleFile("sqlite3");
                TextWriter dbLocation = new StreamWriter(storedDBFilePath);
                dbLocation.WriteLine(path);
                dbLocation.Close();
            }

            pathDataArray = File.ReadAllLines(storedDBFilePath);
            string connectionName = "URI=file:" + pathDataArray[0];
            connect = new SqliteConnection(connectionName);
        }
        
        
        command = connect.CreateCommand();

        getUsernames();

    }

    // Update is called once per frame
    void Update()
    {
        trailLengthVal = trailLength.value;
        if (gameSet)
        {
            if (playerCount >= controllerDataList.Count || gameCount >= gameDataList.Count)
            {

            }
            else
            {
                gameplayPostion.value = playerCount;
                if (playGameplay)
                {
                    beginPlayback();
                }
            }
        }
        
    }

    void getUsernames()
    {
        connect.Open();
        command.CommandText = "SELECT Username FROM User_Information";

        usernames.ClearOptions();
        usernames.value = 0;
        sessions.ClearOptions();
        sessions.value = 0;
        games.ClearOptions();
        games.value = 0;

        List<string> usernamesLits = new List<string>();
        usernamesLits.Add("");
        using (IDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                usernamesLits.Add(reader["Username"].ToString()); ;
            }
        }
        usernames.AddOptions(usernamesLits);
        connect.Close();
    }


    public void changeDB()
    {
        string path = FileBrowser.OpenSingleFile("sqlite3");
        TextWriter dbLocation = new StreamWriter(storedDBFilePath);
        dbLocation.WriteLine(path);
        dbLocation.Close();

        string connectionName = "URI=file:" + path;
        connect = new SqliteConnection(connectionName);
        command = connect.CreateCommand();
        getUsernames();
    }

    public void usernameChosen()
    {

        connect.Open();

        command.CommandText = "SELECT Session_ID, Session_Start FROM Session_Data WHERE Username = '" + uName.text + "'";
        sessions.ClearOptions();
        sessions.value = 0;
        games.ClearOptions();
        games.value = 0;
        List<string> sessionData = new List<string>();
        sessionData.Add("");
        using (IDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {


                sessionData.Add(reader["Session_ID"].ToString() + "|" + reader["Session_Start"].ToString()); ;
            }
        }
        sessions.AddOptions(sessionData);

        connect.Close();
    }

    public void sessionChosen()
    {
        connect.Open();

        choosesnSession = sName.text.Split('|')[0];
        command.CommandText = "SELECT Game_Name, Play_Count FROM Game_Session_Data WHERE Username = '" + uName.text + "'AND Session_ID = '" + choosesnSession + "'";
        games.ClearOptions();
        games.value = 0;
        List<string> gamesLits = new List<string>();
        gamesLits.Add("");
        using (IDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                gamesLits.Add(reader["Play_Count"].ToString() + "|" +  reader["Game_Name"].ToString()); ;
            }
        }
        games.AddOptions(gamesLits);

        connect.Close();

        
    }

    public void gameChosen()
    {
        if (gName.text != "")
        {
            choosesnGame = gName.text.Split('|')[1];
            choosesnGameSession = gName.text.Split('|')[0];
        }
        
    }

    public void load()
    {
        restViewer();
        choosesnSession = sName.text.Split('|')[0];
        choosesnGame = gName.text.Split('|')[1];
        choosesnGameSession = gName.text.Split('|')[0];

        connect.Open();
        command.CommandText = "SELECT Controller_Data, Controller_Data_Time, Game_Data, Game_Data_Time FROM Gameplay_Data WHERE Username = '" + uName.text + "' AND Session_ID = '" + choosesnSession + "'AND Game_Name = '" + choosesnGame + "'AND Play_Count = '" + choosesnGameSession + "'";

       
        
        bool first = true;

        using (IDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {

                if (first)
                {
                    preConTime = Convert.ToDateTime(reader["Controller_Data_Time"]);
                    preGameTime = Convert.ToDateTime(reader["Game_Data_Time"]);
                }
                else
                {
                    controllerData cData = JsonConvert.DeserializeObject<controllerData>(reader["Controller_Data"].ToString());
                    currentConTime = Convert.ToDateTime(reader["Controller_Data_Time"]);
                    TimeSpan cElapsed = currentConTime - preConTime;
                    preConTime = currentConTime;
                    string addC = cData.X2pos.ToString() + "|" + cData.Y2pos.ToString() + "|" + cData.Z2pos.ToString() + "|" + cElapsed.TotalMilliseconds;
                    controllerDataList.Add(addC);

                    gameData gData = JsonConvert.DeserializeObject<gameData>(reader["Game_Data"].ToString());
                    currentGameTime = Convert.ToDateTime(reader["Game_Data_Time"]);
                    TimeSpan gElapsed = currentGameTime - preGameTime;
                    preGameTime = currentGameTime;
                    string addG = gData.Xtarget.ToString() + "|" + gData.Ytarget.ToString() + "|" + gData.Ztarget.ToString() + "|" + gElapsed.TotalMilliseconds;
                    gameDataList.Add(addG);
                }
                first = false;
            }
        };
        connect.Close();

        string[] currentCData = controllerDataList[0].Split('|');
        prePlayerPos = (new Vector3(float.Parse(currentCData[0]), float.Parse(currentCData[1]), float.Parse(currentCData[2]))) / 30 + new Vector3(3.565f, 1.315f, 0);

        string[] currentGData = gameDataList[0].Split('|');
        preTargetPos = (new Vector3(float.Parse(currentGData[0]), float.Parse(currentGData[1]), float.Parse(currentGData[2]))) / 30 + new Vector3(3.565f, 1.315f, 0);

        gameplayPostion.maxValue = controllerDataList.Count-1;
        gameSet = true;
/*
        for (int i = 0; i < controllerDataList.Count; i++)
        {
            currentCData = controllerDataList[i].Split('|');
            Vector3 playerTarilPos = (new Vector3(float.Parse(currentCData[0]), float.Parse(currentCData[1]), float.Parse(currentCData[2]))) / 30 + new Vector3(3.565f, 1.315f, 0);            
            var newName = Instantiate(Trail, playerTarilPos, Quaternion.identity);
            newName.name = i.ToString();
        }
        */
    }


    public void playFlip()
    {
        playGameplay = !playGameplay;
    }

    public void sliderChanged()
    {
            if (gameSet)
            {
                playerCount = (int)gameplayPostion.value;
                string[] currentCData = controllerDataList[playerCount - 1].Split('|');
                prePlayerPos = (new Vector3(float.Parse(currentCData[0]), float.Parse(currentCData[1]), float.Parse(currentCData[2]))) / 30 + new Vector3(3.565f, 1.315f, 0);
                currentCData = controllerDataList[playerCount].Split('|');
                playerPos = (new Vector3(float.Parse(currentCData[0]), float.Parse(currentCData[1]), float.Parse(currentCData[2]))) / 30 + new Vector3(3.565f, 1.315f, 0);
                var newName = Instantiate(Trail, playerPos, Quaternion.identity);
                newName.name = playerCount.ToString();

                gameCount = (int)gameplayPostion.value;
                string[] currentGData = gameDataList[gameCount].Split('|');
                targetPos = (new Vector3(float.Parse(currentGData[0]), float.Parse(currentGData[1]), float.Parse(currentGData[2]))) / 30 + new Vector3(3.565f, 1.315f, 0);
                currentGData = gameDataList[gameCount - 1].Split('|');
                preTargetPos = (new Vector3(float.Parse(currentGData[0]), float.Parse(currentGData[1]), float.Parse(currentGData[2]))) / 30 + new Vector3(3.565f, 1.315f, 0);

                player.transform.position = prePlayerPos;
                activeTarget.transform.position = preTargetPos;

                controllerStopWatch.Reset();
                gameStopWatch.Reset();
            }
    }

    void beginPlayback()
    {
        //////Controller
        controllerStopWatch.Start();
        if (controllerStopWatch.ElapsedMilliseconds >= conTime)
        {
            prePlayerPos = playerPos;
            string[] currentCData = controllerDataList[playerCount].Split('|');
            playerPos = (new Vector3(float.Parse(currentCData[0]), float.Parse(currentCData[1]), float.Parse(currentCData[2])))/ 30 + new Vector3(3.565f, 1.315f, 0);
            conTime = float.Parse(currentCData[2]);
            controllerStopWatch.Reset();
            playerCount += 1;
        }
        playerMoveTime += Time.deltaTime / conTime;
        player.transform.position = Vector3.Lerp(prePlayerPos, playerPos, playerMoveTime);

        /////// Game
        gameStopWatch.Start();
        if (gameStopWatch.ElapsedMilliseconds >= gametime)
        {
            preTargetPos = targetPos;
            string[] currentGData = gameDataList[gameCount].Split('|');
            targetPos = (new Vector3(float.Parse(currentGData[0]), float.Parse(currentGData[1]), float.Parse(currentGData[2]))) / 30 + new Vector3(3.565f, 1.315f, 0);
            gametime = float.Parse(currentGData[2]);
            gameStopWatch.Reset();
            gameCount += 1;
        }
        targetMoveTime += Time.deltaTime / gametime;
        activeTarget.transform.position = Vector3.Lerp(preTargetPos, targetPos, targetMoveTime);

    }

    void restViewer()
    {
        gameSet = false;
        gameDataList.Clear();
        controllerDataList.Clear();
        playerCount = 1;
        gameCount = 1;
    }
    private class gameData
    {
        public double Xtarget { get; set; }
        public double Ytarget { get; set; }
        public double Ztarget { get; set; }
        public double X_Attractor1 { get; set; }
        public double Y_Attractor1 { get; set; }
        public double Z_Attractor1 { get; set; }
        public double X_Attractor2 { get; set; }
        public double Y_Attractor2 { get; set; }
        public double Z_Attractor2 { get; set; }
        public bool Traj_Flag { get; set; }
        public double DeadZone { get; set; }
        public bool Assistance { get; set; }
        public bool Shutdown { get; set; }
        public double AssistanceLevel { get; set; }
    }

    private class controllerData
    {
        public double X0pos { get; set; }
        public double Y0pos { get; set; }
        public double Z0pos { get; set; }
        public double Fx { get; set; }
        public double Fy { get; set; }
        public double Fz { get; set; }
        public Int32 Encoder1 { get; set; }
        public Int32 Encoder2 { get; set; }
        public double Pot1 { get; set; }
        public double Pot2 { get; set; }
        public double Theta0 { get; set; }
        public double Theta1 { get; set; }
        public double X1pos { get; set; }
        public double Y1pos { get; set; }
        public double Z1pos { get; set; }
        public double X2pos { get; set; }
        public double Y2pos { get; set; }
        public double Z2pos { get; set; }
        public bool ErrorOccurred { get; set; }
    }

}
