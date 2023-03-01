
/** GroundSensorModule.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.

*/

#region Includes

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Cuberoot.Modules
{
	#region Ground Sensor Module Base

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public abstract class GroundSensorModuleBase

	<THitResult, TPawn, TGravityModule, TMovementSpace> :
	CasterModule<THitResult>, IPawnModule<TPawn>

	where THitResult : HitResultBase, new()
	where TGravityModule : GravityModuleBase
	{
		#region Inners

		#region GroundEvents

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		[System.Serializable]

		public class GroundEvents
		{
			/// <summary>
			/// This event is called when the pawn lands on solid ground after being airborne or sliding on a slope.
			///</summary>
			[Tooltip("This event is called when the pawn lands on solid ground after being airborne or sliding on a slope.")]
			[SerializeField]

			public UnityEvent<THitResult> OnLand;

			/// <summary>
			/// This event is called when the pawn ceases to touch any ground.
			///</summary>
			[Tooltip("This event is called when the pawn ceases to touch any ground.")]
			[SerializeField]

			public UnityEvent OnAirborne;

			/// <summary>
			/// This event is called when the pawn touches any ground after being airborne. May be called simultaneously with <see cref="OnLand"/>.
			///</summary>
			[Tooltip("This event is called when the pawn touches any ground after being airborne. May be called simultaneously with OnLand.")]
			[SerializeField]

			public UnityEvent<THitResult> OnTouch;

			/// <summary>
			/// This event is called when the pawn leaves solid ground and becomes airborne or begins sliding. May be called simultaneously with <see cref="OnAirborne"/>.
			///</summary>
			[Tooltip("This event is called when the pawn leaves solid ground and becomes airborne or begins sliding. May be called simultaneously with OnAirborne.")]
			[SerializeField]

			public UnityEvent OnSlip;
		}

		#endregion

		#endregion

		#region Fields

		#region TreatSlopesAsFloors

		/// <summary>
		/// If set to true, steep slopes will be treated like slippery floors that can be somewhat navigated. If false, steep slopes are treated exclusively as walls.
		///</summary>
		[Tooltip("If set to true, steep slopes will be treated like slippery floors that can be somewhat navigated. If false, steep slopes are treated exclusively as walls. Set to true for more dynamic/fluid movement, set to false for more rigid/precise movement.")]
		[SerializeField]

		public bool TreatSlopesAsFloors = false;

		#endregion
		#region SlopeMaxAngle

		/// <summary>
		/// Any slope that is steeper than this angle will be treated as a wall instead of ground.
		///</summary>
		[Tooltip("Any slope that is steeper than this angle will be treated as a wall instead of ground. Since stairs are technically flat, this variable does not affect walking up/down stairs.")]
		[Range(0f, 90f)]
		[SerializeField]

		private float _SlopeMaxAngle = 60f;

		/// <inheritdoc cref="_SlopeMaxAngle"/>

		public float SlopeMaxAngle
		{
			get => _SlopeMaxAngle;
			set => _SlopeMaxAngle = Mathf.Clamp(value, 0f, 90f);
		}

		#endregion
		#region CoyoteTimer

		/// <summary>
		/// The amount of time after leaving the ground that we actually switch Ground states. Ergo, Wile E. Coyote.
		///</summary>

		[Tooltip("The amount of time after leaving the ground that we actually switch Ground states. Ergo, Wile E. Coyote.")]
		[SerializeField]

		protected Timer _CoyoteTimer;

		/// <inheritdoc cref="_CoyoteTimer"/>

		public float CoyoteTime
		{
			get => _CoyoteTimer.Duration;
			set => _CoyoteTimer.Duration = value;
		}

		/**	Member CoyoteTimeline
		*/

		#endregion
		#region GroundEvents

		/// <inheritdoc cref="GroundEvents"/>

		[Tooltip("__TODO_ANNOTATE__")]
		[SerializeField]

		private GroundEvents _Events;

		/// <inheritdoc cref="_Events"/>

		public GroundEvents Events => _Events;

		#endregion

		#endregion

		#region Members

		#region Pawn

		/// <inheritdoc cref="Pawn"/>

		private TPawn _Pawn;
		public TPawn Pawn => _Pawn;

		#endregion
		#region GravityModule

		private TGravityModule _Gravity;
		public TGravityModule Gravity => _Gravity;

		#endregion

		#region PreciseHitResult

		/// <summary>
		/// This HitInfo is collected after the initial Hit is performed, only if we <see cref="isTouchingAnyGround"/>. It is used to determine the *actual* slope value that is used for determining speed.
		///</summary>

		private THitResult _preciseHitResult;

		/// <inheritdoc cref="_preciseHitResult"/>

		public THitResult preciseHitResult => _preciseHitResult.Safe();

		#endregion
		#region LandingHitResult

		/// <summary>
		/// This HitInfo is collected after the initial Hit is performed, only if we are NOT <see cref="isGroundedStrictly"/>. It is used to determine if we SHOULD be grounded.
		///</summary>

		private THitResult _landingHitResult;

		/// <inheritdoc cref="_landingHitResult"/>

		public THitResult landingHitResult => _landingHitResult.Safe();

		#endregion

		#region GroundState

		/// <summary>
		/// The current state of being on the ground.
		///</summary>

		private EGroundState _groundState;

		/// <inheritdoc cref="_groundState"/>

		public EGroundState groundState
		{
			get => _groundState;
			protected set
			{
				if (_groundState == value) return;

				var previous = _groundState;
				_groundState = value;

				if (value == EGroundState.Coyoting)
					if (CoyoteTime == 0f)
						_groundState = EGroundState.Airborne;
					else
						_CoyoteTimer.Start();

				if (value == EGroundState.Grounded)
					Events.OnLand.Invoke(preciseHitResult);
				else if (value == EGroundState.Airborne)
					Events.OnAirborne.Invoke();

				if (previous == EGroundState.Airborne)
					Events.OnTouch.Invoke(preciseHitResult);
				else if (previous == EGroundState.Grounded || previous == EGroundState.Coyoting)
					Events.OnSlip.Invoke();
			}
		}

		#endregion

		#endregion

		#region Properties

		#region IsGrounded

		/// <summary>
		/// Whether or not we are currently on solid ground. This will return FALSE even if we are <see cref="EGroundState.Touching"/>.
		///</summary>

		public bool isGroundedStrictly => enabled &&
			_groundState == EGroundState.Grounded || _groundState == EGroundState.Coyoting;

		/// <summary>
		/// Whether or not we are currently touching any ground; even steep ground that is not considered "solid" ground. This will return TRUE if we are on solid ground as well.
		///</summary>

		public bool isTouchingAnyGround => enabled &&
			_groundState != EGroundState.Airborne;

		/// <summary>
		/// If <see cref="TreatSlopesAsFloors"/> is true, this will return <see cref="isTouchingAnyGround"/>. If "treating slopes as walls", this will return <see cref="isGroundedStrictly"/>.
		///</summary>

		public bool isGrounded =>
			TreatSlopesAsFloors ? isTouchingAnyGround : isGroundedStrictly;

		public bool isAirborneStrictly =>
			_groundState == EGroundState.Airborne || _groundState == EGroundState.Coyoting;

		#endregion

		#endregion
		#region Methods

		#region (override) OnValidate

		protected override void OnValidate()
		{
			_Pawn = GetComponent<TPawn>();
			_Gravity = GetComponent<TGravityModule>();

			_CoyoteTimer.Duration = CoyoteTime;
			_CoyoteTimer.OnStart.AddListener(() =>
			{
				_Gravity.enabled = false;
			});
			_CoyoteTimer.OnCease.AddListener(() =>
			{
				groundState = EGroundState.Airborne;
				_Gravity.enabled = true;
			});
			// Events.OnTouch.AddListener((THitResult _) =>
			// {
			// 	_CoyoteTimeline.Cancel();
			// });
		}

		#endregion
		#region (override) FixedUpdate

		protected sealed override void FixedUpdate()
		{
			CastResult = CalculatePrimaryHitResult();

			#region Update Ground State

			if (CastResult.isValid)
			{
				if (isLandingAvailable)
					groundState = EGroundState.Grounded;
				else
					groundState = EGroundState.Touching;
			}
			else if (groundState != EGroundState.Airborne)
			{
				groundState = EGroundState.Coyoting;
			}

			#endregion

			_CoyoteTimer.Update();

			if (!isGroundedStrictly)
				_landingHitResult = CalculateLandingHitResult();

			if (isTouchingAnyGround)
				_preciseHitResult = CalculatePreciseHitResult();

			// print(GroundState);
		}

		#endregion
		// #region OnEnabled

		// protected override void OnEnabled()
		// {

		// }

		// #endregion
		#region OnDisabled

		protected override void OnDisable()
		{
			groundState = EGroundState.Airborne;
		}

		#endregion

		#endregion
		#region Abstracts

		/// <summary>
		/// This property will return true if the player should be grounded.
		///</summary>

		protected virtual bool isLandingAvailable =>
			slopeAngle < SlopeMaxAngle &&
			(landingHitResult.isValid || isGroundedStrictly)
		;

		/// <summary>
		/// The hit normal of the Collider Cast. This is the normal value used to calculate precise movement direction in adherence with the Shape Cast. Primarily used to determine direction.
		///</summary>

		public abstract TMovementSpace slopeMoveNormal { get; }

		/// <summary>
		/// The hit normal of <see cref="PerformCast_Direct"/>. This is the normal value used to calculated walk speed and slope limits. Primarily used to determine magnitude.
		///</summary>

		public abstract TMovementSpace slopeNormal { get; }

		/// <summary>
		/// The percent value of the current slope's steepness in relation to <see cref="PawnPawn.MoveUpVector"/>, where 0 is flat ground and 1 is a vertical wall.
		///</summary>

		public abstract float slopePercent { get; }

		/// <summary>
		/// The degree angle of the current slope's steepness in relation to <see cref="PawnPawn.MoveUpVector"/>, where 0 is flat ground and 90 is a vertical wall.
		///</summary>

		public abstract float slopeAngle { get; }

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		public abstract float slopeDirectionalAngle { get; }

		/// <returns>
		/// The provided forward vector along the ground. If we're not grounded, it will calculate using <see cref="Pawn.moveUpVector"/>.
		///</returns>

		public abstract TMovementSpace GetSlopeDirectionalNormal(in TMovementSpace forward);

		/// <returns>
		/// The sign value of the current slope's steepness in relation to <see cref="Pawn.moveUpVector"/> and <paramref name="forward"/>, where 0 is flat ground, -1 represents a downward slope, and +1 represents an upward slope.
		///</returns>

		public abstract float GetSlopeDirectionalSign(in TMovementSpace forward);

		/// <returns>
		/// The percent value of the current slope's steepness in relation to <see cref="Pawn.moveUpVector"/> and <paramref name="forward"/>, where 0 is flat ground, negative values represent downward slopes, and positive slopes represent upward slopes.
		///</returns>

		public virtual float GetSlopeDirectionalPercent(in TMovementSpace forward)
		{
			return slopePercent * GetSlopeDirectionalSign(forward);
		}

		/// <returns>
		/// The degree angle of the current slope's steepness in relation to <see cref="Pawn.moveUpVector"/> and <paramref name="forward"/>, where 0 is flat ground, negative values represent downward slopes, and positive slopes represent upward slopes.
		///</returns>

		public virtual float GetSlopeDirectionalAngle(in TMovementSpace forward)
		{
			return slopeAngle * GetSlopeDirectionalSign(forward);
		}

		// protected abstract EGroundState CalculateInitialGroundState();
		protected abstract THitResult CalculatePrimaryHitResult();
		protected abstract THitResult CalculatePreciseHitResult();
		protected abstract THitResult CalculateLandingHitResult();

		#endregion
	}

	#endregion
	#region (class) GroundSensorModule

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	[RequireComponent(typeof(PawnCapsule))]

	public sealed class GroundSensorModule :
	GroundSensorModuleBase<HitResult, PawnCapsule, GravityModule, Vector3>
	{
		#region Overrides

		#region ShouldCheckForGround

		protected override bool isLandingAvailable =>
			base.isLandingAvailable &&
			(isGrounded || Pawn.verticalVelocityLocal <= 0f)
		;

		#endregion
		#region SlopeMoveNormal

		public override Vector3 slopeMoveNormal =>
			isTouchingAnyGround ? CastResult.normal : Pawn.moveUpVector;

		#endregion
		#region SlopeNormal

		public override Vector3 slopeNormal => isTouchingAnyGround ? preciseHitResult.normal : Pawn.moveUpVector;

		#endregion

		#region SlopePercent

		public override float slopePercent
		{
			get
			{
				if (!isTouchingAnyGround)
					return 0f;

				float result = Vector3.Dot(Pawn.moveUpVector, slopeNormal);

				/** Return the result squared. 45 degrees = 0.71 radians ^ 2 = 50 percent.
				*/
				return 1f - (result * result);
			}
		}

		#endregion
		#region SlopeMoveAngle

		public override float slopeAngle
		{
			get
			{
				if (!isTouchingAnyGround)
					return 0f;

				return Vector3.Angle(Pawn.moveUpVector, slopeNormal);
			}
		}

		#endregion
		#region SlopeDirectionalAngle

		public override float slopeDirectionalAngle =>
			slopeAngle * GetSlopeDirectionalSign(transform.forward);

		#endregion

		#region GetSlopeDirectionalNormal

		public override Vector3 GetSlopeDirectionalNormal(in Vector3 forward)
		{
			return isGrounded ?
			Vector3.Cross(Vector3.Cross(Pawn.moveUpVector, forward), slopeNormal) :
			forward;

			// /** Used to calculate sliding vector
			// */
			// return Vector3.Project(SlopeNormal, Pawn.MoveUpVector);
		}

		#endregion
		#region GetSlopeDirectionalSign

		public override float GetSlopeDirectionalSign(in Vector3 forward)
		{
			if (forward.Equals(default(Vector3))) return 0f;
			return Mathf.Sign(Vector3.Dot(Pawn.moveUpVector, GetSlopeDirectionalNormal(forward)));
		}

		#endregion
		#region GetSlopeDirectionalPercent

		public override float GetSlopeDirectionalPercent(in Vector3 forward)
		{
			float directionPercent = 1f - Vector3.Dot(forward, Vector3.Cross(Pawn.moveUpVector, slopeNormal).normalized).Abs();

			return base.GetSlopeDirectionalPercent(forward) * directionPercent;
		}

		#endregion
		#region GetSlopeDirectionalAngle

		public override float GetSlopeDirectionalAngle(in Vector3 forward)
		{
			return base.GetSlopeDirectionalAngle(forward) * Vector3.Dot(forward, GetSlopeDirectionalNormal(forward));
		}

		#endregion

		#region CalculatePrimaryHitResult

		protected override HitResult CalculatePrimaryHitResult() =>
			HitResult.CapsuleCast
			(
				Pawn.ColliderShape.GetHeadPositionUncapped() + Pawn.moveUpVector * MaxDistance,
				Pawn.ColliderShape.GetTailPositionUncapped() + Pawn.moveUpVector * MaxDistance,
				Pawn.SkinnedRadius,
				-Pawn.moveUpVector,
				MaxDistance * 2f,
				Layers
			);

		#endregion
		#region CalculateLandingHitResult

		protected override HitResult CalculateLandingHitResult() =>
			HitResult.CapsuleCast
			(
				Pawn.ColliderShape.GetHeadPositionUncapped() + Pawn.moveUpVector * Pawn.SkinWidth,
				Pawn.ColliderShape.GetTailPositionUncapped() + Pawn.moveUpVector * Pawn.SkinWidth,
				Pawn.ColliderShape.radius,
				-Pawn.moveUpVector,
				Pawn.SkinWidth * 2f,
				Layers
			);

		#endregion
		#region CalculatePreciseHitResult

		protected override HitResult CalculatePreciseHitResult()
		{
			var hits = HitResult.LinecastAll
			(
				CastResult.point + (Pawn.moveUpVector * MaxDistance / 2f),
				CastResult.point - (Pawn.moveUpVector * MaxDistance / 2f),
				Layers
			);

			HitResult result = default;
			float largestDot = -2f;
			foreach (var hit in hits)
			{
				if (Vector3.Dot(hit.normal, Pawn.moveUpVector) > largestDot)
					result = hit;
			}

			return result;
		}

		#endregion

		#endregion
		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();
		}

		#endregion



		#region OnDrawGizmosSelected
#if UNITY_EDITOR

		private void OnDrawGizmosSelected()
		{
			// GroundCheckHit.DrawCapsuleCast(transform.forward, transform.up, Pawn.ColliderShape.radius, Pawn.ColliderShape.height);

			// if (IsTouching)
			// {
			// 	DebugDraw.DrawArrow(DirectHit.Point, DirectHit.Normal, IsGrounded ? Color.blue : Color.magenta);

			// 	CastResult.DrawLinecast();
			// 	DebugDraw.DrawPoint(CastResult.AdjustmentPoint, Color.blue);


			// }
		}

#endif
		#endregion

		#endregion
	}

	#endregion

	#region EGroundState

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public enum EGroundState
	{
		/// <summary>
		/// For use when we are not touching any ground.
		///</summary>

		Airborne,

		/// <summary>
		/// For use when we are touching a steep incline that doesn't count as true Ground.
		///</summary>

		Touching,

		/// <summary>
		/// For use when we are firmly planted on shallow ground.
		///</summary>

		Grounded,

		/// <summary>
		/// For use when we are not touching any ground, but we wish to continue to act as if we are grounded, i.e. "Wile E. Coyote-ing."
		///</summary>

		Coyoting
	}

	#endregion
}
