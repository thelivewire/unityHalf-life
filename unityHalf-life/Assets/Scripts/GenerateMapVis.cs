using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GenerateMapVis : MonoBehaviour
{
		public Texture2D missingtexture;//use the included missing.jpg
		public string mapName;
		public int currentLeaf;
		public int model1tLeaf;
		public bool renderlights = true;
		private BSP30map map;
		private int faceCount = 0;
		private GameObject[][] leafRoots;
		public Transform player;
		private bool lockpvs = false;
		private int lastpvs = 0;
		public bool RenderAllFaces = false;

		void Start ()
		{
	
				map = new BSP30map (mapName);
				if (map == null)
				{
						Debug.LogError ("Problem Loading map!!!");
						
				}
				

				if (player == null)
				{
						Debug.LogError ("player is null, cant get transform");

				}
				GenerateVisArrays ();
				GenerateVisObjects ();


		}

		void Update ()
		{
				//model1tLeaf = WalkBSP (map.modelLump.models [1].node);


				if (!lockpvs)
				{
						int pvs = WalkBSP ();
						if (pvs != lastpvs)
								currentLeaf = pvs;
						RenderPVS (pvs);
						lastpvs = pvs;
				}
				if (RenderAllFaces)
						RenderPVS (0);
				// Pressing A will toggle locking the PVS
				if (Input.GetKeyDown (KeyCode.Z))
				{
						lockpvs = !lockpvs;
						Debug.Log ("PVS lock: " + lockpvs.ToString ());
				}
	
		}
	
	
		// This will retrieve and render the PVS for the leaf you pass it
		// Must run every frame/however often you want to update the pvs.
		// you can cease calling this to "lock" the pvs.
		private void RenderPVS (int leaf)
		{
				//Debug.Log("Rendering PVS for Leaf: " + leaf.ToString());
				for (int i = 0; i < leafRoots.Length; i++)
				{
						foreach (GameObject go in leafRoots[i])
						{
								go.GetComponent<Renderer>().enabled = false;
						}
				}

				if (leaf == 0)
				{
						for (int i = 0; i < leafRoots.Length; i++)
						{
								foreach (GameObject go in leafRoots[i])
								{

										go.GetComponent<Renderer>().enabled = true;
										if (go.GetComponent<Renderer>().material.mainTexture.name == "sky")
										{
												go.GetComponent<Renderer>().enabled = false;
										}
								}
						}
						return;
				}

				for (int j = 0; j < map.leafLump.leafs[leaf].pvs.Length; j++)
				{
						if (map.leafLump.leafs [leaf].pvs [j] == true)
						{
								foreach (GameObject go in leafRoots[j + 1])
								{ //+1 because leaf 0 is bullshit, trust me
										go.GetComponent<Renderer>().enabled = true;

										if (go.GetComponent<Renderer>().material.mainTexture.name == "sky")
										{
												go.GetComponent<Renderer>().enabled = false;
										}


								}
						}
				}

		}


    #region BSP Lookup
		// Tests a node's plane, and returns the child to be tested next, or the leaf the player is in.
		private int BSPlookup (int node)
		{
				int child;
				if (!map.planeLump.planes [map.nodeLump.nodes [node].planeNum].plane.GetSide (player.position))
				{
						child = map.nodeLump.nodes [node].children [0];
				}
				else
				{
						child = map.nodeLump.nodes [node].children [1];
				}
				return child;
		}

		// This uses the bsp lookup method to find the leaf
		// the camera is in, and returns it.
		// Calling this (just one time) will give you the leaf the player is in.
		private int WalkBSP (int headnode = 0)
		{
				int child = BSPlookup (headnode);
				while (child >= 0)
				{
						child = BSPlookup (child);
				}

				child = -(child + 1);
				return child;
		}



    #endregion

    #region Object array generation
		void GenerateVisArrays ()
		{
	
	
				leafRoots = new GameObject[map.leafLump.numLeafs][];
				for (int i = 0; i < map.leafLump.numLeafs; i++)
				{
						leafRoots [i] = new GameObject[map.leafLump.leafs [i].NumMarkSurfaces];
				}

		}

		void GenerateVisObjects ()
		{
				for (int i = 0; i < map.leafLump.numLeafs; i++)
				{
						for (int j = 0; j < map.leafLump.leafs[i].NumMarkSurfaces; j++)
						{
								leafRoots [i] [j] = GenerateFaceObject (map.facesLump.faces [map.markSurfacesLump.markSurfaces [map.leafLump.leafs [i].FirstMarkSurface + j]]);
								faceCount++;
						}
				}
		}
    #endregion

    #region Face Object Generation

		GameObject GenerateFaceObject (BSPFace face)
		{

        


				GameObject faceObject = new GameObject ("BSPface " + faceCount.ToString ());
				faceObject.transform.parent = gameObject.transform;
				Mesh faceMesh = new Mesh ();
				faceMesh.name = "BSPmesh";




        // grab our verts
        Vector3[] verts = new Vector3[face.numberEdges];
        int edgestep = (int)face.firstEdgeIndex;
        for (int i = 0; i < face.numberEdges; i++)
        {

            if (map.edgeLump.SURFEDGES[face.firstEdgeIndex + i] < 0)
            {
                verts[i] = map.vertLump.ConvertScaleVertex(map.vertLump.verts[map.edgeLump.edges[Mathf.Abs(map.edgeLump.SURFEDGES[edgestep])].vert1]);
              
            }
            else
            {
                verts[i] = map.vertLump.ConvertScaleVertex(map.vertLump.verts[map.edgeLump.edges[map.edgeLump.SURFEDGES[edgestep]].vert2]);
              
            }

            edgestep++;



            }

        // whip up tris
        int[] tris = new int[(face.numberEdges - 2) * 3];
        int tristep = 1;
        for (int i = 1; i < verts.Length - 1; i++)
        {
            tris[tristep - 1] = 0;
            tris[tristep] = i;
            tris[tristep + 1] = i + 1;
            tristep += 3;
        }

        // whip up uvs
        float scales = map.miptexLump[map.texinfoLump.texinfo[face.texinfo_id].miptex].width ;
        float scalet = map.miptexLump[map.texinfoLump.texinfo[face.texinfo_id].miptex].height ;
        Vector2[] uvs = new Vector2[face.numberEdges];
        for (int i = 0; i < face.numberEdges; i++)
        {
         
        uvs[i] = new Vector2((Vector3.Dot(verts[i], map.texinfoLump.texinfo[face.texinfo_id].vec3s) + map.texinfoLump.texinfo[face.texinfo_id].offs) / scales, (Vector3.Dot(verts[i], map.texinfoLump.texinfo[face.texinfo_id].vec3t) + map.texinfoLump.texinfo[face.texinfo_id].offt) / scalet);
        }


        faceMesh.vertices = verts;
				faceMesh.triangles = tris;
				faceMesh.uv = uvs;
				faceMesh.RecalculateNormals ();
				faceObject.AddComponent<MeshFilter> ();
				faceObject.GetComponent<MeshFilter> ().mesh = faceMesh;
				faceObject.AddComponent<MeshRenderer> ();









		if (face.texinfo_id >= 0 && renderlights && face.lightmapOffset < map.lightlump.Length)
				{

						Material bspMaterial = new Material (Shader.Find ("Legacy Shaders/Lightmapped/Diffuse"));
            //bspMaterial.color = new Color(1,1,1,1);
        
						

            Vector3 v0, v1;

            Vertex[] pVertexList = new Vertex[verts.Length];

            float fUMin = 100000.0f;
            float fUMax = -10000.0f;
            float fVMin = 100000.0f;
            float fVMax = -10000.0f;

            float pMipTexheight = map.miptexLump[map.texinfoLump.texinfo[face.texinfo_id].miptex].height;
            float pMipTexwidth = map.miptexLump[map.texinfoLump.texinfo[face.texinfo_id].miptex].width;
            for (int nEdge = 0; nEdge < verts.Length; nEdge++)
            {


                // Add vertex information
                Vertex vertex = new Vertex(verts[nEdge]);

                // Generate texture coordinates for face
                vertex.u = verts[nEdge].x * map.texinfoLump.texinfo[face.texinfo_id].vec3s.x + verts[nEdge].y * map.texinfoLump.texinfo[face.texinfo_id].vec3s.y + verts[nEdge].z * map.texinfoLump.texinfo[face.texinfo_id].vec3s.z + map.texinfoLump.texinfo[face.texinfo_id].offs;
                vertex.v = verts[nEdge].x * map.texinfoLump.texinfo[face.texinfo_id].vec3t.x + verts[nEdge].y * map.texinfoLump.texinfo[face.texinfo_id].vec3t.y + verts[nEdge].z * map.texinfoLump.texinfo[face.texinfo_id].vec3t.z + map.texinfoLump.texinfo[face.texinfo_id].offt;
                vertex.u /= pMipTexwidth;
                vertex.v /= pMipTexheight;
                vertex.lu = vertex.u;
                vertex.lv = vertex.v;

                fUMin = (vertex.u < fUMin) ? vertex.u : fUMin;
                fUMax = (vertex.u > fUMax) ? vertex.u : fUMax;
                fVMin = (vertex.v < fVMin) ? vertex.v : fVMin;
                fVMax = (vertex.v > fVMax) ? vertex.v : fVMax;

                pVertexList[nEdge] = vertex;
            }


            int lightMapWidth = (int)(Mathf.Ceil((fUMax * pMipTexwidth) / 16.0f) - Mathf.Floor((fUMin * pMipTexwidth) / 16.0f) + 1.0f);
            int lightMapHeight = (int)(Mathf.Ceil((fVMax * pMipTexheight) / 16.0f) - Mathf.Floor((fVMin * pMipTexheight) / 16.0f) + 1.0f);

            float cZeroTolerance = 1e-06f;
            // Update light-map vertex u, v coordinates.  These should range from [0.0 -> 1.0] over face.
            float fUDel = (fUMax - fUMin);
            if (fUDel > cZeroTolerance)
                fUDel = 1.0f / fUDel;
            else
                fUDel = 1.0f;
            float fVDel = (fVMax - fVMin);
            if (fVDel > cZeroTolerance)
                fVDel = 1.0f / fVDel;
            else
                fVDel = 1.0f;
            for (int n = 0; n < pVertexList.Length; n++)
            {
                (pVertexList)[n].lu = ((pVertexList)[n].lu - fUMin) * fUDel;
                (pVertexList)[n].lv = ((pVertexList)[n].lv - fVMin) * fVDel;
            }




            //    Debug.Log(lightMapWidth+" "+ lightMapHeight);
            Texture2D lightTex = new Texture2D(lightMapWidth, lightMapHeight);



            Color[] colourarray = new Color[lightMapWidth * lightMapHeight];

            int tempCount = (int)face.lightmapOffset;
            for (int k = 0; k < lightMapWidth * lightMapHeight; k++)
            {
                if (tempCount + 3 > map.lightlump.Length) break;
                colourarray[k] = new Color32(map.lightlump[tempCount], map.lightlump[tempCount + 1], map.lightlump[tempCount + 2], 100);
                tempCount += 3;
            }

            lightTex.SetPixels(colourarray);
           // lightTex.filterMode = FilterMode.Bilinear;
            lightTex.wrapMode = TextureWrapMode.Clamp;
            
            lightTex.Apply();

            Texture2D lmap = lightTex;




            List<Vector2> lvs = new List<Vector2>();
            for (int a=0; a< pVertexList.Length;a++)

            {

                lvs.Add(new Vector2(pVertexList[a].lu, pVertexList[a].lv));


            }
            faceMesh.SetUVs(1, lvs);
            // faceMesh.SetUVs(2, vlist);

            if (map.miptexLump [map.texinfoLump.texinfo [face.texinfo_id].miptex].texture == null)
						{
			
								bspMaterial.mainTexture = missingtexture;

						}
						else
						{
								bspMaterial.mainTexture = map.miptexLump [map.texinfoLump.texinfo [face.texinfo_id].miptex].texture;
								bspMaterial.mainTexture.name = map.miptexLump [map.texinfoLump.texinfo [face.texinfo_id].miptex].name;
						}
			
		


			
						bspMaterial.SetTexture ("_LightMap", lmap);
            bspMaterial.mainTexture.filterMode = FilterMode.Bilinear;
          
                        faceObject.GetComponent<Renderer>().material = bspMaterial;


				}
				else
				{
			
						if (map.miptexLump [map.texinfoLump.texinfo [face.texinfo_id].miptex].texture == null)
						{
								//faceObject.renderer.material.mainTexture = missingtexture;
								faceObject.GetComponent<Renderer>().material.mainTexture = missingtexture;
								Debug.LogError (map.miptexLump [map.texinfoLump.texinfo [face.texinfo_id].miptex].name.ToString () + "not loaded");
						}
						else
						{
								faceObject.GetComponent<Renderer>().material.mainTexture = map.miptexLump [map.texinfoLump.texinfo [face.texinfo_id].miptex].texture;
           

                faceObject.GetComponent<Renderer>().material.mainTexture.name = map.miptexLump [map.texinfoLump.texinfo [face.texinfo_id].miptex].name;
						}

				}
				faceObject.AddComponent<MeshCollider> ();
				faceObject.isStatic = true;
				//faceObject.renderer.enabled = false;

				return faceObject;
		}
    #endregion





}