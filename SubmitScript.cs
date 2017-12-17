using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SubmitScript : MonoBehaviour {
    
	public Text textField;
	public Text textFieldClient;
	private PlayerPrefsWrapper pw;
	public void Start(){
		pw = GetComponent<PlayerPrefsWrapper>();
	}
    public void OnClick() {
		string IP = textField.text;
		pw.SetIPKey(IP);
		pw.SetDeviceID(textFieldClient.text);
		Debug.Log("Device ID (IPScene): " + textFieldClient.text);
		SceneManager.LoadScene("ClientScene");
    }
}