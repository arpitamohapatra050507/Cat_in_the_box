using System;
using UnityEngine;

namespace LastPassenger
{
    [Serializable]
    public sealed class AnomalyCheckpointCollection
    {
        public AnomalyCheckpointDefinition[] checkpoints;
    }

    [Serializable]
    public sealed class AnomalyCheckpointDefinition
    {
        public string id;
        [Range(0.05f, 0.95f)] public float progress;
        [TextArea] public string message;
        public Color messageColor = Color.white;
        public float displaySeconds = 4f;
        public string audioResource;
        [Range(0f, 1f)] public float audioVolume = 0.2f;
        public float apparitionCheckSeconds = 15f;
        [Range(0f, 1f)] public float apparitionChance = 0.5f;
        public float roadFigureMinimumSeconds = 105f;
        public float roadFigureMaximumSeconds = 135f;
        public string action = "none";
    }
}
