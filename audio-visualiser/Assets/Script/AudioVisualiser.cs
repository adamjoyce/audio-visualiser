using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualiser : MonoBehaviour
{
    public float rmsRefValue = 0.1f;            // The RMS value for 0dB.
    public float rmsValue;                      // The audio's RMS (Root Mean Squared) value this frame.
    public float dbValue;                       // The audio's decibel value this frame.

    private AudioSource audioSource;            // The audio source playing the music that will be visualised.
    private const int SampleSize = 1024;        // The sample size for the audio.
    private float[] samples;                    // Stores the audio's sample data for the frame.
    private float[] spectrum;                   // Stores the audio's spectrum data for the frame.
    private float sampleRate;                   // The audio mixer's current sample rate.

    /* Use this for initialization. */
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        samples = new float[SampleSize];
        spectrum = new float[SampleSize];
        sampleRate = AudioSettings.outputSampleRate;
    }

    /* Update is called once per frame. */
    private void Update()
    {
        AnalyseAudio();
    }

    /* Calculates the RMS and dB audio values. */
    private void AnalyseAudio()
    {
        // Fill our sample array.
        audioSource.GetOutputData(samples, 0);

        // Calculate the RMS value.
        float sum = 0.0f;
        for (int i = 0; i < SampleSize; ++i)
        {
            sum += samples[i] * samples[i];
        }
        rmsValue = Mathf.Sqrt(sum / SampleSize);

        // Calculate the dB value.
        dbValue = 20 * Mathf.Log10(rmsValue / rmsRefValue);

        // Fill our audio spectrum.
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
    }
}