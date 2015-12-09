using UnityEngine;
using System.Collections;

public class TexInfoClass
{
	public int IndexOfMipTex;
	public string TextureName;
	public TexInfoClass(string name,int index)
	{
		TextureName=name;
		IndexOfMipTex=index;
	}
	public TexInfoClass()
	{
	
	}
}
