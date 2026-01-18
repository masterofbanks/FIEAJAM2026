using UnityEngine;

public class AmazongBehavior : MonoBehaviour
{
    public RectTransform[] corners;
    public GameObject XButton;
    public int numClicksNeeded = 4;
    private int numClicks;
    public PhoneManager pHScript;

    private System.Random rnd;

    private void Awake()
    {
        rnd = new System.Random();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        numClicks = 0;
        int randIndex = rnd.Next(0, corners.Length);
        XButton.transform.position = corners[randIndex].position;
    }


    private void OnEnable()
    {
        numClicks = 0;
        int randIndex = rnd.Next(0, corners.Length);
        XButton.transform.position = corners[randIndex].position;
    }

    public void ClickedX()
    {
        numClicks++;
        if(numClicks >= numClicksNeeded)
        {
            pHScript.ReturnToHomeScreen(gameObject);
        }
        else
        {
            int randIndex = rnd.Next(0, corners.Length);
            XButton.transform.position = corners[randIndex].position;
        }
    }
}
