using UnityEngine;
using System.Collections;

public class move : MonoBehaviour {
	public float speed=1;

	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKey(KeyCode.UpArrow))
		{
			this.transform.Translate( Vector3.forward *speed);
		}

		if (Input.GetKey(KeyCode.DownArrow))
		{
			this.transform.Translate(  Vector3.back *speed);
		}

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			this.transform.Translate( Vector3.left *speed);
		}

		if (Input.GetKey(KeyCode.RightArrow))
		{
			this.transform.Translate(   Vector3.right *speed);
		}


	
	


	}
}
