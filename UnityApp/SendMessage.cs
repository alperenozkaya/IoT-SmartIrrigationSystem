using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SendMessage : MonoBehaviour
{
    private string apiKey = "2508C4UZ1PX75OJT";
    private float value = 0f;
    private int messageField = 1;
    private const string apiUrl = "https://api.thingspeak.com/update?api_key=YOUR_API_KEY&fieldFIELD_TO_SEND=MESSAGE";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void SendMessageToThingSpeak(float val, int field)
    {
        value = val;
        messageField = field;

        // replace placeholders in apiUrl with actual values
        string url = apiUrl.Replace("MESSAGE", value.ToString()).Replace("FIELD_TO_SEND", messageField.ToString()).Replace("YOUR_API_KEY", apiKey);

        // send request to ThingSpeak
        UnityWebRequest request = UnityWebRequest.Get(url);

        StartCoroutine(SendRequest(request)); // wait for response
    }

    private IEnumerator SendRequest(UnityWebRequest request)
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Data sent to ThingSpeak successfully!");
        }
        else
        {
            Debug.LogError("Failed to send data to ThingSpeak: " + request.error);
            // if error occurs, wait 15 seconds and try again
            yield return new WaitForSeconds(15f);
            SendRequest(request);
        }

        

    }

    private void WaterPlant()
    {
        SendMessageToThingSpeak(1f, 3);
        
    }

    public void OnWaterPlant()
    {
        WaterPlant();
    }

    public void OnWifiTether()
    {
        // jump to the next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex: gameObject.scene.buildIndex + 1);
    }

    public void OnSetThreshold()
    {
        SendMessageToThingSpeak(50f, 2);
    }
}
