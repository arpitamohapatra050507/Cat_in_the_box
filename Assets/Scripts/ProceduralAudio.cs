using UnityEngine;

namespace LastPassenger
{
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;

        public static AudioClip EngineLoop()
        {
            const float seconds = 2f;
            int samples = Mathf.RoundToInt(SampleRate * seconds);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float pulse = Mathf.Sin(t * Mathf.PI * 2f * 42f);
                float harmonic = Mathf.Sin(t * Mathf.PI * 2f * 84f) * 0.35f;
                float rumble = Mathf.Sin(t * Mathf.PI * 2f * 21f) * 0.22f;
                data[i] = (pulse + harmonic + rumble) * 0.16f;
            }

            return BuildClip("Generated engine loop", data, true);
        }

        public static AudioClip WindLoop()
        {
            const float seconds = 4f;
            int samples = Mathf.RoundToInt(SampleRate * seconds);
            float[] data = new float[samples];
            uint state = 0xC0FFEEu;
            float smooth = 0f;

            for (int i = 0; i < samples; i++)
            {
                state = state * 1664525u + 1013904223u;
                float noise = ((state >> 8) / 16777215f) * 2f - 1f;
                smooth = Mathf.Lerp(smooth, noise, 0.025f);
                float breath = 0.55f + Mathf.Sin(i / (float)SampleRate * Mathf.PI * 0.7f) * 0.2f;
                data[i] = smooth * breath * 0.12f;
            }

            return BuildClip("Generated night wind", data, true);
        }

        public static AudioClip RadioStatic()
        {
            const float seconds = 3f;
            int samples = Mathf.RoundToInt(SampleRate * seconds);
            float[] data = new float[samples];
            uint state = 0xBAD5EEDu;

            for (int i = 0; i < samples; i++)
            {
                state = state * 1103515245u + 12345u;
                float noise = ((state >> 9) / 8388607f) * 2f - 1f;
                float crackle = (state & 0x1FFu) == 0 ? 0.8f : 0f;
                data[i] = noise * 0.055f + crackle;
            }

            return BuildClip("Generated radio static", data, true);
        }

        public static AudioClip Impact()
        {
            const float seconds = 0.35f;
            int samples = Mathf.RoundToInt(SampleRate * seconds);
            float[] data = new float[samples];
            uint state = 0x1234567u;

            for (int i = 0; i < samples; i++)
            {
                state = state * 1664525u + 1013904223u;
                float noise = ((state >> 8) / 16777215f) * 2f - 1f;
                float t = i / (float)samples;
                float envelope = Mathf.Pow(1f - t, 4f);
                data[i] = (noise * 0.45f + Mathf.Sin(i * 0.18f) * 0.3f) * envelope;
            }

            return BuildClip("Generated roadside impact", data, false);
        }

        public static AudioClip HorrorSting()
        {
            const float seconds = 2.5f;
            int samples = Mathf.RoundToInt(SampleRate * seconds);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float normalized = i / (float)samples;
                float rising = 75f + normalized * normalized * 520f;
                float tone = Mathf.Sin(t * Mathf.PI * 2f * rising);
                float dissonance = Mathf.Sin(t * Mathf.PI * 2f * (rising * 1.061f)) * 0.65f;
                float envelope = Mathf.Sin(Mathf.Clamp01(normalized) * Mathf.PI);
                data[i] = (tone + dissonance) * envelope * 0.22f;
            }

            return BuildClip("Generated anomaly sting", data, false);
        }

        public static AudioClip TruckHorn()
        {
            const float seconds = 1.65f;
            int samples = Mathf.RoundToInt(SampleRate * seconds);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float normalized = i / (float)samples;
                float attack = Mathf.Clamp01(normalized / 0.045f);
                float release = Mathf.Clamp01((1f - normalized) / 0.14f);
                float envelope = Mathf.Min(attack, release);
                float wobble = Mathf.Sin(t * Mathf.PI * 2f * 2.1f) * 1.8f;
                float low = Mathf.Sin(t * Mathf.PI * 2f * (92f + wobble));
                float second = Mathf.Sin(t * Mathf.PI * 2f * (116f + wobble * 0.6f)) * 0.72f;
                float grit = Mathf.Sin(t * Mathf.PI * 2f * 232f) * 0.18f;
                data[i] = (low + second + grit) * envelope * 0.24f;
            }

            return BuildClip("Generated distant truck horn", data, false);
        }

        private static AudioClip BuildClip(string name, float[] samples, bool loop)
        {
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
