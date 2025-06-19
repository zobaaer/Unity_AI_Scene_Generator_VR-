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
    public float axisX = 1f;
    public float axisY = 0f;
    public float axisZ = 0f;

    public override string MovementType => "Linear Oscillatory";
}

[Serializable]
public class CircularMovement : Movement
{
    public float radius;
    public float angularSpeed;

    public override string MovementType => "Circular";
}

[Serializable]
public class RotationMovement : Movement
{
    public float axisX;
    public float axisY;
    public float axisZ;
    public float speed;
    public bool clockwise;

    public override string MovementType => "Rotation";
}