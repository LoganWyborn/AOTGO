using Godot;
using System;

public partial class Projectile : RigidBody2D
{
	[Export]
	public float speed = 200;
	public int damage = 0;
	public Mob? target;
	public float targetDir;
	public float lifetime = -9999f;
	
	public Projectile()
	{}
	
	public Projectile(Mob target, int damage)
	{
		this.target = target;
		this.damage = damage;
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Projectile in Ready");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(lifetime > 0)
			lifetime -= (float)delta;

		if(!IsInstanceValid(target) && lifetime < 0)
		{
			QueueFree();
		}
	}

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);
		if(lifetime > 0)
		{
			state.LinearVelocity = speed * Vector2.Right.Rotated(targetDir);
		}
		else
		{
			state.LinearVelocity = speed * Position.DirectionTo(target.Position);
		}
    }
}
