using Godot;
using System;

public partial class MobPropsContainer : Godot.GodotObject
{
	public float speed = 60f;
	public int maxHitPoints = 4;
	public int goldValue;
    public Color Modulate;
    public MobPropsContainer()
    {

    }
    public MobPropsContainer(float speed, int maxHitPoints, int goldValue, Color Modulate)
    {
        this.speed = speed;
        this.maxHitPoints = maxHitPoints;
        this.goldValue = goldValue;
        this.Modulate = Modulate;
    }
}