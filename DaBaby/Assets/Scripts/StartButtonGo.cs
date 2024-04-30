using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonGo : MonoBehaviour
{
    public void startGame()
    {
        SceneManager.LoadScene("Main");
    }
}
