using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnd : MonoBehaviour
{
    public float interval = 30;           // Interval to capture overall status
    public BabyStatus status;
    private int intervalCnt = 0;          // Keep count of how many times we have reached a 30 sec interval
    private float curTime = 0;            // Current time elapsed
    private bool isGameRunning = true;    // Flag to indicate if the game is running

    private List<float> statusList = new List<float>(); // List to store overall status values

    // Update is called once per frame
    void Update()
    {
        if (isGameRunning)
        {
            curTime += Time.deltaTime; //normal 1 second time passage

            if (intervalCnt == 8) //after 6 30 sec intervals
            {
                EndGame();
            }

            if(curTime >= interval)
            {
                CapStatus();
                curTime = 0; //reset
                intervalCnt++; //increment
            }
        }

        void EndGame()
        {
            isGameRunning = false;
            // Will need a scene transition to game over screen here 
            // below is a draft for final score calculation to see if you won or lost
            float finalScore = 0;

            for (int i = 0; i <= (intervalCnt - 1); i++)
            {
                finalScore += statusList[i];
            }

            if (finalScore >= 65)
            {
                SceneManager.LoadScene("GameEndWin");
                //code for you win screen
                Debug.Log("Game Over!");
            }
            else
            {
                SceneManager.LoadScene("GameOverLose");
                //code for you lose screen
                Debug.Log("Game Over!");
            }
        }

        void CapStatus()
        {
            //BabyStatus babyStatus = FindObjectOfType<BabyStatus>();
            if (status != null)
            {
                // Store the overall status value
                statusList.Add(status.overallStatus);
            }
        }
    }
}
