using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
public class BSPMipTexture
{
    public string name;
    public Int32 width;
	public Int32 height;
	public UInt32[] offset;
	public Texture2D texture;

	public BSPMipTexture(string Name, UInt32 Width, UInt32 Height, UInt32[] offset)
    {
		//this.name = RemoveControlCharacters(Name);
		this.name = Name;
        this.width = (int)Width;
        this.height = (int)Height;
        this.offset = offset;
    }

    public int PixelCount()
    {
        return (int)(width * height);
    }
	
	//using bytes because read chars can move stream position depending on the text its reading
	//this removes  ascii control characters that mess up string tests when loading from wad


	


	

}

