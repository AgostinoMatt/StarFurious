using UnityEngine;
using System.Collections;

namespace RFLib
{
	public class AutoRotator : MonoBehaviour 
	{

		public float XRotation = 0.0f;
		public float YRotation = 0.0f;
		public float ZRotation = 0.0f;

		Vector3 rotVector = Vector3.zero;

		// Use this for initialization
		void Start () 
		{
		}
		
		// Update is called once per frame
		void Update () 
		{
			rotVector.x = XRotation;
			rotVector.y = YRotation;
			rotVector.z = ZRotation;

			transform.Rotate(rotVector);
		}
	}
}