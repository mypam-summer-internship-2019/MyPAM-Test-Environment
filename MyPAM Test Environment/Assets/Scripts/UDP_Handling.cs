using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using UnityEngine.UI;

public class UDP_Handling : MonoBehaviour
{
    static UdpClient listener;
    static IPEndPoint groupEP;
    static gameData gData;
    static string sendJson;
    public static string receivedData = "";
    public Text sText;

    //The single instance of this GameObject so it persists between scenes
    private static GameObject instance;

    ///////////////////////////////////////////////////////////////////
    private static double x0pos = 0;
    public static double X0pos { get { return x0pos; } set { } }

    private static double y0pos = 0;
    public static double Y0pos { get { return y0pos; } set { } }

    private static double z0pos = 0;
    public static double Z0pos { get { return z0pos; } set { } }

    private static double fx = 0;
    public static double Fx { get { return fx; } set { } }

    private static double fy = 0;
    public static double Fy { get { return fy; } set { } }

    private static double fz = 0;
    public static double Fz { get { return fz; } set { } }

    private static int encoder1 = 0;
    public static int Encoder1 { get { return encoder1; } set { } }

    private static int encoder2 = 0;
    public static int Encoder2 { get { return encoder2; } set { } }

    private static double pot1 = 0;
    public static double Pot1 { get { return pot1; } set { } }

    private static double pot2 = 0;
    public static double Pot2 { get { return pot2; } set { } }

    private static double theta0 = 0;
    public static double Theta0 { get { return theta0; } set { } }

    private static double theta1 = 0;
    public static double Theta1 { get { return theta1; } set { } }

    private static double x1pos = 0;
    public static double X1pos { get { return x1pos; } set { } }

    private static double y1pos = 0;
    public static double Y1pos { get { return y1pos; } set { } }

    private static double z1pos = 0;
    public static double Z1pos { get { return z1pos; } set { } }

    private static double x2pos = 0;
    public static double X2pos { get { return x2pos; } set { } }

    private static double y2pos = 0;
    public static double Y2pos { get { return y2pos; } set { } }

    private static double z2pos = 0;
    public static double Z2pos { get { return z2pos; } set { } }

    private static bool errorOccurred = false;
    public static bool ErrorOccurred { get { return errorOccurred; } set { } }

    ////////////////////////////////////////////////////////////////////////
    public static double Xtarget { get; set; } = 0;
    public static double Ytarget { get; set; } = 0;
    public static double Ztarget { get; set; } = 0;
    public static double X_Attractor1 { get; set; }
    public static double Y_Attractor1 { get; set; }
    public static double Z_Attractor1 { get; set; }
    public static double X_Attractor2 { get; set; }
    public static double Y_Attractor2 { get; set; }
    public static double Z_Attractor2 { get; set; }
    public static bool Traj_Flag { get; set; } = true;
    public static double DeadZone { get; set; } = 0;
    public static bool Assistance { get; set; } = true;
    public static bool Shutdown { get; set; } = false;
    public static double AssistanceLevel { get; set; } = 50;

    //////////////////////////////////////////////////////////////////////////

    void Start()
    {
        listener = new UdpClient(3000);
        groupEP = new IPEndPoint(IPAddress.Any, 3000);
        listener.Client.ReceiveBufferSize = 0;
        listener.Client.ReceiveTimeout = 15;
        gData = new gameData();
    }

    void Awake()
    {
        UDPsend("G|Initialise Game", 2200);

        
        //If an instance of this GameObject is already in the scene, delete this new instance
        //If not, make it so that this GameObject is not destroyed when a new scene loads
        if (instance == null)
        {
            instance = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != gameObject)
        {
            Destroy(gameObject);
        }
        

    }

    void Update()
    {
        getData();
        string data = "Sending Game Data|" + dataToSend();
        sText.text = data;
        UDPsend("G|" + data, 2300);


        //If the Escape key is pressed when in game, the game will initiate shutdown procedures
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }
    }

    private void getData()
    {
        string message = listenForData();
        try
        {
            controllerData data = JsonConvert.DeserializeObject<controllerData>(message);
            x0pos = data.X0pos;
            y0pos = data.Y0pos;
            z0pos = data.Z0pos;
            fx = data.Fx;
            fy = data.Fy;
            fz = data.Fz;
            encoder1 = data.Encoder1;
            encoder2 = data.Encoder2;
            pot1 = data.Pot1;
            pot2 = data.Pot2;
            theta0 = data.Theta0;
            theta1 = data.Theta1;
            x1pos = data.X1pos;
            y1pos = data.Y1pos;
            z1pos = data.Z1pos;
            x2pos = data.X2pos;
            y2pos = data.Y2pos;
            z2pos = data.Z2pos;
            errorOccurred = data.ErrorOccurred;

        }

        catch
        {
        }
    }

    private string dataToSend()
    {
        gData.Xtarget = Xtarget;
        gData.Ytarget = Ytarget;
        gData.Ztarget = Ztarget;
        gData.X_Attractor1 = X_Attractor1;
        gData.Y_Attractor1 = Y_Attractor1;
        gData.Z_Attractor1 = Z_Attractor1;
        gData.X_Attractor2 = X_Attractor2;
        gData.Y_Attractor2 = Y_Attractor2;
        gData.Z_Attractor2 = Z_Attractor2;
        gData.Traj_Flag = Traj_Flag;
        gData.DeadZone = DeadZone;
        gData.Assistance = Assistance;
        gData.Shutdown = Shutdown;
        gData.AssistanceLevel = AssistanceLevel;

        sendJson = JsonConvert.SerializeObject(gData);
        return (sendJson);
    }

    public string listenForData()
    {
        try
        {
            byte[] bytes = listener.Receive(ref groupEP);
            string[] message = ($"{Encoding.ASCII.GetString(bytes, 0, bytes.Length)}").Split('|');
            receivedData = message[1];
            return (message[1]);
        }
        catch
        {
            return ("timeout");
        }
    }

    private static void UDPsend(string datagram, int port)
    {
        byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(datagram);
        string IP = "127.0.0.1";
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client.SendTo(data, endPoint);
    }

    private void ExitGame()
    {
        Application.Quit();
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
