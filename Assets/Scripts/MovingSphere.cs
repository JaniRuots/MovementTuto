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
	//bool onGround;
	int groundContactCount, stepsSinceLastJump;
	bool OnGround => groundContactCount > 0;
	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;
	[SerializeField, Range(0, 5)]
	int maxAirJumps = 0;
	int jumpPhase;
	[SerializeField, Range(0f, 90f)]
	float maxGroundAngle = 25f;
	float minGroundDotProduct;
	Vector3 contactNormal;
	int stepsSinceLastGrounded;
	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;
	[SerializeField, Min(0f)]
	float probeDistance = 1f;
	[SerializeField]
	LayerMask probeMask = -1;

	Vector3 ProjectOnContactPlane (Vector3 vector) {
		return vector - contactNormal * Vector3.Dot(vector, contactNormal);
	}
	bool SnapToGround () {
		if (stepsSinceLastGrounded > 1 ||stepsSinceLastJump >= 2) {
			return false;
		}
		float speed = velocity.magnitude;
		if (speed > maxSnapSpeed) {
			return false;
		}
		if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask)) {
			return false;
		}
		if (hit.normal.y < minGroundDotProduct) {
			return false;
		}
		groundContactCount = 1;
		contactNormal = hit.normal;
		float dot = Vector3.Dot(velocity, hit.normal);
		if (dot > 0f) {
			velocity = (velocity - hit.normal * dot).normalized * speed;
		}
		return true;
	}


	void OnValidate () {
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
	}

	void Awake () {
		body = GetComponent<Rigidbody>();
		OnValidate();
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

		// Changing color based on OnGround
		GetComponent<Renderer>().material.SetColor(
			"_Color", OnGround ? Color.black : Color.white
		);
    }

    void FixedUpdate () {
		//velocity = body.velocity;
		UpdateState();
		AdjustVelocity();
		//float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
		//float maxSpeedChange = acceleration * Time.deltaTime;

		//velocity.x =
		//	Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
		//velocity.z =
		//	Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
		if (desiredJump) {
			desiredJump = false;
			Jump();
		}
        body.velocity = velocity;
		//onGround = false;
		ClearState();
        

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

	void ClearState () {
		//onGround = false;
		groundContactCount = 0;
		contactNormal = Vector3.zero;
	}

	void UpdateState () {
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		velocity = body.velocity;
		if (OnGround || SnapToGround()) {
			stepsSinceLastGrounded = 0;
			jumpPhase = 0;
			if (groundContactCount > 1) {
				contactNormal.Normalize();
			}
		}
		else {
			contactNormal = Vector3.up;
		}
	}

	void Jump() {
		if (OnGround || jumpPhase < maxAirJumps) {
			stepsSinceLastJump = 0;
			jumpPhase += 1;
			float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
			float alignedSpeed = Vector3.Dot(velocity, contactNormal);
			if (alignedSpeed > 0f) {
				jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			}
			//velocity.y += jumpSpeed;
			velocity += contactNormal * jumpSpeed;
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
			//onGround |= normal.y >= minGroundDotProduct;
			if (normal.y >= minGroundDotProduct) {
				//onGround = true;
				groundContactCount += 1;
				contactNormal += normal;
			}
		}
	}

	void AdjustVelocity () {
		Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
		Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

		float currentX = Vector3.Dot(velocity, xAxis);
		float currentZ = Vector3.Dot(velocity, zAxis);

		float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}


}
