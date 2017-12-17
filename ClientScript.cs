using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ClientScript : MonoBehaviour {

    string serverIP = "192.168.1.144";
    private PlayerPrefsWrapper pw;
    StreamReader reader;
    StreamWriter writer;
    Thread networkThread;
    Boolean serverConnectionIsAlive = false;

    Vector3 coordinates = new Vector3(-200, -200, 0);
    Vector3 modelOnscreenLocation = new Vector3(-200, -200, 0);
    
    Text connectionStatus;
    Text xCoord;
    Text yCoord;
    Text zCoord;
    Text angle;
    Text factor;
    Text tableAngle;
    
    float angleFloat;
    float tableAngleFloat;
    
    string connectionStatusString = "";
    string xCoordString = "0";
    string yCoordString = "0";
    string zCoordString = "0";
    string angleString = "0";
    string tableAngleString = "0";
    
    int deviceID = 0;
    int layerOrder = 0;

    public float heightVal = 0;
    public float widthVal = 0;
    public float scaleVal = 0;
    public float factorVal = 1.0F;
    Vector3 scaleVector = new Vector3(20, 20, 20);

    public GameObject currentModel;
    private GameObject handSkin;
    private GameObject handSkeleton;
    private GameObject handNervous;
    private GameObject handMuscles;
    private GameObject handCirculatory;
    Boolean hide = false;
    Boolean autoSwitch = true;
    int autoSwitchCounter = 0;
    GameObject mainCam;

    Button upButt;
    Button downButt;
    Button rightButt;
    Button leftButt;
    Button scaleUp;
    Button scaleDown;
    Button factorUp;
    Button factorDown;
    Button showHide;
    Button swapLayer;

    void Start() {
        pw = GetComponent<PlayerPrefsWrapper>();
        serverIP = pw.GetIPKey();
        deviceID = Int32.Parse(pw.GetDeviceID());
        Debug.Log("Client ID (ClientScene): " + deviceID);
        connectionStatus = GameObject.FindGameObjectWithTag("ConnectionStatus").GetComponent<Text>();

        xCoord = GameObject.FindGameObjectWithTag("xCoord").GetComponent<Text>();
        yCoord = GameObject.FindGameObjectWithTag("yCoord").GetComponent<Text>();
        zCoord = GameObject.FindGameObjectWithTag("zCoord").GetComponent<Text>();
        angle = GameObject.FindGameObjectWithTag("Angle").GetComponent<Text>();
        factor = GameObject.FindGameObjectWithTag("Factor").GetComponent<Text>();
        tableAngle = GameObject.FindGameObjectWithTag("Table").GetComponent<Text>();

        upButt = GameObject.Find("upButt").GetComponent<Button>();
        downButt = GameObject.Find("downButt").GetComponent<Button>();
        rightButt = GameObject.Find("rightButt").GetComponent<Button>();
        leftButt = GameObject.Find("leftButt").GetComponent<Button>();
        scaleUp = GameObject.Find("scaleUp").GetComponent<Button>();
        scaleDown = GameObject.Find("scaleDown").GetComponent<Button>();
        factorUp = GameObject.Find("factorUp").GetComponent<Button>();
        factorDown = GameObject.Find("factorDown").GetComponent<Button>();
        showHide = GameObject.Find("showHide").GetComponent<Button>();
        swapLayer = GameObject.Find("swapLayer").GetComponent<Button>();
        mainCam = GameObject.Find("Main Camera");

        upButt.onClick.AddListener(upClicked);
        downButt.onClick.AddListener(downClicked);
        rightButt.onClick.AddListener(rightClicked);
        leftButt.onClick.AddListener(leftClicked);
        scaleUp.onClick.AddListener(scaleUpClicked);
        scaleDown.onClick.AddListener(scaleDownClicked);
        factorUp.onClick.AddListener(upFactorClicked);
        factorDown.onClick.AddListener(downFactorClicked);
        showHide.onClick.AddListener(showHideClicked);
        swapLayer.onClick.AddListener(swapLayerClicked);

        handSkin = GameObject.Find("Skin");
        handSkeleton = GameObject.Find("Skeleton");
        handNervous = GameObject.Find("Nervous");
        handMuscles = GameObject.Find("Muscles");
        handCirculatory = GameObject.Find("Circulatory");

        currentModel = handSkin;

        try {
            Debug.Log("Connecting to " + serverIP);
            connectionStatusString = "Connecting to " + serverIP;
            connectionStatus.text = connectionStatusString;
            TcpClient client = new TcpClient(serverIP, 8081);
            reader = new StreamReader(client.GetStream());
            writer = new StreamWriter(client.GetStream());
            serverConnectionIsAlive = true;
            Debug.Log("Connected to " + serverIP + "!");
            connectionStatusString = "Connected to " + serverIP;
            connectionStatus.text = connectionStatusString;
            networkThread = new Thread(new ThreadStart(readFromServer));
            networkThread.Start();
        } catch (Exception e) {
            Console.WriteLine(e);
        }
        // Debug.Log("AutoSwitch? " + autoSwitch + ", counter: " + autoSwitchCounter);

    }

    void upFactorClicked(){
        factorVal += 0.25F;
    }
    void downFactorClicked(){
        if(!(factorVal==1)){
        factorVal -= 0.25F;
        }
    }
    void upClicked() {
        heightVal += 15;
    }
    void downClicked() {
        heightVal -= 15;
    }
    void rightClicked() {
        widthVal -= 15;
    }
    void leftClicked() {
        widthVal += 15;
    }
    void scaleUpClicked() {
        scaleVal += 4;
        scaleVector.x = scaleVal;
        scaleVector.y = scaleVal;
        scaleVector.z = scaleVal;
    }
    void scaleDownClicked() {
        scaleVal -= 4;
        scaleVector.x = scaleVal;
        scaleVector.y = scaleVal;
        scaleVector.z = scaleVal;
    }
    void showHideClicked() {
        if (hide == false) {
            hide = true;
        } else {
            hide = false;
        }
    }
    void swapLayerClicked() {
        if (autoSwitch == true) {
            autoSwitch = false;
        } else {
            if (autoSwitchCounter < 4) {
                autoSwitchCounter++;
            } else {
                autoSwitchCounter = 0;
                autoSwitch = true;
            }
        }
        Debug.Log("AutoSwitch? " + autoSwitch + ", counter: " + autoSwitchCounter);
    }

    void Update() {
        
        if (serverConnectionIsAlive) {
            if (hide == true) {
                xCoord.text = "";
                yCoord.text = "";
                zCoord.text = "";
                angle.text = "";
                factor.text = "";
                tableAngle.text = "";
                connectionStatus.text = "";
                upButt.transform.position = new Vector3(-2000, 0, 0);
                downButt.transform.position = new Vector3(-2000, 0, 0);
                leftButt.transform.position = new Vector3(-2000, 0, 0);
                rightButt.transform.position = new Vector3(-2000, 0, 0);
                scaleDown.transform.position = new Vector3(-2000, 0, 0);
                scaleUp.transform.position = new Vector3(-2000, 0, 0);
                factorUp.transform.position = new Vector3(-2000, 0, 0);
                factorDown.transform.position = new Vector3(-2000, 0, 0);

            } else {
                xCoord.text = "X: " + xCoordString;
                yCoord.text = "Y: " + yCoordString;
                zCoord.text = "Z: " + zCoordString;
                angle.text = "Angle: " + angleString;
                factor.text = "Factor: " + factorVal;
                tableAngle.text = "Table: " + tableAngleString;
                connectionStatus.text = connectionStatusString;
                float ScreenWidthInch = Screen.width / Screen.dpi;
                float ScreenHeightInch = Screen.height / Screen.dpi;

                upButt.transform.position = new Vector3(470, 300, 0);
                downButt.transform.position = new Vector3(600, 300, 0);
                leftButt.transform.position = new Vector3(535, 366, 0);
                rightButt.transform.position = new Vector3(535, 300, 0);
                scaleDown.transform.position = new Vector3(482, 240, 0);
                scaleUp.transform.position = new Vector3(587, 240, 0);
                factorDown.transform.position = new Vector3(482, 190, 0);
                factorUp.transform.position = new Vector3(587, 190, 0);
            }
            coordinates.x = float.Parse(xCoordString)*factorVal;
            coordinates.y = (float.Parse(yCoordString) * -1)*factorVal + 500;
            coordinates.z = float.Parse(zCoordString);

            print("x");
            print(coordinates.x);

            angleFloat = float.Parse(angleString);
            tableAngleFloat = float.Parse(tableAngleString);
            layerOrder = (int)(coordinates.z);

            // Debug.Log(heightVal);
            // translateModel(coordinates, layerOrder, angleFloat, tableAngleFloat);
            coordinates = getVector3("(" + xCoordString + ", " + yCoordString + ", " + zCoordString + ")");
            Debug.Log(coordinates);
        }
        float vertical = Input.GetAxis("Vertical");
		float horizontal = Input.GetAxis("Horizontal");
        // mainCam.transform.eulerAngles = new Vector3( (vertical/1)*90, (horizontal/1)*90, 0);
        mainCam.transform.eulerAngles = new Vector3( (Input.acceleration.y*0.6f)/1*90 , (Input.acceleration.x*0.6f)/1*90 , 0 ) ;

    }

    public void translateModel(Vector3 coordinates, int layerOrder, double angle, double tableAngle) {
        if (autoSwitch == false) {
            layerOrder = autoSwitchCounter;
        }
        switch (layerOrder) {
            case 3:
                currentModel = handSkin;
                handSkin.SetActive(true);
                handSkeleton.SetActive(false);
                handNervous.SetActive(false);
                handMuscles.SetActive(false);
                handCirculatory.SetActive(false);
                if (autoSwitch == false) {
                    swapLayer.GetComponentInChildren<Text>().text = "Skin";
                } else {
                    swapLayer.GetComponentInChildren<Text>().text = "Auto";
                }
                break;
            case 0:
                currentModel = handSkeleton;
                handSkin.SetActive(false);
                handSkeleton.SetActive(true);
                handNervous.SetActive(false);
                handMuscles.SetActive(false);
                handCirculatory.SetActive(false);
                if (autoSwitch == false) {
                    swapLayer.GetComponentInChildren<Text>().text = "Skeleton";
                } else {
                    swapLayer.GetComponentInChildren<Text>().text = "Auto";
                }
                break;
            case 4:
                currentModel = handNervous;
                handSkin.SetActive(false);
                handSkeleton.SetActive(false);
                handNervous.SetActive(true);
                handMuscles.SetActive(false);
                handCirculatory.SetActive(false);
                if (autoSwitch == false) {
                    swapLayer.GetComponentInChildren<Text>().text = "Nervous";
                } else {
                    swapLayer.GetComponentInChildren<Text>().text = "Auto";
                }
                break;
            case 1:
                currentModel = handMuscles;
                handSkin.SetActive(false);
                handSkeleton.SetActive(false);
                handNervous.SetActive(false);
                handMuscles.SetActive(true);
                handCirculatory.SetActive(false);
                if (autoSwitch == false) {
                    swapLayer.GetComponentInChildren<Text>().text = "Muscles";
                } else {
                    swapLayer.GetComponentInChildren<Text>().text = "Auto";
                }
                break;
            case 2:
                currentModel = handCirculatory;
                handSkin.SetActive(false);
                handSkeleton.SetActive(false);
                handNervous.SetActive(false);
                handMuscles.SetActive(false);
                handCirculatory.SetActive(true);
                if (autoSwitch == false) {
                    swapLayer.GetComponentInChildren<Text>().text = "Circulatory";
                } else {
                    swapLayer.GetComponentInChildren<Text>().text = "Auto";
                }
                break;
            default:
                break;
        }
        modelOnscreenLocation = coordinates;
        modelOnscreenLocation.y += widthVal;
        modelOnscreenLocation.x += heightVal;
        modelOnscreenLocation.z = 300 + scaleVal;
        // currentModel.transform.position = modelOnscreenLocation;
        //currentModel.transform.localScale = scaleVector;
        float newX = modelOnscreenLocation.y;
        float newY = modelOnscreenLocation.x;
        modelOnscreenLocation.x = newX;
        modelOnscreenLocation.y = newY;
        mainCam.transform.position = modelOnscreenLocation;

        // Debug.Log(modelOnscreenLocation);
        // Debug.Log(new Vector3(heightVal,0,0));
        // currentModel.transform.position = new Vector3(heightVal,0,0);
        // currentModel.transform.eulerAngles = new Vector3(0,(float)angle,0);
        // currentModel.transform.Rotate(0,0,5);

        mainCam.transform.rotation = Quaternion.Euler(0, 0, ((float) angle) * -1 + 180);
        currentModel.transform.rotation = Quaternion.Euler(0, ((float) tableAngle) * -1 + 180, 0);
        // currentModel.transform.eulerAngles = new Vector3(0,(float)tableAngle,0);
    }

    private void readFromServer() {
        writer.WriteLine(deviceID);
        writer.Flush();
        while (serverConnectionIsAlive) {
            xCoordString = reader.ReadLine();
            yCoordString = reader.ReadLine();
            zCoordString = reader.ReadLine();
            angleString = reader.ReadLine();
            tableAngleString = reader.ReadLine();
        }
    }

    void OnApplicationQuit() {
        if (networkThread != null) {
            serverConnectionIsAlive = false;
            networkThread.Join(0);
            // Debug.LogError(networkThread.IsAlive);
        }
    }

    public Vector3 getVector3(string rString) {
        string[] temp = rString.Substring(1, rString.Length - 2).Split(',');
        float x = float.Parse(temp[0]);
        float y = float.Parse(temp[1]);
        float z = float.Parse(temp[2]);
        Vector3 rValue = new Vector3(x, y, z);
        return rValue;
    }
}