using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsWrapper : MonoBehaviour {
    public string IPKey = "IP";
    public string clientID = "ID";
    
    public void SetIPKey(string IP) {
        PlayerPrefs.SetString(IPKey, IP);
    }

    public void SetDeviceID(string ID) {
        PlayerPrefs.SetString(clientID, ID);
    }

    public string GetIPKey() {
        string IP = PlayerPrefs.GetString(IPKey);
        if (IP == "") {
            return "-1";
        } else {
            return IP;
        }
    }

    public string GetDeviceID(){
        string ID = PlayerPrefs.GetString(clientID);
        return ID;
    }

}