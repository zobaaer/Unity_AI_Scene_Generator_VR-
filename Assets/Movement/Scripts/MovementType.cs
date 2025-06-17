using System;
using UnityEngine;

[Serializable]
public abstract class Movement
{
    public abstract string MovementType { get; }
}

[Serializable]
public class LinearOscillatoryMovement : Movement
{
    public float amplitude;
    public float frequency;

    public override string MovementType => "Linear Oscillatory";
}

[Serializable]
public class CircularMovement : Movement
{
    public float radius;
    public float angularSpeed;

    public override string MovementType => "Circular";
}