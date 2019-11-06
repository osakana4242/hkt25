using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osakana4242 {
	/**
	 * from: https://learn.unity.com/tutorial/live-session-2d-platformer-character-controller#5c7f8528edbc2a002053b68e
	 */
	public class PhysicsObject : MonoBehaviour {

		public float minGroundNormalY = .65f;
		public float gravityModifier = 1f;

		public Vector2 targetVelocity;
		public bool grounded;
		public Vector2 groundNormal;
		public Rigidbody2D rb2d;
		public Vector2 velocity;
		public ContactFilter2D contactFilter;
		public RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
		public List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);


		public const float minMoveDistance = 0.001f;
		public const float shellRadius = 0.01f;

		void OnEnable() {
			rb2d = GetComponent<Rigidbody2D>();
		}

		void Start() {
			contactFilter.useTriggers = false;
			contactFilter.SetLayerMask(UnityEngine.Physics2D.GetLayerCollisionMask(gameObject.layer));
			contactFilter.useLayerMask = true;
		}

		void FixedUpdate() {
			velocity += gravityModifier * UnityEngine.Physics2D.gravity * Time.deltaTime;
			velocity.x = targetVelocity.x;

			grounded = false;

			Vector2 deltaPosition = velocity * Time.deltaTime;

			Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

			Vector2 move = moveAlongGround * deltaPosition.x;

			Movement(move, false);

			move = Vector2.up * deltaPosition.y;

			Movement(move, true);
		}

		void Movement(Vector2 move, bool yMovement) {
			float distance = move.magnitude;

			if (distance > minMoveDistance) {
				int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
				hitBufferList.Clear();
				for (int i = 0; i < count; i++) {
					hitBufferList.Add(hitBuffer[i]);
				}

				for (int i = 0; i < hitBufferList.Count; i++) {
					Vector2 currentNormal = hitBufferList[i].normal;
					if (currentNormal.y > minGroundNormalY) {
						grounded = true;
						if (yMovement) {
							groundNormal = currentNormal;
							currentNormal.x = 0;
						}
					}

					float projection = Vector2.Dot(velocity, currentNormal);
					if (projection < 0) {
						velocity = velocity - projection * currentNormal;
					}

					float modifiedDistance = hitBufferList[i].distance - shellRadius;
					distance = modifiedDistance < distance ? modifiedDistance : distance;
				}


			}

			rb2d.position = rb2d.position + move.normalized * distance;
		}

	}
}