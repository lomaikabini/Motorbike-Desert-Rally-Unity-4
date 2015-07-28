using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MyRandom {

	static int count = 15;
	static List<int> mass;

	public static int[] GetValues(int size)
	{
		int[] res = new int[size];
		if(mass == null)
		{
			mass = new List<int>();
			for(int i = 0;i<count;i++)
			{
				mass.Add(i);
			}
		}
		for(int j = 0;j<size;j++)
		{
			int pos = Random.Range(0,mass.Count);
			res[j] = mass[pos];
			mass.RemoveAt(pos);
		}
		foreach (int a in res)
						mass.Add (a);
		return res;
	}
}
