using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio;

namespace SpeechForm.Business.Attendance
{
    public class CaptureLoopBack
    {
        public void startLoopBack()
        {
            var enumerator = new MMDeviceEnumerator();
            var end = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)[1];
            var capture = new WasapiLoopbackCapture();

            var waveOut = new NAudio.Wave.WaveOut();

            waveOut.DeviceNumber = 1;
            var bufferedWaveProvider = new BufferedWaveProvider(end.AudioClient.MixFormat);

            capture.DataAvailable += (s, a) =>
            {
                try
                {
                    bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);
                    if (waveOut.PlaybackState != PlaybackState.Playing)
                    {
                        waveOut.Init(bufferedWaveProvider);
                        waveOut.Play();
                    }
                }
                catch { }
            };

            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
            };

            capture.StartRecording();
        }
    }
}
