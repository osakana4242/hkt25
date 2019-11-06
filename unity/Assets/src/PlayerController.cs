using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Osakana4242 {
	public class PlayerController : MonoBehaviour {

		public float maxSpeed = 7;
		public float jumpTakeOffSpeed = 7;

		public PhysicsObject physics;

		void Awake() {
		}

		public void ManualUpdate() {
			Vector2 move = Vector2.zero;

			move.x = Input.GetAxis("Horizontal");

			if (Input.GetKeyDown(KeyCode.Z) && physics.grounded) {
				physics.velocity.y = jumpTakeOffSpeed;
			} else if (Input.GetKeyUp(KeyCode.Z)) {
				if (physics.velocity.y > 0) {
					physics.velocity.y = physics.velocity.y * 0.5f;
				}
			}

			physics.targetVelocity = move * maxSpeed;
		}
	}
}