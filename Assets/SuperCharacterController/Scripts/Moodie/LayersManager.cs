using UnityEngine;
using System.Collections;

public class LayersManager : MonoBehaviour 
{
	public LayerMask collisionLayers;
    public LayerMask walkable;

	public static LayerMask colLayers;
    public static LayerMask walk;


	
	void Awake()
	{
		colLayers = collisionLayers;
        walk = walkable;
	}

}
