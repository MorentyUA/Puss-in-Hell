using System.Collections.Generic;
using UnityEngine;

namespace PussInHell.Interaction
{
    public class OutlineHighlight : MonoBehaviour
    {
        [SerializeField] private Interactable interactable;
        [SerializeField] private Material outlineMaterial;
        [Tooltip("Objects with a Renderer that receive the outline")]
        [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();

        private readonly List<Renderer> renderers = new List<Renderer>();
        private readonly List<Material[]> originalMaterials = new List<Material[]>();

        private void Awake()
        {
            if (interactable == null) interactable = GetComponent<Interactable>();

            foreach (var target in targetObjects)
            {
                if (target == null) continue;
                var renderer = target.GetComponent<Renderer>();
                if (renderer == null) continue;
                renderers.Add(renderer);
                originalMaterials.Add(renderer.sharedMaterials);
            }
        }

        private void OnEnable()
        {
            if (interactable == null) return;
            interactable.onRangeEntered.AddListener(Show);
            interactable.onRangeExited.AddListener(Hide);
        }

        private void OnDisable()
        {
            if (interactable == null) return;
            interactable.onRangeEntered.RemoveListener(Show);
            interactable.onRangeExited.RemoveListener(Hide);
            Hide();
        }

        public void Show()
        {
            if (outlineMaterial == null) return;

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;
                var materials = renderer.sharedMaterials;
                if (System.Array.IndexOf(materials, outlineMaterial) >= 0) continue;

                var extended = new Material[materials.Length + 1];
                materials.CopyTo(extended, 0);
                extended[materials.Length] = outlineMaterial;
                renderer.sharedMaterials = extended;
            }
        }

        public void Hide()
        {
            for (int i = 0; i < renderers.Count && i < originalMaterials.Count; i++)
                if (renderers[i] != null) renderers[i].sharedMaterials = originalMaterials[i];
        }
    }
}
