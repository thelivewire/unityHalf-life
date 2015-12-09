using UnityEngine;
using System.Collections;

public class Vertex

{

    public Vector3 vPosition;             // Vertex position
   public  float lu, lv;               // Texture coordinates for HL light-map
   public  float u, v;                 // Texture coordinates for HL texture map
   
    public Vertex(Vector3 pos)
    {
        vPosition = pos;
    }



	
	}

