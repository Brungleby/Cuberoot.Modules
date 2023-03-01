
/** WalkMovementModule.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using InputContext = UnityEngine.InputSystem.InputAction.CallbackContext;

#endregion

namespace Cuberoot.Modules
{
	#region WalkModuleBase

	/// <summary>
	/// Base class for walk movement.
	///</summary>

	public abstract class WalkModuleBase
	<TRigidbody, TPawn, TFrictionModule, TGroundSensor, TMovementSpace, TInputSpace> :

	MovementModule_RequirePawn
	<TPawn>

	where TFrictionModule : Module
	where TGroundSensor : Module
	where TMovementSpace : unmanaged
	where TInputSpace : unmanaged
	{
		#region (enum) WalkState

		public enum EWalkState
		{
			/// <summary>
			/// For use when the player is touching low-angled ground and fully in control of their movement.
			///</summary>

			[Tooltip("For use when the player is touching low-angled ground and fully in control of their movement.")]

			Grounded,

			/// <summary>
			/// For use when the player is touching high-angled ground, but not in full control of their movement.
			///</summary>

			[Tooltip("For use when the player is touching high-angled ground, but not in full control of their movement.")]

			Sliding,

			/// <summary>
			/// For use when the player is not touching any ground.
			///</summary>

			[Tooltip("For use when the player is not touching any ground.")]

			Airborne
		}

		#endregion

		#region Public Fields

		[Header("Components")]

		#region (field) ViewTransform

		/// <summary>
		/// The relative transform to calculate input direction from.
		///</summary>

		[Tooltip("The relative transform to calculate input direction from. It is recommended that this is set to the transform of the camera rig (not necessarily the camera itself).")]
		[SerializeField]

		private Transform _ViewTransform;

		/// <inheritdoc cref="_ViewTransform"/>

		public virtual Transform ViewTransform => _ViewTransform;

		#endregion

		[Header("Base Attributes")]

		#region (field) MaxWalkSpeed

		/// <summary>
		/// The maximum speed this character walks. When using analog inputs, the character can move slower than, but never faster than, this value.
		///</summary>

		[InspectorName("Walk Speed")]
		[Tooltip("The maximum speed this character walks. When using analog inputs, the character can move slower than, but never faster than, this value.")]
		[Min(0f)]
		[SerializeField]

		public float MaxWalkSpeed = 5f;

		#endregion
		#region (field) WalkAcceleration

		/// <summary>
		/// How quickly this character accelerates to reach their <see cref="MaxWalkSpeed"/>.
		///</summary>

		[InspectorName("Walk Acceleration")]
		[Tooltip("How quickly this character accelerates to reach their Walk Speed. This value is actually a multiplier for Friction.DefaultStrength.")]
		[Min(1f)]
		[SerializeField]

		public float WalkAcceleration = 2f;

		#endregion

		[Header("Fine-Tuning")]

		#region (field) EnableExcessWalkSpeedInAir

		/// <summary>
		/// If enabled, the pawn will be able to travel beyond <see cref="MaxWalkSpeed"/> whilst not on solid ground.
		///</summary>

		[Tooltip("If enabled, the pawn will be able to travel beyond MaxWalkSpeed whilst not on solid ground.")]
		[SerializeField]

		public bool EnableExcessWalkSpeedInAir = false;

		#endregion

		#region (field) AccelerationCurve

		/// <summary>
		/// This curve defines how strong the acceleration should be given our current <see cref="Speed"/>.
		///</summary>

		[Tooltip("This curve defines how strong the acceleration should be given our current Speed.")]
		[SerializeField]

		protected AnimationCurve AccelerationCurve = AnimationCurve.Constant(0f, 1f, 1f);

		#endregion
		#region (field) SlopeSpeedCurve

		/// <summary>
		/// This curve defines what percentage of our <see cref="MaxWalkSpeed"/> should be utilized when walking on an upward or downward slope.
		///</summary>

		[Tooltip("This curve defines what percentage of our MaxWalkSpeed should be utilized when walking on an upward or downward slope. A negative-sloped curve will be used for most applications, as this ensures this pawn will run fast down a hill and climb slowly up one.")]
		[SerializeField]

		protected AnimationCurve SlopeSpeedCurve = AnimationCurve.Constant(-1f, 1f, 1f);

		#endregion
		#region (field) TurnExponent

		/// <summary>
		/// When turning (accelerating in a desired direction that deviates from our current velocity), this value will modify the speed of that turn.
		///</summary>
		[Tooltip("When turning (accelerating in a desired direction that deviates from our current velocity), this value will modify the speed of that turn. If your turns feel stiff, try bumping this value up a little bit.")]
		[Min(1f)]
		[SerializeField]

		private float _TurnExponent = 1f;

		/// <inheritdoc cref="_TurnExponent"/>

		public float TurnExponent
		{
			get => _TurnExponent;
			set => _TurnExponent = Mathf.Max(value, 1f);
		}

		#endregion
		#region (field) VerticalMuteOnLand

		/// <summary>
		/// When landing on a slope, this pawn's vertical velocity will be multiplied by this amount in order to reduce excess motion.
		///</summary>

		[Tooltip("When landing on a slope, this pawn's vertical velocity will be multiplied by this amount in order to reduce excess motion. Set to 1 to maintain all momentum.")]
		[Range(0f, 1f)]

		public float VerticalMuteOnLand = 0.05f;

		#endregion
		#region (field) EnableSurfaceClasping

		/// <summary>
		/// Whether or not this pawn will clasp to steps and slopes.
		///</summary>

		[Tooltip("If enabled, the pawn will stick to the ground and climb up and down steps (this is typical for walk movement). If disabled, the pawn will maintain its momentum as it travels up and down slopes, creating a more physical feeling.")]

		public bool EnableSurfaceClasping = true;

		#endregion

		#endregion
		#region Members

		#region Friction

		/// <summary>
		/// Reference to a <see cref="FrictionMovementModule"/> from which to draw information.
		///</summary>

		private TFrictionModule _Friction;

		/// <inheritdoc cref="_Friction"/>

		public TFrictionModule Friction => _Friction;

		#endregion
		#region Ground

		/// <summary>
		/// Reference to a <see cref="GroundSensorModule"/> from which to draw information.
		///</summary>

		private TGroundSensor _Ground;

		/// <inheritdoc cref="_Ground"/>

		public TGroundSensor Ground => _Ground;

		#endregion

		#region InputVectorRaw

		/// <summary>
		/// The raw input vector read from the <see cref="InputSystem"/> represented by a <see cref="Vector2"/>.
		///</summary>

		private TInputSpace _inputVectorRaw;

		/// <inheritdoc cref="_inputVectorRaw"/>

		public TInputSpace inputVectorRaw => _inputVectorRaw;

		#endregion
		#region InputVector

		/// <summary>
		/// The desired direction of movement in world space represented by a <see cref="Vector3"/>. It is calculated by first finding the <see cref="inputVectorRaw"/> and rotating it around the <see cref="ViewTransform"/> (usually a camera).
		///</summary>

		private TMovementSpace _inputVector;

		/// <inheritdoc cref="_inputVector"/>

		public TMovementSpace inputVector { get => _inputVector; protected set => _inputVector = value; }

		#endregion
		#region LastValidInputVector

		/// <summary>
		/// The last input vector that wasn't zero.
		///</summary>

		private TMovementSpace _lastValidInputVector;

		/// <inheritdoc cref="_lastValidInputVector"/>

		public TMovementSpace lastValidInputVector => _lastValidInputVector;

		#endregion

		#region AccelerationVector

		/// <summary>
		/// Acceleration vector calculated every <see cref="FixedUpdate"/>.
		///</summary>

		protected TMovementSpace _accelVector;

		#endregion

		#endregion

		#region Properties

		#region (abstract) VelocityPercent

		/// <returns>
		/// <see cref="Pawn.velocity"/> as a percentage of <see cref="MaxWalkSpeed"/>.
		///</returns>

		public abstract TMovementSpace velocityPercent { get; }

		#endregion
		#region (abstract) SpeedPercent

		/// <returns>
		/// <see cref="Pawn.speed"/> as a percentage of <see cref="MaxWalkSpeed"/>.
		///</returns>

		public abstract float speedPercent { get; }

		#endregion
		#region (abstract) LateralVelocityPercent

		/// <returns>
		/// <see cref="Pawn.lateralVelocityLocal"/> as a percentage of <see cref="MaxWalkSpeed"/>.
		///</returns>

		public abstract TInputSpace lateralVelocityPercent { get; }

		#endregion
		#region (abstract) LateralSpeedPercent

		/// <returns>
		/// <see cref="Pawn.LateralVelocity.magnitude"/> as a percentage of <see cref="MaxWalkSpeed"/>.
		///</returns>

		public abstract float lateralSpeedPercent { get; }

		#endregion

		#region (abstract) TargetVelocity

		/// <summary>
		/// Our final desired velocity we want to be walking at.
		///</summary>

		protected abstract TMovementSpace targetVelocity { get; }

		#endregion
		#region (abstract) TargetSpeed

		/// <summary>
		/// Our desired speed we want to be walking at.
		///</summary>

		protected abstract float targetSpeed { get; }

		#endregion

		#region (virtual) MaxWalkSpeed

		protected virtual float maxWalkSpeed => MaxWalkSpeed * slopeSpeedMultiplier;

		#endregion
		#region (abstract) SlopeSpeedMultiplier

		/// <summary>
		/// Contextual value multiplied with <see cref="_MaxWalkSpeed"/> to produce a final MaxWalkSpeed value.
		///</summary>

		protected abstract float slopeSpeedMultiplier { get; }

		#endregion

		#region (virtual) WalkAcceleration_dp

		protected virtual float walkAcceleration => Mathf.Lerp(1f, WalkAcceleration, accelerationCurveMultiplier);

		#endregion
		#region AccelerationCurveMultiplier_dp

		protected float accelerationCurveMultiplier => AccelerationCurve.Evaluate(lateralSpeedPercent);

		#endregion

		#endregion
		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_Friction = GetComponent<TFrictionModule>();
			_Ground = GetComponent<TGroundSensor>();
		}

		#endregion
		#region Update

		protected override void Update()
		{
			_inputVector = ConvertInputToWorld(_inputVectorRaw);

			if (!_inputVector.Equals(default(TMovementSpace)))
				_lastValidInputVector = _inputVector;
		}

		#endregion

		#region (abstract) ConvertInputToWorld

		/// <summary>
		/// Converts the given <paramref name="inputVector"/> into world space, relative to <see cref="ViewTransform"/>.
		///</summary>

		public abstract TMovementSpace ConvertInputToWorld(in TInputSpace inputVector);

		#endregion
		#region (abstract) ConstrainAcceleration

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		protected abstract float ConstrainAcceleration(in TMovementSpace vector, in TMovementSpace currentVelocity, float acceleration, float speedLimit);

		#endregion

		#region (input) OnInputWalk

		public void OnInputWalk(in TInputSpace vector)
		{
			_inputVectorRaw = vector;
		}

		public void OnInputWalk(InputContext context)
		{
			_inputVectorRaw = context.ReadValue<TInputSpace>();
		}

		#endregion

		#region (static) CalculateAcceleration

		protected static float CalculateAcceleration(float acceleration, float currentVelocity, float limit)
		{
			return (currentVelocity + acceleration > limit) ? (limit - currentVelocity) : acceleration;
		}

		#endregion
		#region (static) CalculateAcceleration_FixedUpdate

		protected static float CalculateAcceleration_FixedUpdate(float acceleration, float velocity, float limit)
		{
			return CalculateAcceleration(acceleration * Time.fixedDeltaTime, velocity, limit) / Time.fixedDeltaTime;
		}

		#endregion

		#endregion
	}

	#endregion

	#region WalkModule (3D)

	/// <summary>
	/// This module allows a 3D pawn to move along the ground. It also may control the pawn's rotation.
	///</summary>

	[RequireComponent(typeof(PawnCapsule))]
	[RequireComponent(typeof(FrictionModule))]

	public sealed class WalkModule : WalkModuleBase<Rigidbody, PawnCapsule, FrictionModule, GroundSensorModule, Vector3, Vector2>
	{
		#region EYawRotationMode

		public enum EYawRotationMode
		{
			/// <summary>
			/// The pawn will rotate only when it explicitly told to do so.
			///</summary>

			[Tooltip("The pawn will rotate only when it explicitly told to do so. Best for 1st-person characters.")]

			Disabled,

			/// <summary>
			/// The pawn will rotate towards the direction we're walking in.
			///</summary>

			[Tooltip("The pawn will rotate towards the direction we're walking in. Good for simple 3rd-person characters.")]

			Implicit,

			/// <summary>
			/// The pawn will constantly rotate towards the direction we were last walking in.
			///</summary>

			[Tooltip("The pawn will constantly rotate towards the direction we were last walking in.")]

			ImplicitConstant,

			/// <summary>
			/// The pawn will rotate towards our input, and will drive our walk direction as well.
			///</summary>

			[Tooltip("The pawn will rotate towards our input, and will drive our walk direction as well. Good for 3rd-person characters.")]

			Drive,

			/// <summary>
			/// The pawn will constantly rotate towards the direction we were last walking in, and will drive our walk direction as well.
			///</summary>

			[Tooltip("The pawn will constantly rotate towards the direction we were last walking in, and will drive our walk direction as well.")]

			DriveConstant
		}

		#endregion

		#region Public Fields

		[Header("Rotation")]

		#region (field) YawRotationMode

		/// <summary>
		/// Defines how this pawn's walk motion will interact with its yaw rotation.
		///</summary>

		[Tooltip("Defines how this pawn's walk motion will interact with its yaw rotation.")]
		[SerializeField]

		private EYawRotationMode _YawRotationMode = EYawRotationMode.Disabled;
		public EYawRotationMode YawRotationMode
		{
			get => _YawRotationMode;
			set => _YawRotationMode = value;
		}

		#endregion
		#region (field) YawRotationSpeed

		/// <summary>
		/// The speed of how quickly this pawn may turn if <see cref="IsRotationUpdate"/>.
		///</summary>

		[Tooltip("The speed of how quickly this pawn may turn if YawRotationMode is NOT set to Explicit.")]
		[Min(0f)]
		[SerializeField]
		private float _YawRotationSpeed;

		/// <inheritdoc cref="_YawRotationSpeed"/>
		/// <remarks>
		/// This value is restricted and can be no less than 0.
		///</remarks>

		public float YawRotationSpeed
		{
			get => _YawRotationSpeed;
			set => _YawRotationSpeed = Mathf.Max(0f, value);
		}

		#endregion

		#endregion
		#region Members



		#endregion
		#region Properties

		#region WalkDirection

		/// <summary>
		/// This returns the direction in which we will actually move the pawn forward. Determined by <see cref="isMovementDirect"/>.
		///</summary>

		public Vector3 walkDirection =>
			isMovementDirect ?
			inputVector :
			Pawn.moveForwardVector * inputVector.magnitude;

		#endregion
		#region IsRotationUpdated

		/// <summary>
		/// This returns true if our pawn's yaw rotation is controlled by this component.
		///</summary>

		public bool isYawRotationUpdated =>
			YawRotationMode != EYawRotationMode.Disabled;

		#endregion
		#region IsMovementDirect

		/// <summary>
		/// This returns true if our pawn's movement is driven directly by input (i.e. not driven by <see cref="YawRotationMode"/>).
		///</summary>

		public bool isMovementDirect =>
			YawRotationMode != EYawRotationMode.Drive;

		#endregion
		#region TargetYawRotation

		/// <summary>
		/// The desired rotation for this pawn.
		///</summary>

		public float targetYawRotation
		{
			get
			{
				var inputNormal = lastValidInputVector.normalized;
				return Mathf.Atan2(inputNormal.x, inputNormal.z) * Mathf.Rad2Deg;
			}
		}

		#endregion
		#region IsRotationModeConstant

		public bool isRotationModeConstant =>
			YawRotationMode == EYawRotationMode.ImplicitConstant || YawRotationMode == EYawRotationMode.DriveConstant;

		#endregion

		#region (sealed override) VelocityPercent

		public sealed override Vector3 velocityPercent =>
			Pawn.velocity / maxWalkSpeed;

		#endregion
		#region (sealed override) SpeedPercent

		public sealed override float speedPercent =>
			Pawn.velocity.magnitude / maxWalkSpeed;

		#endregion
		#region (sealed override) LateralVelocityPercent

		public sealed override Vector2 lateralVelocityPercent =>
			Pawn.lateralVelocityLocal / maxWalkSpeed;

		#endregion
		#region (sealed override) LateralSpeedPercent

		public sealed override float lateralSpeedPercent =>
			Pawn.lateralVelocityLocal.magnitude / maxWalkSpeed;

		#endregion

		#region (override) TargetVelocity

		protected override Vector3 targetVelocity =>
			inputVector * maxWalkSpeed;

		#endregion
		#region (sealed override) TargetSpeed

		protected sealed override float targetSpeed =>
			targetVelocity.magnitude;

		#endregion
		#region (override) MaxWalkSpeed

		protected override float maxWalkSpeed =>
			base.maxWalkSpeed *
			Friction.airbornePercent *
			(
				Ground.isGrounded && Ground.CastResult.surface ?
				Ground.CastResult.surface.FrictionSpeedScale :
				1f
			);

		#endregion
		#region (override) WalkAcceleration

		protected override float walkAcceleration =>
			base.walkAcceleration * Friction.strength;

		#endregion

		#region (override) SlopeSpeedMultiplier

		protected override float slopeSpeedMultiplier =>
			SlopeSpeedCurve.Evaluate(Ground.GetSlopeDirectionalPercent(inputVector));

		#endregion

		#endregion
		#region Methods

		#region (override) OnEnabled

		protected override void OnEnable()
		{

		}

		#endregion

		#region (override) OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			// Ground.Events.OnLand.AddListener((HitResult hit) =>
			// {
			// 	// Pawn.GetComponent<GravityModule>().Enabled = false;
			// 	// Pawn.VerticalVelocity *= VerticalMuteOnLand;
			// 	// _YawRotation = transform.rotation;
			// });

			// Ground.Events.OnSlip.AddListener(() =>
			// {
			// 	// Pawn.GetComponent<GravityModule>().Enabled = true;
			// });
		}

		#endregion
		#region (override) Update

		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			#region Update YawRotation

			if (YawRotationMode != EYawRotationMode.Disabled)
			{
				float __yaw = Mathf.LerpAngle
				(
					transform.eulerAngles.y,
					targetYawRotation,
					YawRotationSpeed * (isRotationModeConstant ? 1f : inputVector.magnitude) * Time.deltaTime
				);
				var __yawRotation = Quaternion.Euler(transform.eulerAngles.x, __yaw, transform.eulerAngles.z);

				Pawn.Rigidbody.MoveRotation(__yawRotation);
			}

			#endregion
		}

		#endregion
		#region (override) ApplyMotion

		public override void ApplyMotion()
		{
			/** Determine our desired acceleration direction along the ground (or just use the input vector)
			*/

			Vector3 __walkVector = Ground.GetSlopeDirectionalNormal(walkDirection);

			/** Multiply by desired acceleration amount
			*/

			__walkVector *= walkAcceleration;

			#region Update Acceleration Vector

			/** Calculate the appropriate acceleration vector to apply this FixedUpdate.
			*/

			if (__walkVector == Vector3.zero)
				_accelVector = Vector3.zero;
			else if (EnableExcessWalkSpeedInAir && !Ground.isGroundedStrictly)
				_accelVector = __walkVector;
			else
				_accelVector = __walkVector.normalized * ConstrainAcceleration
				(
					/**
					*	Desired move direction and speed
					*/
					__walkVector,

					/**
					*	Current speed, depending on if we are grounded or not.
					*/
					Ground.isGrounded ? Pawn.velocity : Pawn.lateralVelocityWorld,

					/**
					*	Speed of our desired acceleration
					*/
					Vector3.Scale(__walkVector, Vector3.one - Pawn.moveUpVector).magnitude,

					/**
					*	Limits the speed such that we don't accelerate past what MaxWalkSpeed allows.
					*/
					GetTargetSpeed(Time.fixedDeltaTime)
				);

			#endregion
			#region Apply Acceleration as Force

			/**
			*	Apply the acceleration.
			*/

			Pawn.Rigidbody.AddForce(_accelVector, ForceMode.Acceleration);

			#endregion
			#region Surface Clasping

			if (EnableSurfaceClasping && Ground.isGrounded)
			{
				/**
				*	Adjust the pawn position to the height of the step we wish to move on.
				*/

				var __point = Ground.CastResult.point;
				float __heightAdjust = __point.y - Pawn.ColliderShape.GetTailPosition().y;
				Pawn.Rigidbody.MovePosition(transform.position + Pawn.moveUpVector * __heightAdjust);

				/**
				*	Adjust the pawn velocity to account for slopes.
				*/

				Pawn.velocity = Pawn.ProjectVelocityOntoSurface(Ground.slopeNormal);
			}

			#endregion
		}

		#endregion

		#region (sealed override) ConvertInputToWorld

		public sealed override Vector3 ConvertInputToWorld(in Vector2 inputVector)
		{
			var __yawOnlyRotation = Quaternion.Euler(Vector3.Scale(
				ViewTransform.localEulerAngles,
				Pawn.moveUpVector
			));

			return __yawOnlyRotation * inputVector.XZ();
		}


		#endregion
		#region (sealed override) ConstrainAcceleration

		protected sealed override float ConstrainAcceleration(in Vector3 vector, in Vector3 currentVelocity, float acceleration, float speedLimit)
		{
			/** Prevents locking movement up when travelling too fast in a direction other than the desired direction.
			*/

			float __directionAllowance = Math.Remap(Vector3.Dot(vector.normalized, currentVelocity.normalized), -1f, 1f);

			return Mathf.Max(0f, CalculateAcceleration_FixedUpdate(
				acceleration,
				currentVelocity.magnitude * Mathf.Pow(__directionAllowance, TurnExponent),
				speedLimit
			));

		}

		#endregion

		#region GetTargetSpeed

		/// <summary>
		/// This is the "actual" Target Speed. At runtime the applied acceleration is affected by Friction, so using this accounts for that loss in momentum.
		///</summary>

		public float GetTargetSpeed(float deltaTime = 1f) =>
			targetSpeed + (Friction.strength * deltaTime);

		#endregion

		#region OnDrawGizmosSelected
#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			DebugDraw.DrawPoint(transform.position, Color.black);

			if (Ground)
			{
				DebugDraw.DrawArrow
				(
					Ground.isTouchingAnyGround ? Ground.CastResult.point : Pawn.ColliderShape.GetTailPosition(),
					Ground.GetSlopeDirectionalNormal(_accelVector / Friction.strength),
					Color.green
				);
			}
		}

#endif
		#endregion

		#endregion
	}
	#endregion
}
