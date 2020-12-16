using System;
using UnityEngine;
using System.Text;

public class FPSCounter : MonoBehaviour {
	const float fpsMeasurePeriod = 0.5f;
	private int m_FpsAccumulator = 0;
	private float m_FpsNextPeriod = 0;
	private int m_CurrentFps;
	const string display = "{0} FPS";
	private string m_text;
	private GUIStyle m_style;

	//存储临时字符串
	private StringBuilder info = new StringBuilder(string.Empty);


	private void Start() {

		m_style = new GUIStyle ();
		m_style.normal.background = null;
		m_style.normal.textColor = Color.red;
		m_style.fontSize = 30;
		m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
	}

	private Rect m_FpsRect = new Rect(Screen.width - 400, 0, 200, 30);
    private Rect m_VerRect = new Rect(350, 0, 200, 50);

    void OnGUI(){
        GUI.Label(m_FpsRect, m_text, m_style);
    }


	void GetMessage(params string[] str) {
		if (str.Length == 2) {
			info.AppendLine(str[0] + ":" + str[1]);
		}
	}

	private void Update() {
		//measure average frames per second
		m_FpsAccumulator++;
		if (Time.realtimeSinceStartup > m_FpsNextPeriod) {
			m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
			m_FpsAccumulator = 0;
			m_FpsNextPeriod += fpsMeasurePeriod;
			m_text = "FPS:" + m_CurrentFps;
		}
    }
}
