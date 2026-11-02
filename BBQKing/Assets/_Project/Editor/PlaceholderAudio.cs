using System.IO;
using UnityEditor;
using UnityEngine;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// Makes placeholder sound effects in code and saves them as WAV files under Audio/Placeholder,
    /// like <see cref="PlaceholderArt"/> does for images. Each file is named after an item in the
    /// design.md section 8 audio list, so it can be replaced one for one: overwrite the WAV with a
    /// real recording and everything that uses it plays the new sound. Existing files are never
    /// overwritten.
    ///
    /// The chimes are simple bell tones: a few sine waves at the note and its overtones, each
    /// fading away at its own speed.
    /// </summary>
    public static class PlaceholderAudio
    {
        const string Root = "Assets/_Project/Audio/Placeholder/";

        /// <summary>One bell note, for finishing a section of a clean.</summary>
        public const string CleanChime = Root + "clean_chime.wav";

        /// <summary>Three rising bell notes, for finishing a whole object.</summary>
        public const string CleanChimeTriple = Root + "clean_chime_triple.wav";

        const int SampleRate = 44100;

        // Notes of a C major chord, in equal temperament tuned to A = 440 Hz.
        const float C6 = 1046.50f, E6 = 1318.51f, G6 = 1567.98f;

        // A bell's overtones are not whole multiples of the note, which is what makes it sound
        // like a bell rather than an organ. Higher overtones are quieter and fade faster.
        static readonly float[] OvertoneRatios = { 1f, 2.76f, 5.4f };
        static readonly float[] OvertoneVolumes = { 1f, 0.4f, 0.2f };
        static readonly float[] OvertoneFades = { 3f, 5f, 8f };     // how fast each dies away, per second

        /// <summary>Writes every missing placeholder sound.</summary>
        public static void GenerateAll()
        {
            SaveIfMissing(CleanChime, Bells(1.2f, (C6, 0f)));
            SaveIfMissing(CleanChimeTriple, Bells(1.6f, (C6, 0f), (E6, 0.15f), (G6, 0.3f)));
        }

        /// <summary>Bell notes, each starting at its own time (in seconds), mixed into one sound.</summary>
        static float[] Bells(float seconds, params (float frequency, float start)[] notes)
        {
            var samples = new float[Mathf.CeilToInt(seconds * SampleRate)];
            foreach (var note in notes)
            {
                int first = Mathf.RoundToInt(note.start * SampleRate);
                for (int i = first; i < samples.Length; i++)
                {
                    float t = (i - first) / (float)SampleRate;
                    float attack = Mathf.Clamp01(t / 0.005f); // a 5 ms fade in avoids a click
                    float value = 0f;
                    for (int o = 0; o < OvertoneRatios.Length; o++)
                        value += OvertoneVolumes[o] * Mathf.Exp(-OvertoneFades[o] * t)
                                 * Mathf.Sin(2f * Mathf.PI * note.frequency * OvertoneRatios[o] * t);
                    samples[i] += attack * value;
                }
            }

            // Scale so the loudest moment sits at half of full volume, leaving room to mix.
            float peak = 0f;
            foreach (float s in samples) peak = Mathf.Max(peak, Mathf.Abs(s));
            if (peak > 0f) for (int i = 0; i < samples.Length; i++) samples[i] *= 0.5f / peak;
            return samples;
        }

        /// <summary>Saves a 16-bit mono WAV file, only if it does not exist yet.</summary>
        static void SaveIfMissing(string assetPath, float[] samples)
        {
            if (File.Exists(assetPath)) return;
            PlaceholderAssetWriter.EnsureFolder(assetPath);

            using (var writer = new BinaryWriter(File.Create(assetPath)))
            {
                int dataBytes = samples.Length * 2;
                // The standard WAV header: the file's layout, then the sound's format.
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataBytes);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);               // size of the format block
                writer.Write((short)1);         // plain uncompressed samples
                writer.Write((short)1);         // mono
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);   // bytes per second
                writer.Write((short)2);         // bytes per sample
                writer.Write((short)16);        // bits per sample
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(dataBytes);
                foreach (float s in samples)
                    writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
            }
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
