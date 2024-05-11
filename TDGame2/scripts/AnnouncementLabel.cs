using Godot;
using System;

public partial class AnnouncementLabel : Label
{
	public bool isSliding = false;
	public float slideSpeed = 20;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(isSliding)
			Position = new Vector2(Position[0], GlobalPosition[1] - (slideSpeed * (float)delta));

	}

	public void StartSlide()
	{
		Show();
		isSliding = true;
		GetNode<Timer>("HideTimer").Start();
	}

	public void StopSlide()
	{
		OnHideTimerTimeout();
	}

	private void OnHideTimerTimeout()
	{
		Hide();
		isSliding = false;
		Position = GetNode<Marker2D>("../AnnouncementMarker").Position;
	}
}
