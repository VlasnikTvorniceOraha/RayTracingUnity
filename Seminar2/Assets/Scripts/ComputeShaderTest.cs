using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader computeShader;

    public RenderTexture renderTexture;

    public bool turnOn;
    public bool blackWhite;
    public bool ambient;
    public bool diffuse;
    public bool specular;
    public bool reflection;
    public bool refraction;
    public bool shadows;
    public bool blinnPhong;
    public bool blackBackground;

    public int bounceNumber;

    //struct RayTracingMaterial {
      //  public Color colour;
        //public float reflectionCoef;
        //public float refractionCoef;
    //}

    public struct LightData {
        public Vector3 position;
        public Color intensity;
    }

    public LightData sceneLight;

    public Color ambientColor;

    struct RayTracingMaterial {
	    public Color ambientColor;
	    public Color diffuseColor;
	    public Color specularColor;
	    public float specularFactor;
	    public float indeksLoma;
	    public float reflectionCoef;
	    public float refractionCoef;
    }


    struct Sphere {
        public float radius;
        public Vector3 position;
        public RayTracingMaterial material;
    }

    struct Ray {
        Vector3 origin;
        Vector3 direction;
    }

    Sphere[] sphereData;
    Ray[] rayData;

    ComputeBuffer computeBuffer;
    // Start is called before the first frame update
    void Start()
    {
        //kamera stvari test

        computeShader.SetMatrix("_CameraToWorldMatrix", Camera.main.cameraToWorldMatrix);
        computeShader.SetMatrix("_CameraInverseProjectionMatrix", Camera.main.projectionMatrix.inverse);

        //posalji poziciju kamere u sjencar
        computeShader.SetVector("cameraPos", Camera.main.transform.position);

        //postavi ambijentalno svijetlo

        computeShader.SetVector("ambientLight", ambientColor);

        //postavi svijetlo svijetlo u sjencar

        sceneLight.position = GameObject.FindGameObjectWithTag("Light").transform.position;
        sceneLight.intensity = GameObject.FindGameObjectWithTag("Light").GetComponent<Light>().color;

        computeShader.SetVector("sceneLightPos", sceneLight.position);
        computeShader.SetVector("sceneLightIntensity", sceneLight.intensity);

        //postavljanje bool
        computeShader.SetBool("blackWhite", blackWhite);
        computeShader.SetBool("ambient", ambient);
        computeShader.SetBool("diffuse", diffuse);
        computeShader.SetBool("specular", specular);
        computeShader.SetBool("reflection", reflection);
        computeShader.SetBool("refraction", refraction);
        computeShader.SetBool("shadows", shadows);
        computeShader.SetBool("blinnPhong", blinnPhong);
        computeShader.SetBool("blackBackground", blackBackground);

        computeShader.SetInt("bounceNumber", bounceNumber);

        //get info on spheres in the scene

        GameObject[] sfereUSceni = GameObject.FindGameObjectsWithTag("Sphere");

        sphereData = new Sphere[sfereUSceni.Length];

        int brojac = 0;

        

        foreach (GameObject sfera in sfereUSceni) {

            Sphere sferaGPU;

            sferaGPU.radius = sfera.GetComponent<SphereCollider>().radius*Mathf.Max(sfera.transform.lossyScale.x, sfera.transform.lossyScale.y, sfera.transform.lossyScale.z);

            sferaGPU.position = sfera.transform.position;

            //dobij material iz gameobjecta i postavi ga na gpu
            RayTracingMaterialSettings sphereSettings = sfera.GetComponent<RayTracingMaterialSettings>();

            RayTracingMaterial rtmaterial;

            rtmaterial.ambientColor = sphereSettings.ambientColor;

            rtmaterial.diffuseColor = sphereSettings.diffuseColor;

            rtmaterial.specularColor = sphereSettings.specularColor;

            rtmaterial.specularFactor = sphereSettings.specularFactor;

            rtmaterial.indeksLoma = sphereSettings.indeksLoma;

            rtmaterial.reflectionCoef = sphereSettings.reflectionCoef;

            rtmaterial.refractionCoef = sphereSettings.refractionCoef;
            
            sferaGPU.material = rtmaterial;

            sphereData[brojac] = sferaGPU;

            brojac += 1;

            
        }

        //slanje na gpu

        int Vector3Size = sizeof(float) * 3;
        int radiusSize = sizeof(float);

        int rtMaterialSize = sizeof(float) * 4 * 3 + sizeof(float) * 4;

        int totalSize = Vector3Size + radiusSize + rtMaterialSize;

        


        
        computeBuffer = new ComputeBuffer(sphereData.Length, totalSize);
        computeBuffer.SetData(sphereData);
        //uspjesno postavljeni podatci
        computeShader.SetBuffer(computeShader.FindKernel("CSMain"), "spheres", computeBuffer);
        //computeShader.Dispatch(computeShader.FindKernel("SphereMain"), 1, 1, 1);

       
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if (turnOn) {
            //postavljanje bool
            computeShader.SetBool("blackWhite", blackWhite);
            computeShader.SetBool("ambient", ambient);
            computeShader.SetBool("diffuse", diffuse);
            computeShader.SetBool("specular", specular);
            computeShader.SetBool("reflection", reflection);
            computeShader.SetBool("refraction", refraction);
            computeShader.SetBool("shadows", shadows);
            computeShader.SetBool("blinnPhong", blinnPhong);
            computeShader.SetBool("blackBackground", blackBackground);

            computeShader.SetInt("bounceNumber", bounceNumber);

            sceneLight.position = GameObject.FindGameObjectWithTag("Light").transform.position;
            sceneLight.intensity = GameObject.FindGameObjectWithTag("Light").GetComponent<Light>().color;

            computeShader.SetVector("ambientLight", ambientColor);
            computeShader.SetVector("sceneLightPos", sceneLight.position);
            computeShader.SetVector("sceneLightIntensity", sceneLight.intensity);
            if (renderTexture == null) {
                
                renderTexture = new RenderTexture(src.width, src.height, src.depth);
                //renderTexture.antiAliasing = 4;
                renderTexture.enableRandomWrite = true;
                renderTexture.Create();

                
                

            }
            

            Graphics.Blit(src, renderTexture);

            

            //postavljanje na shaderu

            computeShader.SetTexture(computeShader.FindKernel("CSMain"), "Result", renderTexture);
            computeShader.SetFloat("Resolution", renderTexture.width);
            computeShader.Dispatch(computeShader.FindKernel("CSMain"), renderTexture.width / 8, renderTexture.height / 8, 1);

            Graphics.Blit(renderTexture, dest);


            //provjeri rayinfo od rayeva
        

            /*computeShader.SetTexture(0, "Result", src);
            computeShader.SetFloat("Resolution", src.width);
            computeShader.Dispatch(0, src.width / 8, src.height / 8, 1);

            Graphics.Blit(src, dest);*/


        } else {

            Graphics.Blit(src, dest);
        }
        




    }

    private void OnDestroy() {
        computeBuffer.Release();
    }
}
