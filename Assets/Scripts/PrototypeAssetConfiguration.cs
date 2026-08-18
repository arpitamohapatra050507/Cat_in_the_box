using UnityEngine;

namespace LastPassenger
{
    [DisallowMultipleComponent]
    public sealed class PrototypeAssetConfiguration : MonoBehaviour
    {
        [Header("Optional prefab overrides")]
        [Tooltip("A complete repeating road chunk whose root begins at local Z = 0 and extends forward along +Z. Leave empty for the generated road.")]
        [SerializeField] private GameObject roadChunkPrefab;

        [Tooltip("Used for the common roadside fir trees. Leave empty for generated pine trees.")]
        [SerializeField] private GameObject pineTreePrefab;

        [Tooltip("Used for the occasional bare roadside trees. Leave empty for generated leafless trees.")]
        [SerializeField] private GameObject leaflessTreePrefab;

        [Header("Road prefab dimensions")]
        [Tooltip("The forward length of one road prefab in Unity units. Keep this at 80 for the current generated road.")]
        [SerializeField, Min(10f)] private float roadChunkLength = 80f;

        public GameObject RoadChunkPrefab => roadChunkPrefab;
        public GameObject PineTreePrefab => pineTreePrefab;
        public GameObject LeaflessTreePrefab => leaflessTreePrefab;
        public float RoadChunkLength => Mathf.Max(10f, roadChunkLength);

#if UNITY_EDITOR
        public void ApplyEditorOverrides(
            GameObject roadPrefab,
            GameObject pinePrefab,
            GameObject leaflessPrefab,
            float chunkLength)
        {
            roadChunkPrefab = roadPrefab;
            pineTreePrefab = pinePrefab;
            leaflessTreePrefab = leaflessPrefab;
            roadChunkLength = Mathf.Max(10f, chunkLength);
        }
#endif
    }
}
