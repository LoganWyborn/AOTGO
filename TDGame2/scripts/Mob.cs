using Godot;
using System;

public partial class Mob : Area2D
{
	[Signal]
	public delegate void MobKilledEventHandler(int gold);
	[Signal]
	public delegate void MobEscapedEventHandler(int damage);
	[Export]
	public float speed = 60f;
	[Export]
	public int maxHitPoints = 4;
	public int hitPoints;
	public int goldValue;
	public float progressPx = 0f;
	public float progressRatio = 0f;
	private float facing = -99f;

/*
	public Mob(float speed, int maxHitPoints, int goldValue)
	{
		this.speed = speed;
		this.maxHitPoints = maxHitPoints;
		this.goldValue = goldValue;
	}
*/
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Mob enters scene tree");
		hitPoints = maxHitPoints;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(progressRatio >= 1)
		{
			GD.Print("Mob escaped!");
			EmitSignal(SignalName.MobEscaped, 1);
			QueueFree();
		}
		else
		{
			progressPx += speed * (float)delta;
			PathFollow2D follower = GetNode<PathFollow2D>("../MobPath/MobPathFollower");
			follower.Progress = progressPx;
			this.progressRatio = follower.ProgressRatio;
			this.Position = follower.Position;
			
			if(facing != follower.Rotation)
			{
				facing = follower.Rotation;
				if(-3 * Mathf.Pi / 4 < facing && facing < -Mathf.Pi / 4)
					GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("up");
				else if(Mathf.Pi / 4 < facing && facing < 3 * Mathf.Pi / 4)
					GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("down");
				else if(-Mathf.Pi / 4 <= facing && facing <= Mathf.Pi / 4)
					GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("right");
				else
					GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("left");
			}
		}
	}
	
	private void OnBodyEntered(Node2D body)
	{
		if(body is Projectile proj)
		{
			GD.Print("Mob body entered by projectile");
			hitPoints -= proj.damage; 
			proj.QueueFree();
		}
		else if(body is Sword sword && sword.Visible)
		{
			GD.Print("Mob body entered by sword");
			hitPoints -= sword.GetParent<MeleeTower>().AttackDamage;
		}
		if(hitPoints <= 0)
		{
			EmitSignal(SignalName.MobKilled, goldValue);
			QueueFree();
		}
		else
			GetNode<ColorRect>("RedBar").Scale = new Vector2(1f - (float)hitPoints/maxHitPoints, 1);
	}
}
