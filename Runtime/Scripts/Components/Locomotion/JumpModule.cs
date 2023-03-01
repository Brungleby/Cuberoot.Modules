
/** JumpModule.cs
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

#endregion

namespace Cuberoot.Modules
{
	#region JumpModuleBase

	/// <summary>
	/// Allows this pawn to jump from the ground.
	///</summary>

	public abstract class JumpModuleBase<TPawn, TGroundSensor, TMovementSpace> : ActuationModule_RequirePawn<TPawn>, IMovementModule
	where TPawn : Component
	where TGroundSensor : Module
	where TMovementSpace : unmanaged
	{
		#region Public Fields

		#region Ground

		/// <summary>
		/// Ground Sensor component from which to gather contextual information.
		///</summary>

		private TGroundSensor _Ground;

		/// <inheritdoc cref="_Ground"/>

		public TGroundSensor Ground => _Ground;

		#endregion

		#region Basic Info
		[Header("Basic Info")]

		#region MaxJumps

		/// <summary>
		/// The number of jumps this pawn can make, including the initial jump (or becoming airborne) from the ground.
		///</summary>

		[Tooltip("The number of jumps this pawn can make, including the initial jump (or becoming airborne) from the ground.")]
		[Min(0)]

		public int MaxJumps = 1;

		#endregion

		#region JumpStrength

		/// <summary>
		/// The initial burst force of the jump.
		///</summary>

		[Tooltip("The initial burst force of the jump.")]
		[Min(0f)]

		public float JumpStrength = 4f;

		#endregion
		#region JumpThrust

		/// <summary>
		/// The amount of force to apply every frame while holding the jump button.
		///</summary>

		[Tooltip("The amount of force to apply every frame while holding the jump button.")]
		[Min(0f)]

		public float JumpThrust = 15f;

		#endregion

		#endregion

		#region Fine Tuning
		[Header("Fine Tuning")]

		#region EnableJumpFromSlopes

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		[Tooltip("__TODO_ANNOTATE__")]
		[SerializeField]

		private bool _EnableJumpFromSlopes = false;

		/// <inheritdoc cref="_EnableJumpFromSlopes"/>

		public virtual bool EnableJumpFromSlopes
		{
			get => _EnableJumpFromSlopes;
			set
			{
				if (_EnableJumpFromSlopes == value) return;

				_EnableJumpFromSlopes = value;

				RefreshEvents();
			}
		}

		#endregion

		#region MaxJumpTime

		/// <summary>
		/// The maximum amount of time we can hold the jump button for and apply <see cref="JumpThrust"/>.
		///</summary>

		[Tooltip("The maximum amount of time we can hold the jump button for and apply JumpThrust.")]
		[Min(0f)]

		public float MaxJumpTime = 0.0f;

		#endregion
		#region InputGraceTime

		/// <summary>
		/// If the player tries to jump while in the air, this is the amount of time before hitting the ground that this pawn will "remember" to jump the moment it lands on the ground.
		///</summary>

		[Tooltip("If the player tries to jump while in the air, this is the amount of time before hitting the ground that this pawn will \"remember\" to jump the moment it lands on the ground. The default value is acceptable for most applications.")]
		[Min(0f)]

		public float InputGraceTime = 0.25f;

		#endregion
		#region GroundDisableTime

		/// <summary>
		/// This is the amount of time after we jump to temporarily disable the GroundSensor component (to prevent a false jump).
		///</summary>

		[Tooltip("This is the amount of time after we jump to temporarily disable the GroundSensor component (to prevent a false jump). The default value is acceptable for most applications.")]
		[Min(0f)]

		public float GroundDisableTime = 0.1f;

		#endregion

		#endregion

		#endregion

		#region Members

		#region IsJumping

		/// <summary>
		/// Whether or not we are currently applying any jump forces. Becomes true when the player presses the jump button and becomes false when the player releases the jump button or holds the button until <see cref="MaxJumpTime"/> has elapsed.
		///</summary>

		private bool _isJumping = false;

		/// <inheritdoc cref="_isJumping"/>

		public bool isJumping => _isJumping;

		#endregion
		#region JumpCount

		/// <summary>
		/// The number of jumps that have been currently been made since leaving the ground.
		///</summary>

		private int _jumpCount = 0;

		#endregion

		#region WhenInputtedJump

		/// <summary>
		/// The time at which the player attempted to <see cref="BeginJump"/>. Used <see cref="OnGroundFloored"/> to determine if a jump should be made immediately.
		///</summary>
		/// <seealso cref="InputLoadTime"/>

		private float _whenInputtedJump = -1000f;

		#endregion
		#region WhenJumped

		/// <summary>
		/// The time at which the player successfully began jumping. Used to determine when to automatically <see cref="CeaseJump"/>ing.
		///</summary>

		private float _whenJumped = -1000f;

		#endregion
		#region GroundResetTimer

		private Timer _groundResetTimer;

		#endregion

		#endregion

		#region Properties

		#region IsAbleToJump

		/// <summary>
		/// Determines whether or not this pawn is allowed to make another jump.
		///</summary>

		public virtual bool isAbleToJump =>
			_jumpCount < MaxJumps;

		#endregion

		#endregion
		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_Ground = GetComponent<TGroundSensor>();

			_groundResetTimer = new Timer();
			_groundResetTimer.Duration = 0f;

			_groundResetTimer.OnStart.AddListener(() => { _Ground.Enabled = false; });
			_groundResetTimer.OnCease.AddListener(() => { _Ground.Enabled = true; });

			Handler.OnBeginActuation.AddListener(() =>
			{
				_whenInputtedJump = Time.time;

				BeginJump();
			});
			Handler.OnCeaseActuation.AddListener(() =>
			{
				CeaseJump();
			});

			RefreshEvents();
		}

		#endregion
		#region Update

		protected override void Update()
		{
			_groundResetTimer.Update();

			if (_isJumping)
			{
				if (Time.time > _whenJumped + MaxJumpTime)
					CeaseJump();

			}

			Ground.Enabled = Time.time > _whenJumped + GroundDisableTime;
		}

		#endregion
		#region FixedUpdate

		protected sealed override void FixedUpdate()
		{
			ApplyMotion();
		}

		#endregion
		#region ApplyMotion

		public abstract void ApplyMotion();

		#endregion

		#region RefreshEvents

		protected abstract void RefreshEvents();

		#endregion

		#region BeginJump

		/// <summary>
		/// Call this function to attempt to perform a jump. It will only execute if <see cref="isAbleToJump"/> returns true.
		///</summary>

		public void BeginJump()
		{
			if (Enabled)
			{
				if (isAbleToJump)
				{
					_isJumping = true;
					_jumpCount++;
					_whenJumped = Time.time;
					_whenInputtedJump = 0f;
					// GetComponent<WalkMovementModule>().EnableSurfaceClasping = false;
					Ground.Enabled = false;

					OnBeginJump();
				}
				else
					OnJumpFailed();
			}
		}

		#endregion
		#region OnBeginJump

		/// <summary>
		/// Applies an instantaneous jumping force to <see cref="Controller.Rigidbody"/>.
		///</summary>
		/// <remarks>
		/// Override this function to define how this jump should be executed.
		///</remarks>

		protected virtual void OnBeginJump()
		{
			// try
			// {
			// 	Animation.Animator.SetBool(Animation.Parameters[0], true);
			// }
			// catch { }
		}

		#endregion

		#region CeaseJump

		/// <summary>
		/// Stops jumping whether caused by holding the jump button too long or by releasing the jump button.
		///</summary>
		public void CeaseJump()
		{
			_isJumping = false;

			if (Enabled)
			{
				OnCeaseJump();
			}
		}

		#endregion
		#region OnCeaseJump

		/// <summary>
		/// This event is called when we successfully <see cref="CeaseJump"/>ing.
		///</summary>
		protected virtual void OnCeaseJump()
		{
			// try
			// {
			// 	Animation.Animator.SetBool(Animation.Parameters[0], false);
			// }
			// catch { }
		}

		#endregion

		#region OnJumpFailed

		/// <summary>
		/// This function is called when we attempt to jump (while this module is enabled) but are prevented from doing so.
		///</summary>
		protected virtual void OnJumpFailed() { }

		#endregion

		#region OnGroundFloored

		/// <summary>
		/// Event called when we land on the ground.
		///</summary>

		public virtual void OnGroundFloored(HitResult hit)
		{
			_jumpCount = 0;

			if (Time.time < _whenInputtedJump + InputGraceTime)
				BeginJump();
		}

		#endregion
		#region OnGroundLeft

		public virtual void OnGroundLeft()
		{
			_jumpCount++;
			_whenJumped = Time.time;
			_whenInputtedJump = 0f;
		}

		#endregion

		#endregion
	}

	#endregion
	#region JumpModule

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	[RequireComponent(typeof(GroundSensorModule))]

	public class JumpModule : JumpModuleBase<Pawn, GroundSensorModule, Vector3>
	{
		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();
		}

		#endregion
		#region FixedUpdate

		public override void ApplyMotion()
		{
			if (isJumping)
			{
				Pawn.Rigidbody.AddForce(Pawn.moveUpVector * JumpThrust, ForceMode.Acceleration);
			}
		}

		#endregion

		#region RefreshEvents

		protected sealed override void RefreshEvents()
		{
			{
				if (EnableJumpFromSlopes)
				{
					Ground.Events.OnTouch.AddListener(OnGroundFloored);
					Ground.Events.OnAirborne.AddListener(OnGroundLeft);

					Ground.Events.OnLand.RemoveListener(OnGroundFloored);
					Ground.Events.OnSlip.RemoveListener(OnGroundLeft);
				}
				else
				{
					Ground.Events.OnLand.AddListener(OnGroundFloored);
					Ground.Events.OnSlip.AddListener(OnGroundLeft);

					Ground.Events.OnTouch.RemoveListener(OnGroundFloored);
					Ground.Events.OnAirborne.RemoveListener(OnGroundLeft);
				}
			}

			#endregion
		}

		protected override void OnBeginJump()
		{
			base.OnBeginJump();

			Pawn.velocity = Vector3.Scale(Pawn.velocity, Vector3.one - Pawn.moveUpVector);
			Pawn.Rigidbody.AddForce(Pawn.moveUpVector * JumpStrength, ForceMode.VelocityChange);
		}
	}

	#endregion
}
