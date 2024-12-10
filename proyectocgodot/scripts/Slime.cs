using Godot;


public partial class Slime : Node2D
{
	[Export] private int speed = 60;
	private int direction = 1;
	private RayCast2D rayCastRight;
	private RayCast2D rayCastLeft;
	private CollisionShape2D hitBox;
	private AnimatedSprite2D sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		rayCastRight = GetNode<RayCast2D>("RayCastRight");
		rayCastLeft = GetNode<RayCast2D>("RayCastLeft");
		sprite = GetNode<AnimatedSprite2D>("Sprite");
		hitBox = GetNode<CollisionShape2D>("PhysicsCollider");
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

	private void OnBulletEntered(Node area2D)
	{
		GD.Print("Detectado colision" + area2D.GetGroups());
		if (area2D.IsInGroup("Bullet"))
		{
			Die();
		}
	}

	//Ejecuta la funcion que mata al slime y lo borra de la escena
	private async void Die()
	{
		//Quita la hitbox para que no pueda hacer daño una vez esta muriendo
		hitBox.QueueFree();
		//Para que no pueda recibir otra bala
		GD.Print("Die entered");
		sprite.Play("dying");
		//Para que deje de moverse cuando muere
		speed = 0;
		
		sprite.Play("diying");
		sprite.AnimationFinished += OnAnimationFinished;
		

	}
	private void OnAnimationFinished()
	{
		if (sprite.Animation == "dying")
		{
			QueueFree();
		}
	}


}
