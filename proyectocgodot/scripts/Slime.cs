using Godot;


public partial class Slime : Node2D
{
	[Export] private int speed = 60;
	private int direction = 1;
	private RayCast2D rayCastRight;
	private RayCast2D rayCastLeft;

	private AnimatedSprite2D sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		rayCastRight = GetNode<RayCast2D>("RayCastRight");
		rayCastLeft = GetNode<RayCast2D>("RayCastLeft");
		sprite = GetNode<AnimatedSprite2D>("Sprite");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public override void _Process(double delta)
	{
		if (rayCastRight.IsColliding())
		{
			direction = -1;
			sprite.FlipH = true;
		}

		if (rayCastLeft.IsColliding())
		{
			direction = 1;
			sprite.FlipH = false;
		}

		Position += new Vector2((float)(direction * speed * delta), 0);
	}

	private void OnBulletEntered(Node body)
	{
		if (body is Proyectil)
		{
			Die();
		}
	}

	//Ejecuta la funcion que mata al slime y lo borra de la escena
	private void Die()
	{
		sprite.Play("diying");
		sprite.Connect("animation_finished", this, nameof(OnAnimationFinished));
	}

	private void OnAnimationFinished(string animName)
	{
		if (animName == "dying")
		{
			QueueFree();
		}
	}
}
