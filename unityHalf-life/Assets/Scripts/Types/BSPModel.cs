using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BSPModel
{

	Vector3 nMins;// Defines bounding box
	Vector3 nMaxs;   // Defines bounding box       
	Vector3 vOrigin;                  // Coordinates to move the // coordinate system
	public Int32[] nodes;// [0]index of first BSP node, [1]index of the first Clip node, [2]index of the second Clip node, [3]  usually zero

	public Int32 numLeafs;// number of BSP leaves
	Int32 indexOfFirstFace,numberOfFaces;// Index and count into faces lump



    


    public int node { get { return nodes[0]; } }

	public BSPModel(Vector3 mins,Vector3 maxs,Vector3 origin,int[] nodes, int numLeafs,Int32 firstFace,Int32 numFaces)
    {
		this.nMins=mins;
		this.nMaxs=maxs;
		this.vOrigin=origin;
        this.nodes = nodes;
        this.numLeafs = numLeafs;
		this.indexOfFirstFace =firstFace;
		this.numberOfFaces =numFaces;
    }
}

