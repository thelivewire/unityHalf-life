using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CustomExtensions;
using System;

public class BSP30map
{
	private BspInfo bspInfo;
		private int NumTexLoadFromWad = 0; //holds amount of textures in external wads
		private BinaryReader BSPfile;
		public BSPHeader header;
		public BSPColors palette;
		public BSPEntityLump entityLump;
		public BSPFaceLump facesLump;
		public BSPEdgeLump edgeLump;
		public byte[] lightlump;
		public BSPVertexLump vertLump;
		public BSPTexInfoLump texinfoLump;
		public BSPMipTexture[] miptexLump;
		public BSPMarkSurfaces markSurfacesLump;
		public BSPvisLump visLump;
		public BSPLeafLump leafLump;
		public BSPPlaneLump planeLump;
		public BSPNodeLump nodeLump;
		public BSPModelLump modelLump;
		private EntityParser myParser;

		public BSP30map (string filename)
		{

				if (File.Exists ("Assets/Bsp/" + filename) == false)
				{
						Debug.LogError ("Bsp not found");


				}
				BSPfile = new BinaryReader (File.Open ("Assets/Bsp/" + filename, FileMode.Open));
				header = new BSPHeader (BSPfile);
		bspInfo = new BspInfo ();

				ReadEntities ();
				ReadFaces ();
				ReadEdges ();
				ReadVerts ();
				ReadTexinfo ();
				ReadLightLump ();
				ReadTextures (); 
				ReadMarkSurfaces ();
				ReadLeafs ();
				ReadPlanes ();
				ReadNodes ();
				ReadModels ();
				ReadVisData ();

        Debug.Log("data start ");
                Debug.Log("Entity char length " + entityLump.rawEntities.Length.ToString());
        Debug.Log("number of Faces " + bspInfo.mapNum_faces.ToString());
        //   Debug.Log("number of edges " + bspInfo.
        Debug.Log("lightmap length " + bspInfo.mapNum_lighting.ToString());
        Debug.Log("number of verts " + bspInfo.mapNum_verts.ToString());
        Debug.Log("number of Faces " + bspInfo.mapNum_faces.ToString());
        Debug.Log("textures " + bspInfo.mapNum_textures.ToString());
        Debug.Log("marksurf " + markSurfacesLump.markSurfaces.Length.ToString());
        Debug.Log("VisData Length: " + bspInfo.mapNum_visability.ToString());
        Debug.Log("leaf limp  Length: " + leafLump.numLeafs.ToString());
        Debug.Log("plane Length: " + planeLump.planes.Length.ToString());
        Debug.Log("node lump Length: " + nodeLump.nodes.Length.ToString());
        Debug.Log("models " + modelLump.models.Length.ToString());

        Debug.Log("data end ");
        bspInfo.mapNum_clipnodes = header.directory [9].length / 8;
		
				

				ReadPVS ();

				BSPfile.BaseStream.Dispose ();
				
				if (NumTexLoadFromWad > 0)
				{
						Debug.Log ("Reading in textures from wad");
						findNullTextures ();//number of textures we need to load  from the wad files
				}

		}

		//This decompresses and stores the pvs for/inside of each leaf object
		//Only model[0] needs pvs as these are static parts of the map, model[1] and above are entites like doors/sounds/area bounding boxes
		private void ReadPVS ()
		{
				//for each leaf...
				for (int i = 1; i < leafLump.numLeafs; i++)
				{

						int c;
						//Debug.Log(i);
						List<byte> pvs = new List<byte> ();
						int offset = leafLump.leafs [i].VisOffset;
						if (offset == -1)
								continue;
						for (int j = 0; j < Mathf.FloorToInt((modelLump.models[0].numLeafs + 7) / 8);)
						{
								if (offset > visLump.compressedVIS.Length)
								{
										Debug.Log ("somthing wrong here");
								}
								if (visLump.compressedVIS [offset] != 0)
								{
										pvs.Add (visLump.compressedVIS [offset++]);
										j++;
								}
								else
								{
										c = visLump.compressedVIS [offset + 1];
										offset += 2;
										while (c != 0)
										{
												pvs.Add ((byte)0);
												j++;
												c--;
										}

								}
						}
						leafLump.leafs [i].pvs = new BitArray (pvs.ToArray ());
				}
		}

		private void ReadNodes ()
		{

				nodeLump = new BSPNodeLump ();
				BSPfile.BaseStream.Position = header.directory [5].offset;
				int nodeCount = header.directory [5].length / BSPNode.Size;

		bspInfo.mapNum_nodes = nodeCount;
				nodeLump.nodes = new BSPNode[nodeCount];
				for (int i = 0; i < nodeCount; i++)
				{
						nodeLump.nodes [i] = new BSPNode (BSPfile.ReadUInt32 (), BSPfile.ReadInt16 (), BSPfile.ReadInt16 (), BSPfile.ReadPoint3s (),
			                                BSPfile.ReadPoint3s (), BSPfile.ReadUInt16 (), BSPfile.ReadUInt16 ());
				}
		}

		public void findNullTextures ()
		{
				//make a list of textures that need to be loaded from wad files
				TexInfoClass[] texinfo = new TexInfoClass[NumTexLoadFromWad];
				//iterate miptexLump and add a TexInfoClass for each null texture we need to load
				int IndexOfTexinfo = 0;
		
				for (int j=0; j<miptexLump.Length; j++)//!!!!!!!!!! do this in the load miptexLump method instead!!!!!!!!!!!!!!!!!!
				{
						if (miptexLump [j].texture == null)
						{
								texinfo [IndexOfTexinfo] = new TexInfoClass (miptexLump [j].name, j);
								IndexOfTexinfo++;
						}
				}
				//next get the string of  wads we need
				string[] wadFileNames;
				myParser = new EntityParser (entityLump.rawEntities);
				
				Dictionary<string, string> mylist = myParser.ReadEntity ();
				string tempString;
				if (mylist.ContainsKey ("wad"))
				{
						tempString = mylist ["wad"];
						
						wadFileNames = tempString.Split (';');
						for (int i =0; i<wadFileNames.Length; i++)
						{
								wadFileNames [i] = wadFileNames [i].Substring (wadFileNames [i].LastIndexOf ("\\") + 1);//remove unwanted text
								if (wadFileNames [i].Length > 3)
								{
										Debug.Log (wadFileNames [i].ToString ());
										LoadTextureFromWad (wadFileNames [i], texinfo);
								}
						}
				}
				else
				{
						Debug.Log ("no textures to load from wad, or no wad key found in bsp");
				}
		}

		private void ReadPlanes ()
		{
				planeLump = new BSPPlaneLump ();
				BSPfile.BaseStream.Position = header.directory [1].offset;
				int planeCount = header.directory [1].length / 20;
		bspInfo.mapNum_planes = planeCount;
				planeLump.planes = new BSPPlane[planeCount];
				for (int i = 0; i < planeCount; i++)
				{

						planeLump.planes [i] = new BSPPlane (BSPfile.ReadVector3 (), BSPfile.ReadSingle (), BSPfile.ReadInt32 ());
				}
		}

		private void ReadVerts ()
		{

				vertLump = new BSPVertexLump ();
				BSPfile.BaseStream.Position = header.directory [3].offset;
				int numVerts = header.directory [3].length / 12;
		bspInfo.mapNum_verts = numVerts;
				vertLump.verts = new Vector3[numVerts];


      
			for (int i = 0; i < numVerts; i++)
				{
            vertLump.verts[i] = BSPfile.ReadVector3();
                }
		}

		private void ReadEntities ()
		{
				BSPfile.BaseStream.Position = header.directory [0].offset;
				entityLump = new BSPEntityLump (BSPfile.ReadChars (header.directory [0].length));
		}

		private void ReadEdges ()
		{
				edgeLump = new BSPEdgeLump ();
				BSPfile.BaseStream.Position = header.directory [12].offset;
				int numEdges = header.directory [12].length / 4;
		bspInfo.mapNum_edges = numEdges;
				edgeLump.edges = new BSPEdge[numEdges];
				for (int i = 0; i < numEdges; i++)
				{
						edgeLump.edges [i] = new BSPEdge (BSPfile.ReadUInt16 (), BSPfile.ReadUInt16 ());
				}

				
				int numSURFEDGES = header.directory [13].length / 4;
				BSPfile.BaseStream.Position = header.directory [13].offset;
		bspInfo.mapNum_surfedges = numSURFEDGES;
				edgeLump.SURFEDGES = new int[numSURFEDGES];
				for (int i = 0; i < numSURFEDGES; i++)
				{
						edgeLump.SURFEDGES [i] = BSPfile.ReadInt32 ();

				}
		}

		private void ReadFaces ()
		{
				facesLump = new BSPFaceLump ();
				BSPfile.BaseStream.Position = header.directory [7].offset;
				int numFaces = header.directory [7].length / 20;
		bspInfo.mapNum_faces = numFaces;
				facesLump.faces = new BSPFace[numFaces];

				for (int i = 0; i < numFaces; i++)
				{
					
							facesLump.faces [i] = new BSPFace (BSPfile.ReadUInt16 (), BSPfile.ReadUInt16 (), BSPfile.ReadUInt32 (), BSPfile.ReadUInt16 (), BSPfile.ReadUInt16 (),
					                          BSPfile.ReadBytes (4), BSPfile.ReadUInt32 (),header.directory[8].length);





		
				}
				Debug.Log ("faces read");
		}

		private void ReadTexinfo ()
		{
				texinfoLump = new BSPTexInfoLump ();
				BSPfile.BaseStream.Position = header.directory [6].offset;
				int numTexinfos = header.directory [6].length / 40;
				texinfoLump.texinfo = new BSPTexInfo[numTexinfos];
				for (int i = 0; i < numTexinfos; i++)
				{
						texinfoLump.texinfo [i] = new BSPTexInfo (BSPfile.ReadVector3 (), BSPfile.ReadSingle (), BSPfile.ReadVector3 (), BSPfile.ReadSingle (), BSPfile.ReadUInt32 (), BSPfile.ReadUInt32 ());
				}
		}

		private void LoadTextureFromWad (string WadFileName, TexInfoClass[] TexturesToLoad)
		{
				if (File.Exists ("Assets/Wad/" + WadFileName))
				{
//read in wad header
						BinaryReader wadStream = new BinaryReader (File.Open ("Assets/Wad/" + WadFileName, FileMode.Open));
						string wadType = new string (wadStream.ReadChars (4));
						if (wadType != "WAD3" && wadType != "WAD2")
						{
								Debug.LogError ("Wad file wrong type");
								return;
						}
						int numberOfTexs = (int)wadStream.ReadUInt32 ();
						
						wadStream.BaseStream.Position = wadStream.ReadUInt32 (); //move to start of wad directory
						

						TexInfoClass[] TexuresInWadFile = new TexInfoClass[numberOfTexs];//this will hold an array of all texture names and  offsets in the wad file
			              
						//for each texture in wad file get ist name of offset in the file
						for (int i=0; i<TexuresInWadFile.Length; i++)
						{
								TexuresInWadFile [i] = new TexInfoClass ();
								TexuresInWadFile [i].IndexOfMipTex = (int)wadStream.ReadUInt32 (); //get offset for texture
								wadStream.BaseStream.Position += 12;//skip info 
								TexuresInWadFile [i].TextureName = wadStream.LoadCleanString (16);
						}

						//now compare our list of missing textures and load any we find in the TexturesToLoad array
						for (int j=0; j<TexturesToLoad.Length; j++)
						{
								for (int k=0; k<TexuresInWadFile.Length; k++)
								{
								

										if (TexturesToLoad [j].TextureName == TexuresInWadFile [k].TextureName)
										{
												//we found a missing texture so load it
					
												miptexLump [TexturesToLoad [j].IndexOfMipTex] = ReadInTexture (TexuresInWadFile [k].IndexOfMipTex, wadStream, WadFileName);
												if (miptexLump [TexturesToLoad [j].IndexOfMipTex].texture != null)
														TexturesToLoad [j].TextureName = null;//might need check for null returned texture 
												break;

										}


								}
								
						}	
	
						wadStream.Close ();
			
				}
				else
				{
						Debug.LogError ("Error wad file " + WadFileName.ToString () + " not found");
				}

		}

		public BSPMipTexture ReadInTexture (long Texoffset, BinaryReader stream, string wadname)
		{

				long textureOffset = Texoffset; // need this below to locate colour array of this texture
				stream.BaseStream.Position = textureOffset;

				BSPMipTexture miptex = new BSPMipTexture (stream.LoadCleanString (16), stream.ReadUInt32 (), stream.ReadUInt32 (), stream.ReadUInt32Array (4));
	
				//check to see if texture is in a wad file
				if (miptex.offset [0] == 0)//if its zero then the texture is in a wad file, So skip this texture
				{
			
						Debug.Log ("Error Error");

						miptex.texture = null;
						return miptex;
			
				}
				

				miptex.texture = new Texture2D (miptex.width, miptex.height);//set size of texture
				//color palette is 2 bytes after the end of mipmap[4]

				stream.BaseStream.Position = ((miptex.width * miptex.height / 64) + miptex.offset [3] + textureOffset + 2); //Move stream to start of  Palette.
		
		
				byte[] colourArray = stream.ReadBytes (256 * 3);

				//move stream to start of the texture array
				stream.BaseStream.Position = (textureOffset + miptex.offset [0]);
				int NumberOfPixels = (int)(miptex.height * miptex.width);
				byte[] pixelArray = stream.ReadBytes (NumberOfPixels);
				miptex.texture = MakeTexture2D (miptex.height, miptex.width, colourArray, pixelArray);
	

				return miptex;

		}

		private Texture2D MakeTexture2D (int height, int width, byte[] colourArray, byte[] pixelArray)
		{
				//check that arrays are correct size
				if ((width * height) != pixelArray.Length || colourArray.Length != (256 * 3))
				{
						Debug.LogError ("(Method MakeTexture2D) something wrong with array sizes");
						return null;
				}

				//build colour palette
				Color[] colourPalette = new Color[256];//used to hold colour palette array
				int indexOfcolourArray = 0;
				for (int j = 0; j < colourPalette.Length; j++)
				{
						//read in texture colour palette (Unlike quake each texture has its own palette)
						colourPalette [j] = new Color32 (colourArray [indexOfcolourArray], colourArray [indexOfcolourArray + 1], colourArray [indexOfcolourArray + 2], 255);
						indexOfcolourArray += 3;
				}

				//each pixel indexs into above colour palette
				int NumberOfPixels = height * width;
				Color[] colour = new Color[NumberOfPixels];
				int indexInToColourPalette;
				for (int currentPixel = 0; currentPixel < NumberOfPixels; currentPixel++)
				{
			
						colour [currentPixel] = new Color ();
						indexInToColourPalette = (int)pixelArray [currentPixel];
						if (indexInToColourPalette < 0 || indexInToColourPalette > 255)
						{
								Debug.LogError ("something wrong here chap!!!");
						}

						colour [currentPixel] = colourPalette [indexInToColourPalette];
				}
				Texture2D newTexture2D = new Texture2D (width, height);
				newTexture2D.SetPixels (colour);
				newTexture2D.filterMode = FilterMode.Point;//point looks like doom
				newTexture2D.Apply ();
				return newTexture2D;

		}

		private void ReadTextures ()
		{
				//(Half-life textures can be in wad file or the bsp)
		
				BSPfile.BaseStream.Position = header.directory [2].offset;//move to start of texture lump
				int numberOfTextures = (int)BSPfile.ReadUInt32 (); //get the amount of stored / referenced textures in this lump
		
		
		
				miptexLump = new BSPMipTexture[numberOfTextures];
		
				//get offsets of each texture
				Int32[] BSPMIPTEXOFFSET = new  Int32[numberOfTextures]; // Array to store the start position of each texture
				for (int i = 0; i < numberOfTextures; i++)
				{
					
						BSPMIPTEXOFFSET [i] = (header.directory [2].offset + BSPfile.ReadInt32 ());

			
				}

	
				//now load in textures(in half-life each texture has its own palette)
				for (int indexOfTex = 0; indexOfTex < numberOfTextures; indexOfTex++)
				{
			
			
						int textureOffset = BSPMIPTEXOFFSET [indexOfTex]; // need this below to locate colour array of this texture
						BSPfile.BaseStream.Position = textureOffset;
						miptexLump [indexOfTex] = new BSPMipTexture (BSPfile.LoadCleanString (16), BSPfile.ReadUInt32 (), BSPfile.ReadUInt32 (), BSPfile.ReadUInt32Array (4));

						//check to see if texture is in a wad file
						if (miptexLump [indexOfTex].offset [0] == 0)//if its zero then the texture is in a wad file, So skip this texture
						{
				
								
								NumTexLoadFromWad++;
								miptexLump [indexOfTex].texture = null;
								continue;
				
						}
						Debug.Log ("starting to read in texture " + miptexLump [indexOfTex].name.ToString ());
						miptexLump [indexOfTex].texture = new Texture2D (miptexLump [indexOfTex].width, miptexLump [indexOfTex].height);//set size of texture
						//color palette is 2 bytes after the end of mipmap[4]
						Debug.Log ((miptexLump [indexOfTex].width * miptexLump [indexOfTex].height / 64) + (miptexLump [indexOfTex].offset [3] + textureOffset + 2).ToString ());
						BSPfile.BaseStream.Position = ((miptexLump [indexOfTex].width * miptexLump [indexOfTex].height / 64) + miptexLump [indexOfTex].offset [3] + textureOffset + 2); //Move stream to start of  Palette.
		

						Color[] colourPalette = new Color[256];//used to hold colour palette array
						for (int j = 0; j < 256; j++)
						{
								//read in texture colour palette (Unlike quake each texture has its own palette)
								colourPalette [j] = new Color32 (BSPfile.ReadByte (), BSPfile.ReadByte (), BSPfile.ReadByte (), 0);
						}
						//move stream to start of the texture array
						BSPfile.BaseStream.Position = (textureOffset + miptexLump [indexOfTex].offset [0]);
						int NumberOfPixels = (int)(miptexLump [indexOfTex].height * miptexLump [indexOfTex].width);
						Color[] colour = new Color[NumberOfPixels];
						int indexInToColourPalette;
						for (int currentPixel = 0; currentPixel < NumberOfPixels; currentPixel++)
						{
				
								colour [currentPixel] = new Color ();
								indexInToColourPalette = (int)BSPfile.ReadByte ();
								if (indexInToColourPalette < 0 || indexInToColourPalette > 255)
								{
										Debug.LogError ("something wrong here chap!!!");
								}
								//Debug.Log (indexInToPalette.ToString ());
								colour [currentPixel] = colourPalette [indexInToColourPalette];
						}
						miptexLump [indexOfTex].texture.SetPixels (colour);
						miptexLump [indexOfTex].texture.filterMode = FilterMode.Bilinear;
						miptexLump [indexOfTex].texture.Apply ();
			
			
				}	
				Debug.Log ("finished reading textures");
		}

		private void ReadMarkSurfaces ()
		{
				markSurfacesLump = new BSPMarkSurfaces ();
				int numMarkSurfaces = header.directory [11].length / 2;
		bspInfo.mapNum_marksurfaces = numMarkSurfaces;
				markSurfacesLump.markSurfaces = new int[numMarkSurfaces];
				BSPfile.BaseStream.Position = header.directory [11].offset;
				for (int i = 0; i < numMarkSurfaces; i++)
				{
						markSurfacesLump.markSurfaces [i] = BSPfile.ReadUInt16 ();
				}
		}

		private void ReadVisData ()
		{
		bspInfo.mapNum_visability = header.directory [4].length;
				visLump = new BSPvisLump ();
				BSPfile.BaseStream.Position = header.directory [4].offset;
				visLump.compressedVIS = BSPfile.ReadBytes (header.directory [4].length);
		}

		private void ReadModels ()
		{

				modelLump = new BSPModelLump ();
				BSPfile.BaseStream.Position = header.directory [14].offset;
				int modelCount = header.directory [14].length / 64;
		bspInfo.mapNum_models = modelCount;
				modelLump.models = new BSPModel[modelCount];
				for (int i = 0; i < modelCount; i++)
				{
						
						modelLump.models [i] = new BSPModel (BSPfile.ReadVector3 (), BSPfile.ReadVector3 (), BSPfile.ReadVector3 ()
			                                     , BSPfile.ReadInt32Array (4), BSPfile.ReadInt32 (), BSPfile.ReadInt32 (), BSPfile.ReadInt32 ());
				}
		}

		private void ReadLeafs ()
		{
				leafLump = new BSPLeafLump ();
				int leafCount = header.directory [10].length / 28;
		bspInfo.mapNum_leafs = leafCount;
				leafLump.leafs = new BSPLeaf[leafCount];
				leafLump.numLeafs = leafCount;
				BSPfile.BaseStream.Position = header.directory [10].offset;
				for (int i = 0; i < leafCount; i++)
				{
						leafLump.leafs [i] = new BSPLeaf (BSPfile.ReadInt32 (), BSPfile.ReadInt32 (), BSPfile.ReadBBoxshort (), BSPfile.ReadBBoxshort (),
			                                 BSPfile.ReadUInt16 (), BSPfile.ReadUInt16 (), BSPfile.ReadBytes (4));


				}


		}

		void ReadLightLump ()
		{
		bspInfo.mapNum_lighting = header.directory [8].length;
				BSPfile.BaseStream.Position = header.directory [8].offset;

				if (header.directory [8].length == 0)
						return;



				lightlump = BSPfile.ReadBytes (header.directory [8].length);



		}


}
