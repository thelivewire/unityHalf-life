using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


public class BSPHeader
{

	string[] Lump_type = {
		
		"LUMP_ENTITIES",
		"LUMP_PLANES",
		"LUMP_TEXTURES",
		"LUMP_VERTICES",
		"LUMP_VISIBILITY",
		"LUMP_NODES",
		"LUMP_TEXINFO",
		"LUMP_FACES",
		"LUMP_LIGHTING",
		"LUMP_CLIPNODES",
		"LUMP_LEAVES",
		"LUMP_MARKSURFACES",
		"LUMP_EDGES",
		"LUMP_SURFEDGES",
		"LUMP_MODELS",
		"HEADER_LUMPS" };

    public class HeaderEntry
    {
        public int offset;
        public int length;
		public string lump_type;//just to make it esay to debug lumps
	
        public HeaderEntry(int offset, int length,string lumpType)
        {
            this.offset = offset;
            this.length = length;
			this.lump_type = lumpType;

        }
		public HeaderEntry(){}
        public override string ToString()
        {
            return "Offset: " + offset + " Length: " + length + "\r\n";
        }
    }

   
	public HeaderEntry[] directory = new HeaderEntry[16];
    public Int32 version;

    public BSPHeader(BinaryReader map)
    {

		map.BaseStream.Position=0;
        version = map.ReadInt32();
		if(version!=30) Debug.LogError("Bsp is wrong type");


        for (int i = 0; i < 16; i++)
        {

			directory[i]= new HeaderEntry(map.ReadInt32(),map.ReadInt32(),Lump_type[i]);
			

        }

    }

    public void PrintInfo()
    {
        for (int i = 0; i < 15; i++)
        {
            Debug.Log("Lump " + i.ToString() + " " + directory[i].ToString());
        }
    }
}

