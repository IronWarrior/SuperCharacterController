using UnityEngine;
using System.Collections;

[System.Serializable]
public class MoverDetails
{
    public bool isWaitFirstTime = false;
    public bool isRandomStartPosition = false;

    public PlatformMoverLocationDetails start;
    public PlatformMoverLocationDetails target;
    public MoverDetailsSpin spin; 	   
}


[System.Serializable]
public class MoverDetailsSpin
{
    public bool isSpinToTarget = false;
    public float spinRevolutions;
    public Vector3 spinAxis = Vector3.up;
}




[System.Serializable]
public class PlatformMoverLocationDetails
{
    public bool toggleOnly = false;

    public float waitTime = 1f;
    public float timeToNextTargetTime = 1f;
    public MoverDetailsEasingType easingToNextTarget = MoverDetailsEasingType.EaseInOut;

    [HideInInspector]
    public Vector3 positon = Vector3.zero;
    [HideInInspector]
    public Quaternion rotation = Quaternion.identity;
}


public enum PlatformMoverState
{
    WaitingAtStart,
    WaitingAtDestination,
    MovingTo,
    MovingFrom    
}

public enum MoverDetailsEasingType
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut
}
