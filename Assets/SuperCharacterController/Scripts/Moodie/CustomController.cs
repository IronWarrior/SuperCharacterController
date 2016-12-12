using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomController : MonoBehaviour 
{
	public bool debugSpheres = true;

	public CapsuleCollider myCollider;
	private int numberOfSpheres = 2;

	public int recursivePushBack = 2;

	private struct CollisionSphere
	{
		public float offset;
		public bool isFeet;
		public bool isHead;
	}

	CollisionSphere feet;
	List<CollisionSphere> collisionSpheres = new List<CollisionSphere> ();

	private int _temporaryLayerIndex; // Index for the temporary layer
	private const string _temporaryLayer = "TempCast"; // A Temporary layer, this layer should be defined in the layer editor
	private const float _tinyTolerance = 0.01f;// A tiny Tolerance value
	private const float _tolerance = 0.05f;// A Tolerance value
	private const float _groundingUpperBoundAngle = 60.0f; //How steep a slope should be until the maximum ratio should be taken to determine if the controller is grounded when standing on a edge
	private const float _groundingMaxPercentFromCenter = 0.95f; //Max Ratio of the controller radius to check if the controller is grounded when standing on a edge
	private const float _groundingMinPercentFromcenter = 0.50f; //Min Ratio of the controller radius to check if the controller is grounded when standing on a edge

	private float radius;

	private float castDistance = 15;

	public float slopeLimit = 60;

	[HideInInspector]
	public Vector3 initialPosition;

	private struct GroundHit
	{
		public Vector3 point;
		public Vector3 normal;
		public float distance;
		public bool isValid;

		public void SetGround(Vector3 point, Vector3 normal, float distance, bool isValid)
		{
			this.point = point;
			this.normal = normal;
			this.distance = distance;
			this.isValid = isValid;
		}
	}

	//The current ground directly below the character
	private GroundHit _currentGround; 
	//If standing on a edge the far ground is the ground furthest to the controller center where the near ground is the ground closest to the controller center
	private GroundHit _farGround;   
	private GroundHit _nearGround;
	//If current ground slope is greater than slope limit than the flushground is the ground found after raycasting down the slope from the controllers position
	private GroundHit _flushGround;

	private bool _isClamping = true;
    private bool slopeLimiting = false;

	void Awake () 
	{
        if(!myCollider)
		    myCollider = transform.GetComponentInChildren<CapsuleCollider>(); //If the controller has its own collider add it here

		SetCollisionParameters();

		_temporaryLayerIndex = LayerMask.NameToLayer(_temporaryLayer); // Set the _tempLayerIndex to the _temporaryLayer "TempCast" 

		ProbeGround();
	}
	
	public void CollideWithWorld()
	{
		for(int i = 0; i < recursivePushBack; i++)
		{
			if(!PushBack())
				break;
		}

		ProbeGround();

        if (slopeLimiting)
		    if(SlopeLimit()) //Limit the controller on a slope
			    ProbeGround(); //If controller was slope limited then gather data about the ground around the controller
		
		if (_isClamping) 
			ClampToGround(); //Clamp the controller to ground in order to traverse uneven ground smoothly
	}

	bool SlopeLimit()
	{
		//Calculate the angle with the current ground first to see if it is greater than slope limit
		Vector3 n = _currentGround.normal;
		float a = Vector3.Angle(n, transform.up);
		
		if (a > slopeLimit)
		{
			//Grab the direction that the controller is moving in
			Vector3 absoluteMoveDirection = Math3d.ProjectVectorOnPlane(n, transform.position - initialPosition);
			
			// Retrieve a vector pointing down the slope
			Vector3 r = Vector3.Cross(n, -transform.up);
			Vector3 v = Vector3.Cross(r, n);
			
			//Check the angle between the move direction of the controller and a vector down the slope. If less than 90 degrees then the player is moving down the slope return false
			float angle = Vector3.Angle(absoluteMoveDirection, v);
			
			if (angle <= 90.0f)
				return false;
			
			// Calculate where to place the controller on the slope, or at the bottom, based on the desired movement distance
            Vector3 resolvedPosition = Math3d.ProjectPointOnLine(initialPosition, r, transform.position);
            Vector3 direction = Math3d.ProjectVectorOnPlane(n, resolvedPosition - transform.position);
			
			transform.position += direction;
			
			return true;
		}
		
		return false;
	}

	void ClampToGround()
	{
		//TODO: This always clamps to current ground, is this fine?
		if(_currentGround.isValid)
			transform.position -= transform.up * _currentGround.distance; // Move the controller down by that distance
		else
		{
		}
	}

	void ProbeGround()
	{
		//Set all grounds to invalid
		ResetGround();
		
		// Reduce our radius by Tolerance squared to avoid failing the SphereCast due to clipping with walls
		float smallerRadius = radius*0.95f;

		RaycastHit hit;
		
		if (Physics.SphereCast(SpherePosition(feet), smallerRadius, -transform.up, out hit, castDistance, LayersManager.walk))
		{
			// By reducing the initial SphereCast's radius by Tolerance, our casted sphere no longer fits with
			// our controller's shape. Reconstruct the sphere cast with the proper radius
			hit = SimulateSphereCast(hit, smallerRadius, SpherePosition(feet), radius);
			
			_currentGround.SetGround(hit.point,hit.normal,hit.distance,true);
			
			// If we are standing on a perfectly flat surface, we cannot be either on an edge
			// On a slope or stepping off a ledge hence return
			if (Vector3.Distance(Math3d.ProjectPointOnPlane(transform.up, transform.position, hit.point), transform.position) < _tinyTolerance)
			{
				return;
			}

			// As we are standing on an edge, we need to retrieve the normals of the two
			// faces on either side of the edge and store them in nearHit and farHit
            Vector3 toCenter = Math3d.ProjectVectorOnPlane(transform.up, (transform.position - hit.point).normalized * _tinyTolerance); // Get a vector pointing to the center of the controller
			
			Vector3 awayFromCenter = Quaternion.AngleAxis(-80.0f, Vector3.Cross(toCenter, transform.up)) * -toCenter; // Get a vector pointing towards the center of the controller
			
			Vector3 nearPoint = hit.point + toCenter*8 + (transform.up * _tinyTolerance); // Calculate a position close to the center of the controller to raycast from to get info about nearground
			Vector3 farPoint = hit.point + (awayFromCenter * 8); // Calculate a position further to the center of the controller to raycast from to get info about the farground
			
			RaycastHit nearHit; //Properties of the nearHit ground
			RaycastHit farHit; //Properties of the farHit ground
			
			Physics.Raycast(nearPoint, -transform.up, out nearHit, castDistance, LayersManager.walk); //Perform nearHit raycast
			Physics.Raycast(farPoint, -transform.up, out farHit, castDistance, LayersManager.walk); //Perform farHit raycast

			_nearGround.SetGround(nearHit.point, nearHit.normal, nearHit.distance,true);
			_farGround.SetGround(farHit.point, farHit.normal, farHit.distance, true);
			
			// If we are currently standing on ground that should be counted as a wall(Greater than slope limit),
			// we are likely flush against it on the ground. Retrieve what we are standing on
			if (Vector3.Angle(hit.normal, -transform.up) > slopeLimit)
			{
				// Retrieve a vector pointing down the slope
				Vector3 r = Vector3.Cross(hit.normal, -transform.up);
				Vector3 v = Vector3.Cross(r, hit.normal);
				
				//Get a position slightly above the controller position to raycast down the slope from to avoid clipping
				Vector3 flushOrigin = hit.point + hit.normal * _tinyTolerance;
				
				// Properties of the flushHit ground
				RaycastHit flushHit;
				
				if (Physics.Raycast(flushOrigin, v, out flushHit, castDistance, LayersManager.walk))//Perform Raycast
				{
					flushHit = SimulateSphereCast(flushHit,0,flushOrigin,radius);
					_flushGround.SetGround(flushHit.point, flushHit.normal, flushHit.distance,true);
				}
			}
		}
	}

	Collider[] overlappingColliders;
	bool PushBack()
	{
		bool isPushedBack = false;
		for(int i =  collisionSpheres.Count - 1; i >= 0 ; i--)
		{
			overlappingColliders = Physics.OverlapSphere(SpherePosition(collisionSpheres[i]),radius,LayersManager.colLayers);
		
			for(int j = 0; j < overlappingColliders.Length; j++)
			{
				if(overlappingColliders[j] != myCollider)
				{
					isPushedBack = true;
					Vector3 position = SpherePosition(collisionSpheres[i]); // The position of the sphere in world coordinates
					Vector3 contactPoint;

					SuperCollider.ClosestPointOnSurface(overlappingColliders[j], position, radius, out contactPoint); // The closest point on the collider, named contact point

					if (contactPoint != Vector3.zero) // Contact point was found
					{
						Vector3 v = contactPoint - position; //The direction from the position of the sphere to the contact point
						
						if (v != Vector3.zero)
						{
							// Cache the collider's layer so that we can cast against it
							int layer = overlappingColliders[j].gameObject.layer;
							
							overlappingColliders[j].gameObject.layer = _temporaryLayerIndex;
							
							// Check which side of the normal we are on
							bool facingNormal = Physics.Raycast(new Ray(position, v.normalized), v.magnitude + _tinyTolerance, 1 << _temporaryLayerIndex);
							
							//Set col layer back to its previous layer
							overlappingColliders[j].gameObject.layer = layer;
							
							// Orient and scale our vector based on which side of the normal we are situated
							if (facingNormal)
							{
								if (Vector3.Distance(position, contactPoint) < radius)
								{
									v = v.normalized * (radius - v.magnitude) * -1; //The distance needed to move the controller in order to push it out of the collider
								}
								else
								{
									// A previously resolved collision has had a side effect that moved us outside this collider
									continue;
								}
							}
							else
							{
								v = v.normalized * (radius + v.magnitude); //The distance needed to move the controller in order to push it out of the collider
							}
							transform.position += v; //Push back controller
							
							overlappingColliders[j].gameObject.layer = layer;
						}
					}
				}
			}
		}

		return isPushedBack;
	}

	//Calculate the position of the sphere collisions in world coordinates
	private Vector3 SpherePosition(CollisionSphere sphere)
	{
		return transform.position + sphere.offset * transform.up;
	}

	private void ResetGround()
	{
		_currentGround.isValid = false;
		_farGround.isValid = false;
		_nearGround.isValid = false;
		_flushGround.isValid = false;
	}

	private RaycastHit SimulateSphereCast(RaycastHit hit, float smallerRadius, Vector3 origin, float radius)
	{
		float groundAngle = Vector3.Angle(hit.normal, transform.up) * Mathf.Deg2Rad;
		float cos = 1/Mathf.Cos(groundAngle);
		
		float hypS = smallerRadius*cos;
		float hypB = radius*cos;
		
		Vector3 circleCenterSmall = hit.point + hit.normal*smallerRadius;
		Vector3 pointOnSurface = circleCenterSmall + hypS*transform.up*-1;
		Vector3 circleCenterBig = pointOnSurface + hypB*transform.up;
		
		hit.distance = Vector3.Distance(origin, circleCenterBig);
		
		if(hit.distance < _tinyTolerance*_tinyTolerance)
			hit.distance = 0;
		
		hit.point = circleCenterBig - hit.normal*radius;
		
		return hit;
	}

	//Set Collider radius and spheres
	private void SetCollisionParameters()
	{
		radius = myCollider.radius*myCollider.transform.localScale.x;
		if(myCollider)
		{
			for(int i = 0; i < numberOfSpheres; i++)
			{
				if(i == 0)
				{
					sphere.isFeet = true;
					sphere.isHead = false;
					sphere.offset = radius;
					feet = sphere;
				}
				else if(i == numberOfSpheres - 1)
				{
					sphere.isFeet = false;
					sphere.isHead = true;
					sphere.offset = (myCollider.height*myCollider.transform.localScale.y - radius);
				}
				else
				{
					sphere.isFeet = false;
					sphere.isHead = false;
					//TODO Add sphere offset if number of spheres is greater than two!
				}
				collisionSpheres.Add(sphere);
			}
		}
		else
		{
			Debug.LogWarning("Please attach a capsule collider to character");
		}
	}

	public bool IsGrounded(float distance)
	{
		//Check if current ground exists and if greater than a set distance the controller is not grounded
		if(!_currentGround.isValid || _currentGround.distance > distance)
		{
			return false;
		}
		
		// Check if we are standing on an edge or slope and if ground is greater than slope limit
		if (_farGround.isValid && Vector3.Angle(_farGround.normal, transform.up) > slopeLimit)
		{
			//Check if distance to flushground if exists is smaller than a set distance, if yes then the controller is grounded
			if (_flushGround.isValid && Vector3.Angle(_flushGround.normal, transform.up) < slopeLimit && _flushGround.distance < distance) 
				return true;
			
			return false;
		}

        // Check if we are at the edge of an edge and the controller is within the limits of standing off the far ground
        if (_farGround.isValid && !OnSteadyGround(_farGround.normal, _currentGround.point))
        {
            //Before setting the controller to not grounded check if the controller is within the limits of standing off the near ground
            if (_nearGround.isValid && _nearGround.distance <= distance)
                return true;

            return false;
        }
		
		return true;
	}
	
	bool OnSteadyGround(Vector3 normal, Vector3 point)
	{
		//Grab the angle of the slope we are checking against
		float angle = Vector3.Angle(normal, transform.up);
		
		//Grab the angle ratio with a set constant (See comment in defining what _groundingUpperBoundAngle is)
		float angleRatio = angle / _groundingUpperBoundAngle;
		//Calculate the distance ratio based on the angle ratio
		float distanceRatio = Mathf.Lerp(_groundingMinPercentFromcenter, _groundingMaxPercentFromCenter, angleRatio);
		//Project the point we are checking with on the same plane of the controller center
		Vector3 p = Math3d.ProjectPointOnPlane(transform.up, transform.position, point);
		//Calculate the distance to the controller center from point p
		float distanceFromCenter = Vector3.Distance(p, transform.position);
		//If distance from center is less than distance ratio times radius then we are on steady ground
		return distanceFromCenter <= distanceRatio * radius;
	}

	public void EnableClamping()
	{
		_isClamping = true;
	}
	
	public void DisableClamping()
	{
		_isClamping = false;
	}

    public void EnableSlopeLimit()
    {
        slopeLimiting = true;
    }

    public void DisableSlopeLimit()
    {
        slopeLimiting = false;
    }
	
//--------------------------------------------------------------------------------------------------------//
	//DEBUG//
	CollisionSphere sphere;
	void OnDrawGizmos()
	{
		if(debugSpheres)
		{
			for(int i = 0; i < collisionSpheres.Count; i++)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(SpherePosition(collisionSpheres[i]), radius);
			}
		}
	}
}


