using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPS_counter : MonoBehaviour
{

	public Text outputTextBox;

	//private CircularBufferFloat avg;


	// Use this for initialization
	void Start()
	{
		//avg = new CircularBufferFloat(1);
	}

	//// Update is called once per frame
	//void Update () 
	//{
	//	float lastTime = Time.deltaTime;
	//	float FPS = 1 / lastTime;

	//	avg.AddSample(FPS);

	//	outputTextBox.text = "FPS: " + avg.GetAvg();
	//}

	int m_frameCounter = 0; 
	float m_timeCounter = 0.0f;
	float m_lastFramerate = 0.0f;
	public float m_refreshTime = 0.5f;

	void Update()
	{
		if (m_timeCounter < m_refreshTime)
		{
			m_timeCounter += Time.deltaTime;
			m_frameCounter++;
		}
		else
		{ //This code will break if you set your m_refreshTime to 0, which makes no sense.
			m_lastFramerate = (float)m_frameCounter / m_timeCounter;
			m_frameCounter = 0;
			m_timeCounter = 0.0f;

			outputTextBox.text = "FPS: " + m_lastFramerate;
		}
	}

}
