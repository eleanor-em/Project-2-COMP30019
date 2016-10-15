using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer),
                  typeof(MeshFilter))]
public class BlinnPhongShaderControl : MonoBehaviour {
    public Texture2D texture = null;
    public Texture2D normalMap = null;
    public Color color = Color.white;
    public float textureScaleFactorX = 1;
    public float textureScaleFactorY = 1;

    public Color fogColor;
    public float fogDensity = 0.06f;

    private MeshRenderer meshRenderer;

    private Light[] lights;

    // Lighting parameters
    public float ambientAlbedo = 1;
    public float diffuseAlbedo = 1;
    public float specularAlbedo = 1;
    public float specularExponent = 25;

    // Hack for Unity 5.3 to pass an array to the shader. Cheers Alex, or whoever wrote that code.
    void passToShader() {
        int numLights = lights.Length;
        // Set light data
        if (numLights >= 256) {
            Debug.LogWarning("Too many lights passed to shader. Some will not be used.");
            numLights = 256;
        }
        meshRenderer.material.SetInt("_PointLightCount", numLights);
        for (int i = 0; i < numLights; ++i) {
            meshRenderer.material.SetVector("_PointLightPositions" + i.ToString(), lights[i].transform.position);
            meshRenderer.material.SetColor("_PointLightColors" + i.ToString(), lights[i].color);
            meshRenderer.material.SetVector("_PointLightAttenuations" + i.ToString(), new Vector2(lights[i].range, 0));
        }
        // Set light reflectance values
        meshRenderer.material.SetColor("_Color", color);
        meshRenderer.material.SetFloat("_Ka", ambientAlbedo);
        meshRenderer.material.SetFloat("_Kd", diffuseAlbedo);
        meshRenderer.material.SetFloat("_Ks", specularAlbedo);
        meshRenderer.material.SetFloat("_N", specularExponent);
        meshRenderer.material.SetColor("_fogColor", fogColor);
        meshRenderer.material.SetFloat("_fogDensity", fogDensity);
    }
    
	void Start () {
        // Grab the light array, only selecting point lights
        var list = new List<Light>(FindObjectsOfType<Light>());
        list.RemoveAll(light => light.type != LightType.Point);
        lights = list.ToArray();
                                                            
        // Set shader values
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
            // This is for sizing/tiling non-UV-mapped textures
            if (textureScaleFactorX != 0 && textureScaleFactorY != 0) {
                // Calculate scaling factors by finding bounds
                var mf = GetComponent<MeshFilter>();
                var bounds = mf.mesh.bounds;
                var size = Vector3.Scale(bounds.size, transform.localScale);
                size.Scale(new Vector3(textureScaleFactorX, 0f, textureScaleFactorY));
                // Use x and z bounds for small y
                if (size.y < 0.001f) {
                    size.y = size.z;
                }
                meshRenderer.material.SetTextureScale("_MainTex", size);
            }
        } else {
            meshRenderer.material.shader = Shader.Find("Unlit/BlinnPhongShader");
        }
	}
	
	void Update () {
        passToShader();
	}
}
