using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// a box, like <see cref="HUD"/>, that displays information to the user.
/// In this case, it continuously calculates and displays the real-time framerate. It can be enabled/disabled in 
/// <see cref="VRNChairSettings"/>.
/// </summary>
public class FPS_counter : MonoBehaviour
{
	/// <summary>
	/// where to draw text
	/// </summary>
	public Text outputTextBox;

	int m_frameCounter = 0; 
	float m_timeCounter = 0.0f;
	float m_lastFramerate = 0.0f;

	/// <summary>
	/// how frequently to calculate the framerate
	/// </summary>
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
