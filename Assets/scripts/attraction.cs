﻿using UnityEngine;
using System.Collections;

public class attraction : MonoBehaviour {
	public float rotationSpeed = 1.0f;

	public float pullFactor = 1.0f;

	Transform[] cornerPoints;
	Transform[] centerPoints;
	Vector3[] faceNormals = new Vector3[3];
	System.Collections.Generic.List<Connection> joints;
	public TriangleGrid gridScript;
	bool currentlyColliding = false; //true if the triangle is making contact with another triangle

	/**
	 * Represents a PullForce will all variables needed for one. 
	 * @param Vector3 applyPos is the original point of the triangle this script is on
	 * @param Vector3 dstPos is the point that is closest to the original point, this is a corner or middle point
	 * depending on what the original point is. 
	 * @param Vector3 force is the closest point - the original point
	 */
	public struct PullForce
	{
		public Vector3 applyPos;
		public Vector3 dstPos;
		public Vector3 force;	
		public float dist;
		
		public PullForce(Vector3 applyPos_, Vector3 dstPos_, Vector3 force_)
		{
			applyPos = applyPos_;
			dstPos = dstPos_;
			force = force_;
			dist = Vector3.Distance(applyPos_, dstPos_);
		}
	};

	/**
	 * Adds a connection between this triangle and the colliding triangle. 
	 * @param Transform ctrlPoint1 is the point which the original triangle is joining the colliding one on.
	 * @param Transform ctrlPoint2 is the point which the colliding triangle is joining on
	 */
	public struct Connection
	{
		public Transform ctrlPoint1;
		public Transform ctrlPoint2;
		public Transform connectedTriangle;
		
		public Connection(Transform p1, Transform p2, Transform connectedTriangle_)
		{
			ctrlPoint1 = p1;
			ctrlPoint2 = p1;
			connectedTriangle = connectedTriangle_;
		}
	};

	// Use this for initialization
	void Awake () {
		cornerPoints = new Transform[3];
		centerPoints = new Transform[3];
		
		int cornerIndex = 0;
		int centerIndex = 0;
		
		for(int i=0; i< transform.childCount; i++)
		{
			if(transform.GetChild(i).tag == "CornerControlPoint")
			{
				cornerPoints[cornerIndex] = transform.GetChild(i);
				cornerIndex++;
			}
			else if(transform.GetChild(i).tag == "MiddleControlPoint")
			{
				centerPoints[centerIndex] = transform.GetChild(i);
				centerIndex++;
			}
		}
		
		joints = new System.Collections.Generic.List<Connection>();

		gridScript = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TriangleGrid>();

		faceNormals[0] = Quaternion.Euler( new Vector3(0, 0, 60)) * transform.up;
		faceNormals[1] = Quaternion.Euler( new Vector3(0, 0, 120)) * faceNormals[0];
		faceNormals[2] = Quaternion.Euler( new Vector3(0, 0, 120)) * faceNormals[1];


	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		//apply forces for attraction
		applyForces();			
		
		if(joints.Count > 0)
		{
			GlobalFlags.canFire = true;
			gridScript.connectTriangle(joints[0].connectedTriangle.gameObject, gameObject);
			rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			enabled = false;
		}
	}
	
	void OnCollisionStay(Collision collision) 
	{

		if(enabled == false)
		{
			return;
		}

		if(collision.gameObject.tag == "Triangle" && collision.gameObject.transform.childCount >0)
		{
			for(int i=0; i< cornerPoints.Length; i++)
			{
				for(int j=0; j< (cornerPoints.Length + centerPoints.Length); j++)
				{
					if(collision.gameObject.transform.GetChild(j).tag == "MiddleControlPoint" &&
						isCornerTouching(centerPoints[i].position, collision.gameObject.transform.GetChild(j).position))
					{
						//lock triangles
						attemptFormConnection(centerPoints[i].transform, collision.gameObject.transform.GetChild(j), collision.gameObject.transform);
					}
				}
			}
		}
	}
	
	void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "Triangle")
		{
			currentlyColliding = true;
		}
	}
	
	void OnCollisionExit(Collision collision)
	{
        if(collision.gameObject.tag == "Triangle")
		{
			currentlyColliding = false;
		}
    }

	/**
	 * @param Transform t1 the point of the original triangle that a connection will tried to be made on
	 * @param Transform t2 point of colliding triangle
	 * @param Transform CollisionObj is the colliding triangle
	 */
	void attemptFormConnection(Transform t1, Transform t2, Transform collisionObj)
	{
		for(int i=0; i< joints.Count; i++)
		{
			//if joint for the connection exists we don't want to create another
			if((joints[i].ctrlPoint1.position == t1.position) && (joints[i].ctrlPoint2.position == t2.position))
			{
				return;
			}		
			else
			{
				//Debug.Log ((joints[i].ctrlPoint1.position - t1.position) + ", " + (joints[i].ctrlPoint2.position - t2.position));
			}
		}
		
		Vector3 test = t2.position - t1.position;
		transform.position =  transform.position + test;
		
		joints.Add(new Connection(t1, t2, collisionObj));
	}

	void applyForces()
	{
		PullForce[] centerForces = new PullForce[3];
		PullForce[] lowestCenterForces =  new PullForce[3];
		PullForce[] cornerForces = new PullForce[3];
		PullForce[] lowestCornerForces =  new PullForce[3];

		for(int i=0; i< centerPoints.Length; i++)
		{
			centerForces[i] = getForceForPoint(centerPoints[i], false);
			cornerForces[i] = getForceForPoint(cornerPoints[i], true);
		}
		
		lowestCenterForces = sortForces(centerForces);
		lowestCornerForces = sortForces(cornerForces);

		// only force to attract triangle
		float forceMult = 1f;
		Vector3 finalForce = Vector3.zero;
		
		int numberOfCornerForces = 2;
		
		if(lowestCornerForces[0].dist < 1) //change priority of attraction point depending on distance. not done yet
		{
//			numberOfCornerForces = 1;
//			forcemult = 2;
		}
		
		for(int i=0; i< numberOfCornerForces; i++)
		{
			//finalForce += lowestCornerForces[i].force * forcemult;
		}
		
		
		for(int i=0; i< 1; i++)
		{	
			finalForce += lowestCenterForces[i].force;
			//GetComponent<Rigidbody>().AddForce(lowestCenterForces[i].force * forcemult);	
		}
		
		if(lowestCenterForces[0].dist > 0)
		{
			forceMult = 1/(lowestCenterForces[0].dist);
			forceMult = pullFactor/(Mathf.Pow(lowestCenterForces[0].dist, 2));
		}
		
		GetComponent<Rigidbody>().AddForce(finalForce * forceMult);	

		//if we are not colliding try to rotate to face the closest triangle
		//if(!currentlyColliding || true)
		{
			float smallestRot = Vector3.Angle((transform.rotation * faceNormals[0]).normalized, (lowestCenterForces[0].dstPos - transform.position).normalized);
			//find polarity
			Vector3 cross = Vector3.Cross(new Vector3(0, 0, 1), transform.rotation * faceNormals[0]);
			float dotProduct = Vector3.Dot(cross, lowestCenterForces[0].dstPos - transform.position);
			bool negative = dotProduct < 0;

			for(int i=1; i < faceNormals.Length; i++)
			{
				float rotToFace = Vector3.Angle((transform.rotation * faceNormals[i]).normalized, (lowestCenterForces[0].dstPos - transform.position).normalized);
	
				if(smallestRot > rotToFace)
				{
					smallestRot = rotToFace;
					//find polarity
					cross = Vector3.Cross(new Vector3(0, 0, 1), transform.rotation * faceNormals[i]);
					dotProduct = Vector3.Dot(cross, lowestCenterForces[0].dstPos - transform.position);
					negative = dotProduct < 0;
				}
			}
			
			if(negative)
			{
				if(smallestRot < Time.deltaTime * rotationSpeed)
				{
					transform.Rotate(new Vector3(0, 0, -smallestRot));
				}
				else
				{
					transform.Rotate(new Vector3(0, 0, Time.deltaTime * -rotationSpeed));
				}
			}
			else
			{
				if(smallestRot < Time.deltaTime * rotationSpeed)
				{
					transform.Rotate(new Vector3(0, 0, smallestRot));
				}
				else
				{
					transform.Rotate(new Vector3(0, 0, Time.deltaTime * rotationSpeed));
				}
			}
		}
	}
	
	bool isCornerTouching(Vector3 c1, Vector3 c2)
	{
		c1.z = 0;
		c2.z = 0;
		return (Vector3.Distance(c1, c2) < 0.1f);
	}

	/**
	 * Used to determine the force for a given point. It will find the closest corner/middle point
	 * in all of the triangles in the scene and return a pullforce struct
	 * @param Transform is the transform point that we want to determine the pull force it has
	 * @param bool is to say it it's a corner point or not. 
	 */
	PullForce getForceForPoint(Transform o, bool cornerPoint)
	{
		GameObject[] points;
		if(cornerPoint)
		{
			points = GameObject.FindGameObjectsWithTag("CornerControlPoint");
		}
		else
		{
			points = GameObject.FindGameObjectsWithTag("MiddleControlPoint");
		}
		 
		
		Transform closestPoint = o;
		double dist = -1;
		
		for(int i=0; i< points.Length; i++)
		{
			double newDist = Vector3.Distance(o.position, points[i].transform.position);
			if(!shouldIgnore(points[i]) && ((dist == -1) || (newDist< dist)))
			{
				closestPoint = points[i].transform;
				dist = newDist;
			}
		}
		
		//make sure it found a point that is closest
		if(dist != -1)
		{
			Vector3 force = (closestPoint.position - o.position);
			
			return new PullForce(o.position, closestPoint.position, force);
		}
		
		// return max force so it is never chosen as the lowest force. this is the equivlant to null
		return new PullForce(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue ), new Vector3(float.MaxValue, float.MaxValue, float.MaxValue )); 
	}

	/**
	 * Used to determine if this point is a child of the triangle we're currently in
	 * @param Gameobject the point which we compare. 
	 */
	bool shouldIgnore(GameObject o)
	{
		if(o.transform.IsChildOf(transform))
		{
			return true;
		}
		
		return false;
	}
	
	// use selection sort to return a sort list of the pull forces
	// sorted from smallest to largest
	PullForce[] sortForces(PullForce[] a)
	{
		int i,j;
		int iMin;
		 
		for (j = 0; j < a.Length - 1; j++) {
		    iMin = j;
		    for ( i = j+1; i < a.Length; i++) {
		        if (a[i].dist < a[iMin].dist) {
		            iMin = i;
		        }
		    }

		    if ( iMin != j ) {
				PullForce temp = a[j];
				a[j] = a[iMin];
		       	a[iMin] = temp;
		    }
		}
		
		return a;
	}
}
