using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Parabox.CSG;
using Boolean = Parabox.CSG.Boolean;
using Debug = UnityEngine.Debug;

public class Sample_Move : MonoBehaviour
{
    bool Move = false;
    float value_add;
    private GameObject cube;
    private GameObject cylinder;
    
    // Start is called before the first frame update
    void Start()
    {
        value_add = -0.01f;
        cube = GameObject.Find("Cube");
        cylinder = GameObject.Find("Cylinder");
    }
    
    public void StartMove()
    {
        Move=!Move;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Move){
            GameObject.Find("Cylinder").transform.Translate(new Vector3(0,0,value_add));

            // Do the operation subtration between the cube and the cylinder 
            
            
            GameObject.Find("Result").GetComponent<MeshFilter>().mesh.Clear();

            // Put the mesh result in the mesh give in parameter if you don't give a mesh he return a new mesh with the result


            long startTicks = Stopwatch.GetTimestamp();
            CSG_Model result = Boolean.Intersect(cube, cylinder);
            long endTicks = Stopwatch.GetTimestamp();

            double elapsedMicroseconds = (endTicks - startTicks) * 1_000_000.0 / Stopwatch.Frequency;
            Debug.Log($"Boolean subtraction took: {elapsedMicroseconds:F3} µs");
            
            
            GameObject.Find("Result").GetComponent<MeshFilter>().mesh = result.mesh;
            
            if(GameObject.Find("Cylinder").transform.position.y>0.5f){
                value_add = -0.01f;
            }
            else if(GameObject.Find("Cylinder").transform.position.y<-0.5f){
                value_add = 0.01f;
            }
        }
        
    }

}
