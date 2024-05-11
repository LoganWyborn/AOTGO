using Godot;
using System;

public partial class TestMob : Area2D
{
	public float progress = 0.99f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position = new Vector2(x: Position.X + (float)delta * 100, y: Position.Y);
	}
}
