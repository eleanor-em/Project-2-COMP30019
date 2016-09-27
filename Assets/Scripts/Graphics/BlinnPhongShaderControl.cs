using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer),
                  typeof(MeshFilter))]
public class BlinnPhongShaderControl : MonoBehaviour {
    public Texture2D texture = null;
    public Texture2D normalMap = null;
    public Color color = Color.white;
    public GameObject pointLightManager;
    public float textureScaleFactor = 1f;

    public Color fogColor;
    public float fogDensity = 0.1f;

    private GetPointLights pointLights;
    private MeshRenderer meshRenderer;

    // Lighting parameters
    public float ambientAlbedo = 1;
    public float diffuseAlbedo = 1;
    public float specularAlbedo = 1;
    public float attenuationFactor = 1;
    public float specularExponent = 25;

    // Hack for Unity 5.3 to pass an array to the shader. Cheers Alex, or whoever wrote that code.
    void passToShader() {
        PointLight[] lights = pointLights.Get();
        if (lights.Length >= 256) {
            Debug.LogWarning("Too many lights passed to shader. Some will not be used.");
        }
        for (int i = 0; i < Mathf.Min(lights.Length, 256); ++i) {
            meshRenderer.material.SetVector("_PointLightPositions" + i.ToString(), lights[i].Position);
            meshRenderer.material.SetColor("_PointLightColors" + i.ToString(), lights[i].Color);
        }
        meshRenderer.material.SetInt("_PointLightCount", lights.Length);
        meshRenderer.material.SetColor("_Color", color);
        // Debug
        meshRenderer.material.SetFloat("_Ka", ambientAlbedo);
        meshRenderer.material.SetFloat("_Kd", diffuseAlbedo);
        meshRenderer.material.SetFloat("_Ks", specularAlbedo);
        meshRenderer.material.SetFloat("_fAtt", attenuationFactor);
        meshRenderer.material.SetFloat("_N", specularExponent);
        meshRenderer.material.SetColor("_fogColor", fogColor);
        meshRenderer.material.SetFloat("_fogDensity", fogDensity);
    }
    
	void Start () {
        // Grab the necessary components
        pointLights = pointLightManager.GetComponent<GetPointLights>();
        meshRenderer = GetComponent<MeshRenderer>();
        // Automatically get the correct shader, and add parameters as needed
        if (texture != null) {
            if (normalMap != null) {
                meshRenderer.material.shader = Shader.Find("Unlit/BlinnPhongTexNormalShader");
                meshRenderer.material.SetTexture("_NormalMap", normalMap);
            } else {
                meshRenderer.material.shader = Shader.Find("Unlit/BlinnPhongTexShader");
            }

            meshRenderer.material.SetTexture("_MainTex", texture);
            // Calculate scaling factors
            var mf = GetComponent<MeshFilter>();
            var bounds = mf.mesh.bounds;
            var size = Vector3.Scale(bounds.size, transform.localScale) * textureScaleFactor;
            // Use x and z bounds for small y scale
            if (size.y < 0.001f) {
                size.y = size.z;
            }
            meshRenderer.material.SetTextureScale("_MainTex", size);
        } else {
            meshRenderer.material.shader = Shader.Find("Unlit/BlinnPhongShader");
        }
        // Set properties common to all shaders
	}
	
	void Update () {
        // Pass data to shader
        passToShader();
	}
}
