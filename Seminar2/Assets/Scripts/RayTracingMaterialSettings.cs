using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaterialSettings : MonoBehaviour
{

    public Color colour;
    public Color ambientColor;
    public Color diffuseColor;
    public Color specularColor;
    public float specularFactor;
    public float indeksLoma;
    public float reflectionCoef;
    public float refractionCoef;

    // Start is called before the first frame update
    void Awake()
    {
        colour = this.GetComponent<Renderer>().material.color;
        ambientColor = this.GetComponent<Renderer>().material.color;
        diffuseColor = this.GetComponent<Renderer>().material.color;
        specularColor = this.GetComponent<Renderer>().material.color;
    }


}
