﻿using UnityEngine;
using System.Collections;

/**
 * Level parser will parse a level file, depending on what is passed to it during construction
 * And contrains methods for gathering information from the file to help build levels,
 * whether it be needed for the level itself or the queue that is needed to beat the level. 
 *
 * Note about level file: The first line is the first letter of the triangles in the queue. B = blue, G = Green
 * R = red, etc. The next lines are all locations of the triangles on the grid itself, followed by what colour that 
 * triangle is. 
 */
public class LevelParser : MonoBehaviour {
	
	/**
	 * Represents the triangle of a given line in the level file. Will also contain information about what is next in the queue.
	 */
	public struct TriInfo {
		int x;
		int y;
		string colour;
		
		public TriInfo(int x, int y, string colour) {
			this.x = x;
			this.y = y;
			this.colour = colour;
		}
		
		public void setX(int xVal) {
			x = xVal;
		}
		public int getX() {
			return x;
		}
		public void setY(int yVal) {
			y = yVal;
		}
		public int getY() {
			return y;
		}
		public void setColour(string colourVal) {
			colour = colourVal;
		}
		public string getColour() {
			return colour;
		}
	}
	
	private TriInfo [] triArray;
	
	private string [] queueTris;
	
	/*
	 * tuning paramater for how far in each direction randomly generated triangles can go to.
	 */
	public static readonly int RANDOM_LENGTH = 4;
	
	
	
	/**
	 * This has to be awake, as the levels have to be parsed before everything else calls the data
	 * from the file. Depending on if the RandomLevel flag is flipped in the @GlobalFlag.cs class, 
	 * then a level will be generated randomly and without the use of a file.
	 */
	void Awake() {
		bool randomLevel = GlobalFlags.getRandLevel ();
		
		if(!randomLevel) {
			//TODO Once we have levels being set globally, get the level here from the global variables. 
			try {
				
				string[] lines = System.IO.File.ReadAllLines("assets/levels/level" + 1  + ".txt");
				
				triArray = new TriInfo[lines.Length - 1]; //first line of the level file is the queue for that level
				queueTris = lines[0].Split(',');
				for(int i=1; i< lines.Length; i++) {
					int commaIndex = lines[i].IndexOf(',');
					int colonIndex = lines[i].IndexOf(':');
					int x = int.Parse(lines[i].Substring(0, commaIndex));
					int y = int.Parse(lines[i].Substring(commaIndex + 1, colonIndex - 1 - commaIndex));
					string colour = lines[i].Substring(colonIndex + 1, lines[i].Length - 1 - colonIndex);
					
					TriInfo triInfo = new TriInfo(x, y, colour);
					
					triArray[i - 1] = triInfo; // first "i" was for the queue
				}
			}
			catch {
				Debug.Log("error reading file level" + 1 + ".txt");
			}
		} 
		else {
			
			//Loads a random level into the Triangle array
			int negRandY = -4;//(int) Random.Range(1,RANDOM_LENGTH) * - 1;
			int randY = 4;//(int) Random.Range(1,RANDOM_LENGTH);
			TriInfo [] temp = new TriInfo[(RANDOM_LENGTH + Mathf.Abs(RANDOM_LENGTH) + 1) * (RANDOM_LENGTH + Mathf.Abs(RANDOM_LENGTH) + 1) - 1];
			
			//used to help make sure there is no complete cluster in the random level
			string [,] testComplete = new string[RANDOM_LENGTH * 2 + 1, RANDOM_LENGTH * 2 + 1];
			
			int arrLoc = 0;
			int yLocTri = 0; //x location of the triangle in an array
			for(int i = negRandY; i <= randY; i++) {
				int negRandX = -4;//(int) Random.Range(1,RANDOM_LENGTH) * - 1;
				int randX = 4;//(int) Random.Range(1,RANDOM_LENGTH);
				
				int xLocTri = 0; //y location  of the triangle in an array
				for(int j = negRandX; j <= randX; j++) {
					if(i == 0 && j == 0) {
						testComplete[xLocTri,yLocTri] = "black";
						xLocTri++;
						continue;
					}
					
					string rColour = randomColour();
					testComplete[xLocTri,yLocTri] = rColour;
					while(hasCompleteTriangle(testComplete, false, xLocTri, yLocTri)){
						rColour = randomColour();
						testComplete[xLocTri,yLocTri] = rColour;
					}
					
					
					TriInfo triInfo = new TriInfo(j, i, rColour);
					temp[arrLoc] = triInfo;
					arrLoc++;
					xLocTri++;
				}
				yLocTri++;
			}
			
			triArray = new TriInfo[arrLoc];
			System.Array.Copy(temp, 0, triArray, 0, arrLoc);
			
			//loops over arbitrary number (just picked 20), to load the queue with that many random colours. 
			queueTris = new string[40];
			for(int i = 0; i < queueTris.Length; i++) {
				queueTris[i] = randomColour();
			}
		}
		
	}
	
	/*
	 * Check whether or not the there is a complete triangle in this array. Can be used 2 ways. The first way
	 * is by giving it coordinates of a triangle, and it will test if having just added that triangle, if a cluster
	 * was formed. Ex, pointing up triangle will test against (-2, 0) and (-1, -1), pointing down will test against
	 * (-1, -1) and (1, -1). The second way is by going through the entire list and returning true or false depending on 
	 * if a cluster is formed. 
	 * @param string [] testComp, the array of triangle of the level so far, which will be tested against. 
	 * @param bool wholeArray, flag for if you want to determine if the entire array has a complete set
	 * @param int xLoc, x location of a triangle
	 * @param int yLoc, y location of a triangle
	 */
	public bool hasCompleteTriangle(string [,] testComp, bool wholeArray, int xLoc, int yLoc) {
		bool hasCluster = false;
		
		if (wholeArray) {
			for (int i = 1; i < testComp.GetLength(0); i++) {
				for (int j = 1; j < testComp.GetLength(1); j++) {
					bool isTriUp = isPointingUp(xLoc, yLoc);
					if(i == 1 && isTriUp) {
						continue;
					}
					else if(!isTriUp && i == testComp.GetLength(0) - 1){
						continue;
					}
					else {
						if(isTriUp) {
							string colour1 = testComp[i - 2, j];
							string colour2 = testComp[i - 1, j - 1];
							string colour3 = testComp[i, j];
							
							hasCluster = string.Compare(colour1, colour2) == 0 && string.Compare(colour1, colour3) == 0;
						} 
						else {
							string colour1 = testComp[i - 1, j - 1];
							string colour2 = testComp[i + 1, j - 1];
							string colour3 = testComp[i, j];
							
							hasCluster = string.Compare(colour1, colour2) == 0 && string.Compare(colour1, colour3) == 0;
						}
					}
				}
			}
		}
		else {
			bool isTriUp = isPointingUp(xLoc, yLoc);
			if (xLoc == 0 || yLoc == 0) {//nothing to test against
				hasCluster = false;
			}
			else if(xLoc == 1 && isTriUp) {//nothing to test against
				hasCluster = false;
			}
			else if(!isTriUp && xLoc == testComp.GetLength(0) - 1){
				hasCluster = false;
			}
			else {//test for clusters
				if(isTriUp) {
					string colour1 = testComp[xLoc - 2, yLoc];
					string colour2 = testComp[xLoc - 1, yLoc - 1];
					string colour3 = testComp[xLoc, yLoc];
					
					hasCluster = string.Compare(colour1, colour2) == 0 && string.Compare(colour1, colour3) == 0;
				} 
				else {
					string colour1 = testComp[xLoc - 1, yLoc - 1];
					string colour2 = testComp[xLoc + 1, yLoc - 1];
					string colour3 = testComp[xLoc, yLoc];
					
					hasCluster = string.Compare(colour1, colour2) == 0 && string.Compare(colour1, colour3) == 0;
				}
			}
			
		}
		return hasCluster;
	}
	
	/**
	 * Gives a random colour that a triangle can have. If new colours are added, this function needs to be updated
	 * to attribute for this. Current triangles can be: blue, aqua, green pink, red, yellow.
	 */
	public string randomColour() {
		int thisIsTheMostPointlesslyLongVariableNameThatHasNothingToDoWithWhatItIsUsedFor = (int) Random.Range (1, 6);
		string returnColour = "b"; //default colour blue
		
		if(thisIsTheMostPointlesslyLongVariableNameThatHasNothingToDoWithWhatItIsUsedFor == 1) {
			returnColour = "r";
		}
		else if(thisIsTheMostPointlesslyLongVariableNameThatHasNothingToDoWithWhatItIsUsedFor == 2) {
			returnColour = "g";
		}
		else if(thisIsTheMostPointlesslyLongVariableNameThatHasNothingToDoWithWhatItIsUsedFor == 3) {
			returnColour = "a";
		}
		else if(thisIsTheMostPointlesslyLongVariableNameThatHasNothingToDoWithWhatItIsUsedFor == 4) {
			returnColour = "p";
		}
		else if(thisIsTheMostPointlesslyLongVariableNameThatHasNothingToDoWithWhatItIsUsedFor == 5) {
			returnColour = "y";
		} 
		
		return returnColour;
	}
	
	/**
	 * Function for determining if a triangle is pointing up or not
	 * @param x the x value of the triangle
	 * @param y, the y value of the triangle
	 */
	private bool isPointingUp(int x, int y) {
		return Mathf.Abs(x % 2) == Mathf.Abs(y % 2);
	}
	
	/**
	 * Gets the array that contains the colour of each triangle and the location in x y coordinates 
	 */
	public TriInfo [] getTriArray() {
		return triArray;
	}
	
	/**
	 * Gets the array containing all of the colours for the queue, represented by letters
	 */
	public string [] getQueueArray() {
		return queueTris;
	}
	
	public void CreateLevelFile(string levelName,string[] queue,string[,] grid)
	{
		string filename = "assets/levels/UserMade/" + levelName + ".txt";
		if(System.IO.File.Exists(filename))
		{
			System.IO.File.Delete(filename);
		}
		//System.IO.File.Create(filename);
		
		using (System.IO.StreamWriter sw = System.IO.File.AppendText(filename))
		{
			for(int i = 0; i < queue.Length; i++)
			{
				if(i != 0 )
				{
					sw.Write(",");
				}
				sw.Write(queue[i]);
			}
			
			int size = (int)Mathf.Sqrt(grid.Length);
			for(int i = 0; i < size; i++)
			{
				for(int j = 0; j < size; j++)
				{
					if(!grid[i,j].Equals(""))
					{
						int x = i - size/2;
						int y = size/2 - j;
						
						if(!(x==0 && y==0))
						{
							string tri = "\n" + x + "," + y + ":" + grid[i,j];
							sw.Write(tri);
						}
					}
				}
			}
		}
	}
	
}
