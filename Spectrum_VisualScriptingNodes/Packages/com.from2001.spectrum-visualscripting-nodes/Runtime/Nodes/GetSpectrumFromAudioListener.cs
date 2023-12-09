using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Linq;

namespace spectrumvisualscriptingnodes
{
    [UnitTitle("Get audio spectrum from AudioListener")]
    [UnitShortTitle("Spectrum from AudioListener")]
    [UnitCategory("Audio/Spectrum")]
    [UnitSubtitle("Get audio spectrum from AudioListener")]
    public class GetSpectrumFromAudioListener : Unit
    {
        [DoNotSerialize]
        public ControlInput inputTrigger;

        [DoNotSerialize]
        public ControlOutput outputTrigger;

        [DoNotSerialize]
        [NullMeansSelf]
        public ValueInput AudioListener;
        [DoNotSerialize]
        public ValueInput channel;
        [DoNotSerialize]
        public ValueInput window;
        [DoNotSerialize]
        public ValueInput spectrumIndex;

        [DoNotSerialize]
        public ValueOutput result_SamplesList;

        float[] spectrum;
        protected override void Definition()
        {
            inputTrigger = ControlInput("inputTrigger", Enter);
            outputTrigger = ControlOutput("outputTrigger");

            AudioListener = ValueInput<AudioListener>("AudioListener", null).NullMeansSelf();
            channel = ValueInput<int>("channel", 0);
            window = ValueInput<FFTWindow>("window", FFTWindow.Rectangular);
            spectrumIndex = ValueInput<int>("spectrumIndex", 64);

            result_SamplesList = ValueOutput<float[]>("result_SamplesList");
        }

        private ControlOutput Enter(Flow flow)
        {
            AudioListener AudioListener = flow.GetValue<AudioListener>(this.AudioListener).GetComponent<AudioListener>();
            int channel = flow.GetValue<int>(this.channel);
            FFTWindow window = flow.GetValue<FFTWindow>(this.window);
            int spectrumIndex = flow.GetValue<int>(this.spectrumIndex);
            
            if (!IsPowerOfTwo(spectrumIndex))
            {
                Debug.LogError("spectrumIndex must be a power of two");
                return outputTrigger;
            }
            if (spectrumIndex >= 64)
            {
                spectrum = new float[spectrumIndex];
                AudioListener.GetSpectrumData(spectrum, channel, window);
                flow.SetValue(result_SamplesList, spectrum);
            }
            else
            {   
                // if spectrumIndex is less than 64, we downsample the spectrum since GetSpectrumData doesn't allow to get a spectrum with less than 64 samples
                spectrum = new float[64];
                AudioListener.GetSpectrumData(spectrum, channel, window);
                int downSampleRate = 64 / spectrumIndex;
                float[] spectrum2 = Enumerable.Range(0, spectrumIndex)
                        .Select(i => spectrum.Skip(i * downSampleRate).Take(downSampleRate).Average())
                        .ToArray();
                flow.SetValue(result_SamplesList, spectrum2);
            }
            return outputTrigger;
        }

        private bool IsPowerOfTwo(int x)
        {
            return x > 0 && (x & (x - 1)) == 0;
        }
    }
}

