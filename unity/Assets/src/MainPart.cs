using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Osakana4242 {
	public sealed class MainPart : MonoBehaviour {
		public Data data;

		public TMPro.TextMeshProUGUI progressTextUI;
		public TMPro.TextMeshProUGUI centerTextUI;

		StateMachine<MainPart> sm_;
		List<MyObject> objectList_;
		public ResourceBank resource;
		public GameObject cameraGo;
		public GameObject playerGo;
		public GameObject doorGo;
		public GameObject trainGo;

		public UnityEngine.Tilemaps.Tilemap tilemap1;

		[SerializeField]
		public int playerId;

		public sealed class MyObject : MonoBehaviour {
			public bool hasDestroy;
			public int id;
			public string category;
			public Player player;
			public Stone stone;
			public float time;

			public void Destroy() {
				hasDestroy = true;
			}
		}

		public struct CollisionInfo {
			public Collider collider;
			public Collision collision;
		}

		public class CollilsionObserver : MonoBehaviour {
			public System.Action<CollisionInfo> onEvent;
			public void OnDestroy() {
				onEvent = null;
			}
			public void OnTriggerEnter(Collider collider) {
				if (onEvent == null) return;
				onEvent(new CollisionInfo() {
					collider = collider,
				});
			}
			public void OnCollisionEnter(Collision collision) {
				if (onEvent == null) return;
				onEvent(new CollisionInfo() {
					collision = collision,
				});
			}
		}

		public sealed class Player {
			public int score;
		}

		public sealed class Stone {
			public int score = 1;
			public Vector3 startPosition;
			public Vector3 targetPosition;
			public float duration = 2f;
			public bool hasHit;
		}

		void Awake() {
			sm_ = new StateMachine<MainPart>(stateInit_g_);
			objectList_ = new List<MyObject>();
			Application.logMessageReceived += OnLog;
			//var tilemap2 = GameObject.Instantiate(tilemap1, tilemap1.transform.position + new Vector3(5, 0, 0), Quaternion.identity, tilemap1.transform.parent);
			doorGo.SetActive(true);
		}

		public void OnLog(string condition, string stackTrace, LogType type) {
			switch (type) {
				case LogType.Exception:
				Debug.Break();
				GameObject.Destroy(gameObject);
				Application.Quit();
				break;
			}
		}

		void OnDestroy() {
			Application.logMessageReceived -= OnLog;
			sm_ = null;
			objectList_ = null;
		}

		float trainSpeed;
		float trainTargetSpeed;
		TrainState trainState;
		float trainTargetX = 100f;
		float trainTime;
		enum TrainState {
			CLOSE1,
			CLOSE2,
			CLOSE3,
			RUN,
			OPEN1,
			OPEN2,
			OPEN3,
		}

		void FixedUpdate() {
			if (data.isPlaying) {
			}

			var trainPos = trainGo.transform.position;
			var prevTrainState = trainState;
			switch (trainState) {
				case TrainState.CLOSE1:
					if (0.5f <= trainTime) {
						trainState = TrainState.CLOSE2;
					}
					break;
				case TrainState.CLOSE2:
					if (trainTime == 0f) {
						doorGo.GetComponent<Animator>().Play("close");
					}
					if (1f <= trainTime) {
						trainState = TrainState.CLOSE3;
					}
					break;
				case TrainState.CLOSE3:
					trainState = TrainState.RUN;
					break;
				case TrainState.RUN:
					var dist = trainTargetX - trainPos.x;
					if ( 15f < dist ) {
						trainTargetSpeed = 3f;
					} else {
						trainTargetSpeed = 0.1f;
					}
					trainSpeed = Mathf.MoveTowards( trainSpeed, trainTargetSpeed, 0.1f * Time.deltaTime );
					if (dist == 0f) {
						trainState = TrainState.OPEN1;
					}
					break;
				case TrainState.OPEN1:
					if (1f <= trainTime) {
						trainState = TrainState.OPEN2;
					}
					break;
				case TrainState.OPEN2:
					if (trainTime == 0f) {
						doorGo.GetComponent<Animator>().Play("open");
					}
					if (1f <= trainTime) {
						trainState = TrainState.OPEN3;
					}
					break;
				case TrainState.OPEN3:
					break;
			}
			trainTime += Time.deltaTime;
			if (prevTrainState != trainState) {
				trainTime = 0f;
			}
			trainPos.x = Mathf.MoveTowards( trainPos.x, trainTargetX, trainSpeed * Time.deltaTime );
			trainGo.transform.position = trainPos;
			// {
			// 	var rb = playerGo.GetComponent<Rigidbody2D>();
			// 	// playerGo.OnCollisionEnter2DAsObservable().Subscribe(_col => {
			// 	// 	UnityEngine.Tilemaps.Tilemap tilemap;
			// 	// 	tilemap.

			// 	// });
			// 	var v = rb.velocity;
			// 	v.x = 2f;
			// 	if (hasJump_) {
			// 		v.y = 4f;
			// 		hasJump_ = false;
			// 	}
			// 	rb.velocity = v;
			// }

			for (var i = objectList_.Count - 1; 0 <= i; i--) {
				var obj = objectList_[i];
				{
					var player = obj.player;
					if (player != null) {
						UpdatePlayer(obj, player);
					}
				}
				{
					var stone = obj.stone;
					if (stone != null) {
					}
				}
				obj.time += Time.deltaTime;
			}


			for (var i = objectList_.Count - 1; 0 <= i; i--) {
				var obj = objectList_[i];
				if (!obj.hasDestroy) continue;
				objectList_.RemoveAt(i);
				GameObject.Destroy(obj.gameObject);
			}

			if (data.isPlaying) {
				data.time += Time.deltaTime;
			}
		}

		void UpdatePlayer(MyObject obj, Player player) {
		}

		void Update() {
			sm_.Update(this);
		}

		public MyObject FindObjectById(int id) {
			foreach (var item in objectList_) {
				if (item.id == id) return item;
			}
			return null;
		}

		public MyObject GetPlayer() {
			return FindObjectById(playerId);
		}
		int autoincrement;
		public int CreateObjectId() {
			return ++autoincrement;
		}


		static StateMachine<MainPart>.StateFunc stateExit_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter: {
						self.data.isPlaying = false;
						UnityEngine.SceneManagement.SceneManager.LoadScene("main");
						return null;
					}
				default:
				return null;
			}
		};

		static StateMachine<MainPart>.StateFunc stateInit_g_ = (_evt) => {
			switch (_evt.type) {
				case StateMachineEventType.Enter: {
						var self = _evt.owner;
						self.progressTextUI.text = "";
						self.centerTextUI.text = "READY";

						{
							var player = new GameObject().AddComponent<MyObject>();
							player.player = new Player();
							player.id = player.GetInstanceID();
							self.objectList_.Add(player);
							self.playerId = player.id;
						}

						{
							Random.InitState(1);
						}
						return null;
					}
				case StateMachineEventType.Update: {
						if (1f <= _evt.sm.time) {
							return stateMain_g_;
						}
						return null;
					}

				default:
				return null;
			}
		};

		bool hasJump_;

		static StateMachine<MainPart>.StateFunc stateMain_g_ = (_evt) => {
			var self = _evt.owner;
			// self.StepWave();
			self.data.isPlaying = true;

			var player = self.GetPlayer();

			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("SCORE: {0:F0}\n", player.player.score);
				// sb.AppendFormat("TIME: {0:F2}\n", self.data.RestTime);
				self.progressTextUI.text = ""; // sb.ToString();
			}
			{
				self.centerTextUI.text = "";
			}

			var hasTimeOver = self.data.RestTime <= 0f;
			if (hasTimeOver) {
				return stateTimeOver_g_;
			}

			if (Input.GetKeyDown(KeyCode.Z)) {
				self.hasJump_ = true;
			}

			{
				var pl = self.playerGo.GetComponent<PlayerController>();
				pl.ManualUpdate();
				// playerGo.OnCollisionEnter2DAsObservable().Subscribe(_col => {
				// 	UnityEngine.Tilemaps.Tilemap tilemap;
				// 	tilemap.

				// });
				// var v = rb.velocity;
				// v.x = 2f;
				// if (hasJump_) {
				// 	v.y = 4f;
				// 	hasJump_ = false;
				// }
				// rb.velocity = v;
			}


			if (Input.GetKeyDown(KeyCode.R)) {
				return stateExit_g_;
			}

			return null;
		};

		/** タイムオーバー */
		static StateMachine<MainPart>.StateFunc stateTimeOver_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "TIME OVER";
				self.data.isPlaying = false;
				break;
			}

			if (3f <= _evt.sm.time) {
				return stateResult_g_;
			}

			return null;
		};

		/** 落下 */
		static StateMachine<MainPart>.StateFunc stateFall_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "FALL";
				self.data.isPlaying = false;
				self.cameraGo.GetComponent<CameraController>().target = null;
				break;
			}

			if (3f <= _evt.sm.time) {
				return stateResult_g_;
			}

			return null;
		};

		static StateMachine<MainPart>.StateFunc stateResult_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "PRESS Z KEY";
				self.data.isPlaying = false;
				self.cameraGo.GetComponent<CameraController>().target = null;
				break;
			}

			if (Input.GetKeyDown(KeyCode.Z)) {
				return stateExit_g_;
			}

			return null;
		};

		[System.Serializable]
		public class Data {
			public bool isPlaying;
			/** 経過時間 */
			public float time;
			/** 制限時間 */
			public float duration = 90f;
			/** 走行距離 */
			public float distance;
			public float speed;
			public float speedMax;
			public float RestTime => Mathf.Max(0f, duration - time);
		}

	}
}
