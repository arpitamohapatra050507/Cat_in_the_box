using System;
using UnityEngine;

namespace LastPassenger
{
    [Serializable]
    public sealed class RoadEventCollection
    {
        public RoadEventDefinition[] events;
    }

    [Serializable]
    public sealed class RoadEventDefinition
    {
        public string id;
        public float triggerDistance;
        [TextArea] public string message;
        public Color messageColor = Color.white;
        public float displaySeconds = 5f;

        public RoadEventDefinition(string eventId, float distance, string eventMessage, Color color, float seconds = 5f)
        {
            id = eventId;
            triggerDistance = distance;
            message = eventMessage;
            messageColor = color;
            displaySeconds = seconds;
        }
    }
}
