﻿using UnityEngine;
using System.Collections;

/**
 * Global variables that the game needs to be able to access from anywhere
 */
public class GlobalFlags : MonoBehaviour {

	public static bool canFire = true;

	public static int level = 1; 

	public static int score = 0;

	public static int baseScoreValue = 100;

	public static int multiplier = 1;

	public static int queueBonus = 100;

	public static int queueBonusTotal = 0;

	public static bool paused = false;

	/*
	 * Set for if you want the level to be randomly generated or not
	 */
	public static bool isRandLevel = false;

	/*
	 * Called when the user selects they want to play random levels
	 */
	public static void setRandLevel(bool rLevel) {
		isRandLevel = rLevel;
	}

	/*
	 * Used by the level parser to know if it should be parsing a level that's random, or a specific one. 
	 */
	public static bool getRandLevel() {
		return isRandLevel;
	}

	/**
	 * Made so that when the level is beaten any script can call this and change it to the next level
	 */
	public static void setLevel(int num) {
		level = num;
	}

	/*
	 * For any script that needs to know the current level the user is on
	 */
	public static int getLevel() {
		return level;
	}

	public static void setScore(int newscore){
		score = newscore; 
	}

	public static int getScore(){
		return score;
	}

	public static int getBaseScoreValue()
	{
		return baseScoreValue;
	}

	public static int getMultiplier()
	{
		return multiplier;
	}

	public static void incrementMultiplier()
	{
		multiplier++;
	}

	public static void resetMultiplier()
	{
		multiplier = 1;
	}

	public static int getQueueBounus()
	{
		return queueBonus;
	}

	public static int getQueueBounusTotal()
	{
		return queueBonusTotal;
	}

	public static void setQueueBonusTotal(int i)
	{
		queueBonusTotal = i;
	}

	public static bool getPaused(){
		return paused;
	}

	public static void setPaused(bool p){
		paused = p;
	}
}
