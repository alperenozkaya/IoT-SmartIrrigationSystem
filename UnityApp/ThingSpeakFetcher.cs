using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class ThingSpeakFetcher : MonoBehaviour
{
    
    private const string apiUrl = "https://api.thingspeak.com/channels/CHANNEL_ID/fields/FIELD_INDEX.json?api_key=YOUR_API_KEY&results=150"; // ThingSpeak API URL
    private const string apiKey = "YUYIFECX4EZ35UNR"; // ThingSpeak API Key
    private const int desiredValueCount = 20; // Number of values to store in dataValuesTemp

    public List<float> dataValuesTemp = new List<float>(); 

    // calls FetchDataFromThingSpeak() coroutine because I don't like invoking coroutines from other scripts, I have principles...
    public void FetchData(int fieldIndex)
    {
        StartCoroutine(FetchDataFromThingSpeak(fieldIndex));
    }

    /// <summary>
    /// Fetches data from ThingSpeak and stores it in dataValuesTemp
    /// </summary>
    /// <param name="fieldIndex"></param>
    /// <returns></returns>
    private IEnumerator FetchDataFromThingSpeak(int fieldIndex)
    {
        // replace placeholders in apiUrl with actual values
        string url = apiUrl.Replace("CHANNEL_ID", "2183676").Replace("FIELD_INDEX", fieldIndex.ToString()).Replace("YOUR_API_KEY", apiKey);

        // send request to ThingSpeak
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest(); // wait for response

            if (www.result == UnityWebRequest.Result.Success) // if response is successful parse JSON and store values in dataValuesTemp
            {
                string json = www.downloadHandler.text;
                var response = JsonUtility.FromJson<ThingSpeakResponse>(json);

                dataValuesTemp.Clear();
                foreach (var feed in response.feeds)
                {
                    string value = null;
                    switch (fieldIndex)
                    {
                        case 1:
                            value = feed.field1;
                            break;
                        case 2:
                            value = feed.field2;
                            break;
                        case 3:
                            value = feed.field3;
                            break;
                        case 4:
                            value = feed.field4;
                            break;
                    }
                    if (value != null)
                    {
                        // try to parse value as float, if successful add it to dataValuesTemp
                        if(float.TryParse(value, out float result))
                        {
                            dataValuesTemp.Add(result);
                        }
                        else
                        {
                            if (value.Length > 0)
                                Debug.LogWarning($"Failed to parse '{value}' as float."); 

                            else 
                                Debug.Log($"A null value was found.");  // if value is null, log a message
                        }
                    }
                }

                while (dataValuesTemp.Count > desiredValueCount) // if dataValuesTemp contains more values than desiredValueCount, remove the oldest values
                {
                    dataValuesTemp.RemoveAt(0);
                }
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }
}

// classes for parsing JSON response from ThingSpeak
[Serializable]
public class ThingSpeakResponse
{
    public Feed[] feeds;
}

[Serializable]
public class Feed
{
    public string field1;
    public string field2;
    public string field3;
    public string field4;
}
