using UnityEngine;

namespace CaptainHindsight
{
    [ExecuteInEditMode]
    public class MShaderShockwave : MonoBehaviour
    {

        [SerializeField] public Material shaderMaterial;
        [SerializeField] private float percentageOfShader;
        [HideInInspector] private float defaultPercentageOfShader = 1.5f;
        // The above is to avoid seeing remnants of the shader during normal play
        [SerializeField] public float DurationOfShader = 1;
        [SerializeField] private bool shockwaveFired;
        [SerializeField] private bool shockwaveEnds;

        private void Awake() => shaderMaterial.SetFloat("_Percent", defaultPercentageOfShader);

        private void Update()
        {
            // Outward progression of the shader
            if (shockwaveFired)
            {
                percentageOfShader += Time.deltaTime / DurationOfShader;
                if (percentageOfShader > 1)
                {
                    percentageOfShader = defaultPercentageOfShader;
                    shockwaveFired = false;
                }
                shaderMaterial.SetFloat("_Percent", percentageOfShader);
            }

            // Inward progression of the shader
            if (shockwaveEnds)
            {
                percentageOfShader -= Time.deltaTime / DurationOfShader;
                if (percentageOfShader < 0)
                {
                    percentageOfShader = defaultPercentageOfShader;
                    shockwaveEnds = false;
                }
                shaderMaterial.SetFloat("_Percent", percentageOfShader);
            }
        }

        public void InitiateShockwaveEffect(float durationInSeconds)
        {
            shockwaveEnds = false;
            percentageOfShader = 0;
            DurationOfShader = durationInSeconds;
            shockwaveFired = true;
        }

        public void EndShockwaveEffect(float durationInSeconds)
        {
            shockwaveFired = false;
            percentageOfShader = 1;
            DurationOfShader = durationInSeconds;
            shockwaveEnds = true;
        }
    }
}