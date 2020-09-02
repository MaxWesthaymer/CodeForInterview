using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZombieShooter.Tools
{
    public class HouseHider : MonoBehaviour
    {
        private List<Material> houseMaterials;
        private List<Collider> playersColliders;
        private bool isRaycasting;
        private Material transparentMaterial;
        private new Renderer[] renderer;

        [SerializeField] private float speed = 0.7f;
        [SerializeField] private float alpha = 0.1f;

        private float targetAlpha;
        private bool isEditAlpha;
        private bool isAlpha;

        #region UnityMethods  

        private void Start()
        {
            houseMaterials = new List<Material>();
            playersColliders = new List<Collider>();
            renderer = transform.GetComponentsInChildren<Renderer>();

            foreach (var it in renderer)
            {
                houseMaterials.Add(it.material);
            }

            var str = GetComponent<Renderer>().material.name;
            transparentMaterial = Resources.Load<Material>($"Materials/{str.Remove(str.Length - (str.Length - 20))}_T");
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (!playersColliders.Contains(collider) && collider.gameObject.CompareTag("Player"))
            {
                playersColliders.Add(collider);
            }
        }

        private void OnTriggerExit(Collider collider)

        {
            if (playersColliders.Contains(collider))
            {
                playersColliders.Remove(collider);
            }
        }

        private void FixedUpdate()
        {
            if (playersColliders.Count > 0)
            {
                targetAlpha = alpha;
                StartTransparent();
                playersColliders.RemoveAll(item => item == null);
            }
            else
            {
                targetAlpha = 1;
                StartOpaque();
                playersColliders.Clear();
            }
        }

        #endregion

        #region PrivateMethods  

        private void SetMaterialTransparent()
        {
            foreach (Renderer m in renderer)
            {
                m.material = transparentMaterial;
            }
        }

        private void SetMaterialOpaque()
        {
            for (int i = 0; i < renderer.Length; i++)
            {
                renderer[i].material = houseMaterials[i];
            }
        }

        private void StartTransparent()
        {
            if (isEditAlpha||isAlpha)
            {
                return;
            }

            SetMaterialTransparent();
            StartCoroutine(Fading());
        }

        private void StartOpaque()
        {
            if (isEditAlpha||!isAlpha)
            {
                return;
            }

            StartCoroutine(Fading());
        }

        private IEnumerator Fading()
        {
            isEditAlpha = true;
            
            var initialColor = targetAlpha < 1 ? 1f : 0.1f;
            var percent = 0f;
            while (percent < 1)
            {
                percent += Time.deltaTime / speed;

                foreach (Renderer m in renderer)
                {
                    m.material.SetFloat("_ALPHA", Mathf.Lerp(initialColor, targetAlpha, percent));
                }

                yield return null;
            }

            if (targetAlpha >= .99f)
            {
                SetMaterialOpaque();
            }

            isAlpha = targetAlpha < 1f;
            isEditAlpha = false;
        }

        #endregion
    }
}