/** FrictionModule.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using UnityEngine;

#endregion

namespace Cuberoot.Modules
{
	#region (abstract) FrictionModuleBase

	/// <summary>
	/// This movement component decelerates the PawnController to a stop. If a GroundSensor is provided, this component will apply a different strength deceleration depending on whether or not they are on the ground.
	///</summary>

	public abstract class FrictionModuleBase<TRigidbody, TPawn, TGroundSensor, TMovementSpace> : MovementModule_OptionalPawn<TRigidbody, TPawn>
	{
		#region Public Fields

		#region DefaultStrength

		/// <summary>
		/// Default strength of friction force applied if no physical material data is available.
		///</summary>

		[Tooltip("Default strength of friction force applied if no physical material data is available.")]
		[Min(0f)]

		public float DefaultStrength = 50f;

		#endregion

		#endregion
		#region AirbornePercent

		/// <summary>
		/// Percentage of DefaultStrength to use while not grounded. If no GroundSensor is available, this value will not be used.
		///</summary>

		[Tooltip("Percentage of DefaultStrength to use while not grounded. If no GroundSensor is available, this value will not be used. NOTE: the actual value will be applied as the square of this value.")]
		[Range(0f, 1f)]
		[SerializeField]

		private float _AirbornePercent = 0.25f;

		/// <inheritdoc cref="_AirbornePercent"/>
		public float AirbornePercent
		{
			get => _AirbornePercent.Pow(2f);
			set => _AirbornePercent = Mathf.Clamp01(value.Pow(0.5f));
		}

		#endregion

		#region Members

		#region Ground

		/// <summary>
		/// Ground Sensor component from which to gather information.
		///</summary>

		private TGroundSensor _Ground;

		/// <inheritdoc cref="_Ground"/>

		public TGroundSensor Ground => _Ground;

		#endregion

		#region Scale

		/// <summary>
		/// Overall scale of friction force applied to this object.
		///</summary>

		[HideInInspector]

		public float Scale = 1f;

		#endregion

		#region MotionVector

		/// <summary>
		/// Value used to draw gizmos.
		///</summary>

		protected Vector3 _motionVector;

		#endregion

		#endregion

		#region Properties

		#region Strength

		/// <summary>
		/// This is the actual strength value used to apply force. It is determined by a variety of contextual factors.
		///</summary>
		/// <seealso cref="SurfaceStrength_cx"/>
		/// <seealso cref="ContextualAirborneStrength"/>

		public float strength
		{
			get => DefaultStrength * surfaceStrength * airbornePercent * Scale;
		}

		#endregion
		#region (virtual) SurfaceStrength

		/// <summary>
		/// Contextual multiplier determined by the physical material this pawn is standing on. If no <see cref="TGroundSensor"/> is available, <see cref="DefaultStrength"/> is used.
		///</summary>

		public virtual float surfaceStrength => 1f;

		#endregion
		#region (virtual) AirbornePercent

		/// <summary>
		/// Contextual multiplier determined by whether or not we're currently in the air. If no <see cref="TGroundSensor"/> is available, this value is ignored.
		///</summary>

		public virtual float airbornePercent => 1f;

		#endregion

		#endregion
		#region Methods

		#region (override) OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			if (_Ground == null)
				_Ground = GetComponent<TGroundSensor>();
		}

		#endregion

		#region (abstract) CalculateDeceleration

		/// <summary>
		/// Calculates the appropriate deceleration vector given the <paramref name="inputVelocity"/> and a <paramref name="decelerationStrength"/>.
		///</summary>
		/// <remarks>
		/// <paramref name="decelerationStrength"/> will automatically be scaled by <see cref="Time.fixedDeltaTime"/> within this function.
		///</remarks>

		public abstract TMovementSpace CalculateDeceleration(in TMovementSpace inputVelocity, in float decelerationStrength);

		#endregion


		#endregion
	}

	#endregion
	#region FrictionModule

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	[RequireComponent(typeof(GroundSensorModule))]

	public class FrictionModule : FrictionModuleBase<Rigidbody, Pawn, GroundSensorModule, Vector3>
	{
		#region Properties

		public sealed override float airbornePercent =>
			Ground.isGrounded ? 1f : AirbornePercent;

		#endregion
		#region Methods

		#region ApplyMotion

		public sealed override void ApplyMotion()
		{
			if (strength <= 0f) return;

			base.ApplyMotion();
		}

		protected sealed override void ApplyMotion_WithPawn()
		{
			if (Pawn.velocity == Vector3.zero) return;

			Vector3 __motionVector = CalculateDeceleration(Ground.GetSlopeDirectionalNormal(Ground.isGrounded ? Pawn.velocity : Pawn.lateralVelocityWorld), strength);

			Rigidbody.AddForce(__motionVector, ForceMode.Acceleration);
		}

		protected sealed override void ApplyMotion_WithoutPawn()
		{
			if (Rigidbody.velocity == Vector3.zero) return;

			Vector3 __motionVector = CalculateDeceleration(Rigidbody.velocity, strength);

			Rigidbody.AddForce(__motionVector, ForceMode.Acceleration);
		}

		#endregion
		#region (sealed) CalculateDeceleration

		public sealed override Vector3 CalculateDeceleration(in Vector3 inputVelocity, in float decelerationStrength)
		{
			Vector3 __result = -inputVelocity.normalized * decelerationStrength * Time.fixedDeltaTime;
			Vector3 __velocitySign = inputVelocity.Sign();

			if (__velocitySign.x != 0f && __velocitySign.x != Mathf.Sign(__result.x + inputVelocity.x))
				__result.x = -inputVelocity.x;
			if (__velocitySign.y != 0f && __velocitySign.y != Mathf.Sign(__result.y + inputVelocity.y))
				__result.y = -inputVelocity.y;
			if (__velocitySign.z != 0f && __velocitySign.z != Mathf.Sign(__result.z + inputVelocity.z))
				__result.z = -inputVelocity.z;

			return __result / Time.fixedDeltaTime;
		}

		#endregion

		#region OnDrawGizmosSelected
#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			if (Ground.CastResult.IsValid())
				DebugDraw.DrawArrow(Ground.CastResult.point, _motionVector / strength, Color.red);
		}

#endif
		#endregion

		#endregion
	}

	#endregion
}
