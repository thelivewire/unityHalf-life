using System.Collections;
using UnityEngine;
using System;

public class BSPLeaf
{
	public Int32 ContentsType;
	public Int32 VisOffset;
    public Vector3 mins;
    public Vector3 maxs;
    public int FirstMarkSurface;
    public int NumMarkSurfaces;
	public byte[] AmbientLevels;
    public BitArray pvs;

	public BSPLeaf(Int32 type, Int32 vislist, Vector3 Mins, Vector3 Maxs, ushort lface_index, ushort num_lfaces,byte[] ambientLevels )
    {
        this.ContentsType = type;
        this.VisOffset = vislist;
		this.mins=Mins;
		this.maxs=Maxs;
        this.FirstMarkSurface = (int)lface_index;
        this.NumMarkSurfaces = (int)num_lfaces;
		this.AmbientLevels=ambientLevels;
      
    }

    private Vector3 SwizVert(Vector3 vert)
    {
      // vert.Scale(new Vector3(0.03f, 0.03f, 0.03f));
        float tempx = -vert.x;
        float tempy = vert.z;
        float tempz = -vert.y;
        return new Vector3(tempx, tempy, tempz);
    }

    public override string ToString()
    {
        return "Type: " + ContentsType.ToString() + " Vislist: " + VisOffset.ToString() + " Mins/Maxs: " + mins.ToString() + " / " + maxs.ToString();
    }
}
