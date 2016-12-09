SuperCharacterController v 2.0.0 Unity package, by Erik Ross.

Package is free to use, modify and redistribute, although linking to the original blog where I documented it: http://roystanross.wordpress.com/
would be nice :) Especially since it goes over the creation process.

--------------------------------------------------------------------------

Used to make use of RPGMesh and RPGTriangleTree scripts from the RPGController package written by Unity forums user fholm. (Deprecated in favor of a BSPTree)
His profile can be viewed here: http://forum.unity3d.com/members/59346-fholm
And his package accessed via his GitHub here: https://github.com/fholm/unityassets

SuperStateMachine is a modification of the Finite State Machine built in a tutorial offered by Unity Gems: http://unitygems.com/
The tutorial can be accessed here: http://unitygems.com/fsm1/

--------------------------------------------------------------------------

This is an ongoing project and will be improved in the future, hopefully with better documentation too.

--------------------------------------------------------------------------

CHANGELOG

VERSION	LOG NOTES													CONTRIBUTOR


2.0.0:	- Iterative grounding solver added.							Erik Ross (Iron-Warrior)
		- Grounding data restructured for more intuitive use
		- Collision response is now recursive to ensure
		controller is not colliding with objects at the end of frame
		- RPGMesh deprecated in favour of a BSPTree
		- CapsuleCollider primitive collider is now supported		Moodie
		
1.1.0:	- Recursive grounding method added
	- Support for moving platforms added
	- Cleaned up SuperCollisionType
	
1.0.2: 	- Debug OnGUI tools placed in separate script to avoid		iivo_k 	
	resource hog of empty OnGUI method
	- The RPG classes now use Lists in a few places instead of 
	HashSets, since with the current Mono version in Unity using a 
	collection iterator allocates memory but a for loop does not
	- SuperCharacterCollider creates a List<SuperCollisions> on init 
	and clears it in the Pushback method instead of creating a new 
	List
	- Some inner classes to structs, so they reside in the stack and 
	don't allocate heap memory	
	
1.0.1: 	Fixed issue where ignoring colliders didn’t properly work	Erik Ross (Iron-Warrior)

1.0.0: 	Release								Erik Ross (Iron-Warrior)