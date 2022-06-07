using UnityEngine;

public class MovingSphere : MonoBehaviour
{

	[SerializeField, Range(0f, 100f)]
	float maxSpeed = 10f;
	[SerializeField, Range(0f, 100f)]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;
	// [SerializeField, Range(0f, 1f)]
	// float bounciness = 0.5f;
	// [SerializeField]
	// Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);
    Vector3 velocity, desiredVelocity;
	Rigidbody body;
	// Jump related things here
	bool desiredJump;
	bool onGround;
	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;
	[SerializeField, Range(0, 5)]
	int maxAirJumps = 0;
	int jumpPhase;


	void Awake () {
		body = GetComponent<Rigidbody>();
	}


	void Update () {
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		Vector3 acceleration =
			new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
		desiredVelocity =
			new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
		desiredJump |= Input.GetButtonDown("Jump");
    }

    void FixedUpdate () {
		//velocity = body.velocity;
		UpdateState();
		float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;
		velocity.x =
			Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
		velocity.z =
			Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
		if (desiredJump) {
			desiredJump = false;
			Jump();
		}


        body.velocity = velocity;
		onGround = false;
        
		// Vector3 displacement = velocity * Time.deltaTime;
		// Vector3 newPosition = transform.localPosition + displacement;
		// if (newPosition.x < allowedArea.xMin) {
		// 	newPosition.x = allowedArea.xMin;
		// 	velocity.x = -velocity.x * bounciness;
		// }
		// else if (newPosition.x > allowedArea.xMax) {
		// 	newPosition.x = allowedArea.xMax;
		// 	velocity.x = -velocity.x * bounciness;
		// }
		// if (newPosition.z < allowedArea.yMin) {
		// 	newPosition.z = allowedArea.yMin;
		// 	velocity.z = -velocity.z * bounciness;
		// }
		// else if (newPosition.z > allowedArea.yMax) {
		// 	newPosition.z = allowedArea.yMax;
		// 	velocity.z = -velocity.z * bounciness;
		// }
		// transform.localPosition = newPosition;
	}

	void UpdateState () {
		velocity = body.velocity;
		if (onGround) {
			jumpPhase = 0;
		}
	}

	void Jump() {
		if (onGround || jumpPhase < maxAirJumps) {
			jumpPhase += 1;
			float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
			if (velocity.y > 0f) {
				jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
			}
			velocity.y += jumpSpeed;
		}
	}

	void OnCollisionEnter (Collision collision) {
		//onGround = true;
		EvaluateCollision(collision);
	}

	void OnCollisionStay (Collision collision) {
		//onGround = true;
		EvaluateCollision(collision);
	}
	
	void EvaluateCollision (Collision collision) {
		for (int i = 0; i < collision.contactCount; i++) {
			Vector3 normal = collision.GetContact(i).normal;
			onGround |= normal.y >= 0.9f;
		}
	}



}
