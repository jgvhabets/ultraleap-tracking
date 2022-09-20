using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityStandardAssets.Utility
{
    [RequireComponent(typeof (Text))]
    public class FPSCounter : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.2f;
        private float m_CurrentFps;
		private float m_avgFps;
		private int m_FpsAccumulator = 0;
		private float m_MaxFps;
		private float m_MinFps;
		private int m_droppedFrames;
		private Text m_Text;
		private List<float> fpsList = new List<float>();

		private float m_FpsNextPeriod = 0;

		private void Start()
        {
            m_Text = GetComponent<Text>();
			m_MaxFps = 0f;
			m_MinFps = 200f;
			m_droppedFrames = 0;
			m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
			}


        private void Update()
        {
			// measure average frames per second
			m_FpsAccumulator++;
			m_CurrentFps = 1f / Time.deltaTime;

			if (m_CurrentFps < m_MinFps) {
				m_MinFps = m_CurrentFps;
				}
			if (m_CurrentFps > m_MaxFps) {
				m_MaxFps = m_CurrentFps;
				}
			if (m_CurrentFps < 60f) {
				m_droppedFrames += 1;
				}

			if (Time.realtimeSinceStartup > m_FpsNextPeriod) {

				m_FpsAccumulator = 0;
				m_FpsNextPeriod += fpsMeasurePeriod;

				fpsList.Add(m_CurrentFps);

				m_avgFps = fpsList.Sum() / fpsList.Count;

				m_Text.text = "Current: " + m_CurrentFps.ToString("000") + " | " + "Min: " + m_MinFps.ToString("000") + " | " + "Max: " + m_MaxFps.ToString("000") + " | " + "Dropped frames (below 60): " + m_droppedFrames.ToString("0") + " | " + "Average: " + m_avgFps.ToString("000");
				}

			// reset on R
			if (Input.GetKeyDown(KeyCode.R)) {
				m_MaxFps = 0f;
				m_MinFps = 200f;
				m_droppedFrames = 0;
				}
			}
		}
}
