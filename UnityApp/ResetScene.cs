using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {   // Reset the scene after 60 seconds
        Invoke("ResetCurrentScene", 60f);
    }
    private void DebugReset(){
        Debug.LogWarning("Resetting scene");
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void ResetCurrentScene() {
        SceneManager.LoadScene(sceneBuildIndex: SceneManager.GetActiveScene().buildIndex);
    }

}
