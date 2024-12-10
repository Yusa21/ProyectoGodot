using Godot;
public partial class Player : CharacterBody2D
{ 
	[Export]
	public float Speed = 130.0f;
	[Export]
	public float JumpVelocity = -300.0f;
	[Export]
	public Vector2 BulletOffset = new Vector2();
	[Export]
	public float BulletSpeed = 250.0f;

	public bool CanShootBullet = true;
	private bool isDead = false;
	
	
	private AnimatedSprite2D animation;
	private PackedScene bullet;

	public override void _Ready()
	{
		base._Ready();
		animation = GetNode<AnimatedSprite2D>("Sprite");
		bullet = GD.Load<PackedScene>("res://scenes/proyectil.tscn");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		
		// Si esta muerto no funciona
		if (isDead)
		{
			return;
		}

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_up") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			animation.Play("jump");
		}
		
		//Handle Fire
		if (Input.IsActionPressed("Fire") && CanShootBullet)
		{
			float isFlipped = 1.0f;
			//GD.Print("Fire");
			Proyectil instBullet = (Proyectil) bullet.Instantiate();
			if(animation.FlipH) isFlipped = -1.0f;
			instBullet.Speed = isFlipped * BulletSpeed;
			//instBullet.Position = new Vector2(0, -10);
			instBullet.GlobalPosition = GlobalPosition + new Vector2(BulletOffset.X * isFlipped, BulletOffset.Y);
			GetTree().Root.AddChild(instBullet);
			
			//Activates bullet cooldown
			GetNode<Timer>("BulletCoolDown").Start();
			CanShootBullet = false;




		}
		
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			animation.FlipH = direction.X < 0;
			animation.Play("walking");



		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			animation.Play("idle");
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	public void OnBulletCooldownTimeOut()
	{
		//Cuando acabe el cooldown puede volver a disparar
		CanShootBullet = true;

	}

	//Comprueba si el cuerpo de un enemigo ha entrado en el area
	private void OnEnemyEntered(Node body)
	{
		GD.Print("Entered" + body.Name);
		if (body.IsInGroup("Enemy"))
		{
			Die();
		}
		
	}

	//Realiza lo necesario para que el personaje muera
	private void Die()
	{
		GD.Print("Enters Die");
		isDead = true;
		//Pierdes todad las monedas
		GlobalScript.coins = 0;
		animation.Play("dying");
		animation.AnimationFinished += OnAnimationFinished;
		
	}
	
	private void OnAnimationFinished()
	{
		if (animation.Animation == "dying")
		{
			GetTree().ReloadCurrentScene();
		}
	}
}
