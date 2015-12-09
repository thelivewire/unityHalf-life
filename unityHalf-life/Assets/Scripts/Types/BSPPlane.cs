using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BSPPlane
{
    public Vector3 normal;
    public float distance;
    public Int32 type;
    public Plane plane;

	//PLANE_X 0     // Plane is perpendicular to given axis
	//PLANE_Y 1
	//PLANE_Z 2
	//PLANE_ANYX 3  // Non-axial plane is snapped to the nearest
	//PLANE_ANYY 4
	//PLANE_ANYZ 5
    public BSPPlane(Vector3 normal, float distance, Int32 type)
    {
		this.normal = -normal; // Invert,  Half-Life uses left-handed coordinate system
        //this.distance = distance * 0.03f;
        this.distance = distance ;


        this.type = type;
        Swizzle();
        this.plane = new Plane(this.normal, this.distance);
    }

    public override string ToString()
    {
        return "Normal: " + plane.normal.ToString() + " D: " + plane.distance.ToString();
    }

    private void Swizzle()
    {
        float tempx = -normal.x;
        float tempy = normal.z;
        float tempz = -normal.y;
        normal = new Vector3(tempx, tempy, tempz);
       // normal.Scale(new Vector3(0.03f, 0.03f, 0.03f));
    }
}

