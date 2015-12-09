using UnityEngine;
using System.Collections;

public class cammove : MonoBehaviour {
	public float speed=1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKey(KeyCode.E))
		{
			this.transform.Translate( Vector3.up *speed);
		}
		
		if (Input.GetKey(KeyCode.Q))
		{
			this.transform.Translate(  Vector3.down *speed);
		}
	}
}
