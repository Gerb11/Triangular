﻿using UnityEngine;
using System.Collections;

public class TriangleGrid : MonoBehaviour {

	public GameObject rootTriangle;
	public GameObject trianglePrefab;

	public int gridSize = 0;
	// use getNode to access. uses euclidian coordinates. 0,0 is th center triangle
	private triangleNode[,] grid;

	private class triangleNode
	{
		public GameObject triangleObject;
		public int x, y;

		public triangleNode(GameObject obj, int x_, int y_)
		{
			triangleObject = obj;
			x = x_;
			y = y_;
		}
	};

	// Use this for initialization
	void Start () 
	{
		grid = new triangleNode[30,30];
		gridSize = (int)Mathf.Sqrt(grid.Length);
		
		for(int i=0; i<gridSize; i++)
		{
			for(int j=0; j<gridSize; j++)
			{
				grid[i,j] = null;
			}
		}

		setNode(0, 0, rootTriangle);

		//test grid
//		for(int i =-2 ;i < 3; i++)
//		{
//			for(int j =-2 ;j < 3; j++)
//			{
//				GameObject temp = (GameObject)Instantiate(trianglePrefab);
//				temp.GetComponent<attraction>().enabled = false;
//				temp.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
//
//				setNode(i, j, temp);
//
//				setCorrectPosition(getNode(i, j));
//			}
//		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	//attatches new triangle to old triangle
	public void connectTriangle(GameObject oldTriangle, GameObject newTriangle)
	{

		triangleNode oldNode = getNode(oldTriangle);

		if(oldNode == null)
		{
			Debug.Log("could not find stationary triangle to attatch triange to");
			return;
		}

		//find the position closest to the new triangle around the possible locations for the new triangle to attatch
		//default to right
		int x = int.MaxValue,y = int.MaxValue;
		float minDist = float.MaxValue;

		Vector2[] possibleoffsets = {new Vector2(-1,0), new Vector2(1,0), new Vector2(0,1)};

		if(isPointingUp(oldNode))
		{
			possibleoffsets[2] = new Vector2(0, -1);
		}

		for(int i=0; i< possibleoffsets.Length; i++)
		{
			if(getNode(oldNode.x + (int)possibleoffsets[i].x, oldNode.y + (int)possibleoffsets[i].y) == null &&(
			   minDist > Vector3.Distance(newTriangle.transform.position, getBasePosition(oldNode.x + (int)possibleoffsets[i].x, oldNode.y + (int)possibleoffsets[i].y)) 
				|| (x == int.MaxValue || y == int.MaxValue)))
			{
				x = oldNode.x + (int)possibleoffsets[i].x;
				y = oldNode.y + (int)possibleoffsets[i].y;
				minDist = Vector3.Distance(newTriangle.transform.position, getBasePosition(x, y));
			}
		}

		if(x == int.MaxValue || y == int.MaxValue)
		{
			Debug.Log("all sides of the base triangle are taken. not able to attatch triangle");
			return;
		}

		//attatch the new triangle to the side of the oldtriangle closest
		setNode(x, y, newTriangle);
		setCorrectPosition(getNode(x, y));
	}

	private triangleNode getNode(int x, int y)
	{
		Vector2 coords = getRealCoords(x, y);

		return grid[(int)coords.x, (int)coords.y];
	}

	private triangleNode getNode(GameObject obj)
	{
		for(int i=0; i<gridSize; i++)
		{
			for(int j=0; j<gridSize; j++)
			{
				if(grid[i, j] != null && grid[i, j].triangleObject == obj)
				{
					return grid[i, j];
				}
			}
		}

		//return null if not found
		return null;
	}

	private void setNode(int x, int y , GameObject obj)
	{
		Vector2 coords = getRealCoords(x, y);

		grid[(int)coords.x ,(int)coords.y] = new triangleNode(obj, x, y);
	}

	private void setNode(Vector2 coords , GameObject obj)
	{
		setNode((int)coords.x, (int)coords.y, obj);
	}

	private Vector2 getRealCoords(int x_, int y_)
	{
		int x = gridSize/2 + x_;
		int y = gridSize/2 - y_;

		return new Vector2(x, y);
	}

	public void setCorrectPosition(GameObject o)
	{
		setCorrectPosition(getNode(o));
	}

	private void setCorrectPosition(triangleNode n)
	{
		Vector3 pos = getBasePosition(n.x, n.y);

		if(!isPointingUp(n))
		{
			pos.y += 0.24f;	
		}

		//if it should be pointing up, rotate by 120, otherwise rotate by 60
		if(isPointingUp(n))
		{
			n.triangleObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 120));
		}
		else
		{
			n.triangleObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 60));				
		}

		n.triangleObject.transform.position = pos;
	}	

	private Vector3 getBasePosition(int x, int y)
	{
		float xOffset = x * 0.415f;
		float yOffset = y * 0.719f;

		return new Vector3(xOffset, yOffset, 0);
	}

	private bool isPointingUp(triangleNode n)
	{
		return isPointingUp(n.x, n.y);
	}

	private bool isPointingUp(int x, int y)
	{
		return Mathf.Abs(x % 2) == Mathf.Abs(y % 2);
	}

	//for debugging purposes
	public string toString()
	{
		string s = "";

		for(int i= gridSize/2; i >= -gridSize/2; i--)
		{
			for(int j= -gridSize/2; j <= gridSize/2; j++)
			{
				triangleNode n = getNode(j, i);
				if(n != null)
				{
					s += "0";
				}
				else
				{
					s += "1";
				}
			}

			s += "\n";
		}

		return s;
	}
}
