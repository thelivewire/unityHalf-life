using System;
using UnityEngine;
public class BSPNode
{
		public UInt32 planeNum;
		public int[] children = new int[2];
	public Vector3 bbMin = new Vector3();
	public Vector3 bbMax = new Vector3 ();
		public UInt16   first_face;
		public UInt16   num_faces;

	public BSPNode (UInt32 planeNum, Int16 child1, Int16 child2, Point3s BBMin, Point3s BBMax, UInt16 FirstFace, UInt16 NumberFaces)
		{
				this.planeNum = planeNum;// index of the splitting plane (in the plane array)
		
				children [0] = (int)child1;// index of the front child node or leaf
				children [1] = (int)child2;// index of the back child node or leaf

		bbMin = new Vector3(-BBMin.x,BBMin.z,-BBMin.y);;// minimum x, y and z of the bounding box
		bbMax = new Vector3(-BBMax.x,BBMax.z,-BBMax.y);// minimum x, y and z of the bounding box

				first_face = FirstFace;        // index of the first face (in the face array)
				num_faces = NumberFaces;         // number of consecutive edges (in the face array)


		//bbMin.Scale(new Vector3(0.03f, 0.03f, 0.03f));
		//bbMax.Scale(new Vector3(0.03f, 0.03f, 0.03f));
		}
	public BSPNode(){}

	public BSPNode ( Point3s BBMin)
	{

	}
	public static int Size
	{
		get { return (4+2+2+ (2+2+2)+(2+2+2)+2+2);}
	}



	public override string ToString ()
		{
				return "Node - Plane#: " + planeNum.ToString () + " Children: " + children [0].ToString () + " / " + children [1].ToString () + "\r\n";
		}
}