using System;
using UnityEngine;

namespace Aki.Scripts.UI
{
    public class VoiceInputs
    {
        public AudioClip recording;

        public int m_RecordingLength = 10;
        internal void StartRecordAudio()
        {
            recording = Microphone.Start(null, false, m_RecordingLength, 16000);
        }

        internal void StopRecordAudio(Action<AudioClip> _callback)
        {
            Microphone.End(null);
            if (recording != null)
            {
                _callback?.Invoke(recording);
            }
        }
    }
}