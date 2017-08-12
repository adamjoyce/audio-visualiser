using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVisualiser : MonoBehaviour
{
    public GameObject visualObjectPrefab;       // The object used to represent the audio.
    public Color lowDBColor;                    // The low dB color.
    public Color highDBColor;                   // The high dB color.

    public float rmsRefValue = 0.1f;            // The RMS value for 0dB.
    public float rmsValue;                      // The audio's RMS (Root Mean Squared) value this frame.
    public float dbValue;                       // The audio's decibel value this frame.

    public int numOfVisualObjects = 10;         // How many visual objects to spawn.
    public float maxVisualScale = 25.0f;        // The maximum scale a visual object is allowed to reach.
    public float visualScaleModifer = 50.0f;    // The modifer for the scale of each visual object.
    public float smoothSpeed = 10.0f;           // How quickly the visual objects return to their default size.
    public float usablePercentage = 0.5f;       // The percentage of the visual object line that will be kept.

    public float circleRadius = 5.0f;           // The radius of the circle of visual objects.

    private AudioSource audioSource;            // The audio source playing the music that will be visualised.
    private const int SampleSize = 1024;        // The sample size for the audio.
    private float[] samples;                    // Stores the audio's sample data for the frame.
    private float[] spectrum;                   // Stores the audio's spectrum data for the frame.
    private float sampleRate;                   // The audio mixer's current sample rate.

    private Transform[] visualObjects;          // The list of visual objects.
    private float[] visualScale;                // The scale of each visual object.

    /* Use this for initialization. */
    private void Start()
    {
        if (!visualObjectPrefab) { Debug.LogWarning("Missing Visual Object Prefab!"); }

        audioSource = GetComponent<AudioSource>();
        samples = new float[SampleSize];
        spectrum = new float[SampleSize];
        sampleRate = AudioSettings.outputSampleRate;

        // Camera position adjusts with the number of objects.
        //Camera.main.transform.position = new Vector3(numOfVisualObjects / 2, 0, (-numOfVisualObjects / 2) - 1);

        //SpawnLine();
        SpawnCircle();
    }

    /* Update is called once per frame. */
    private void Update()
    {
        AnalyseAudio();
        UpdateVisuals();
    }

    /* Spawns a line of visual objects to display the audio. */
    private void SpawnLine()
    {
        visualObjects = new Transform[numOfVisualObjects];
        visualScale = new float[numOfVisualObjects];

        // Spawn the initial line.
        for (int i = 0; i < numOfVisualObjects; ++i)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            visualObjects[i] = obj.transform;
            visualObjects[i].position = Vector3.right * i;
        }
    }

    /* Spawns a circle of visual objects to display the audio. */
    private void SpawnCircle()
    {
        visualObjects = new Transform[numOfVisualObjects];
        visualScale = new float[numOfVisualObjects];

        // Spawn the circle.
        for (int i = 0; i < numOfVisualObjects; ++i)
        {
            float angle = i * ((Mathf.PI * 2) / numOfVisualObjects);
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circleRadius;

            if (visualObjectPrefab)
            {
                GameObject obj = Instantiate(visualObjectPrefab, pos, Quaternion.identity);
                visualObjects[i] = obj.transform;
            }
        }
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

    /* Updates the scale of the visual objects based on the audio samples. */
    private void UpdateVisuals()
    {
        int visualIndex = 0;
        int spectrumIndex = 0;

        // The number of spectrum samples for each visual object.
        int averageSize = (int)(SampleSize * usablePercentage / numOfVisualObjects);

        while (visualIndex < numOfVisualObjects)
        {
            int j = 0;
            float sum = 0;
            while (j < averageSize)
            {
                sum += spectrum[spectrumIndex];
                spectrumIndex++;
                j++;
            }

            float scaleY = sum / averageSize * visualScaleModifer;
            visualScale[visualIndex] -= smoothSpeed * Time.deltaTime;
            if (visualScale[visualIndex] < scaleY)
            {
                // Instantly snap back up to scaleY.
                visualScale[visualIndex] = scaleY;
            }

            // Clamp to the maximum scale.
            if (visualScale[visualIndex] > maxVisualScale)
                visualScale[visualIndex] = maxVisualScale;

            // Apply the new scale and adjust the object position so it appears to scale in one direction.
            visualObjects[visualIndex].localScale = Vector3.one + (Vector3.up * visualScale[visualIndex]);
            visualObjects[visualIndex].GetComponentInChildren<Renderer>().material.color = Color.Lerp(lowDBColor, highDBColor, (visualObjects[visualIndex].localScale.y / maxVisualScale));
            visualIndex++;
        }
    }
}