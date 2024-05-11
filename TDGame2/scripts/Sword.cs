using Godot;
using System;
using System.Runtime;

public partial class Sword : RigidBody2D
{
	[Signal]
	public delegate void SwordRotationSetEventHandler();

	public float swingAimOffset = 0.5f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);
		MeleeTower parent = GetParent<MeleeTower>();
		if(parent.Upgrades.Specialization == 2)
		{
			state.AngularVelocity = parent.swingSpeed;
		}
		else if(IsInstanceValid(parent.target) && !Visible)
		{
			PointAtTarget(state, parent.target.Position);
		}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}

	public void PointAtTarget(PhysicsDirectBodyState2D state, Vector2 targetPosition)
	{
		state.Transform = state.Transform.RotatedLocal(state.Transform.Origin.AngleToPoint(targetPosition) - state.Transform.Rotation - swingAimOffset);
		EmitSignal(SignalName.SwordRotationSet);
	}
}
