
/** GhostMovementModule.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using UnityEngine;
using InputContext = UnityEngine.InputSystem.InputAction.CallbackContext;

#endregion

namespace Cuberoot.Modules
{
	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>
	public abstract class GhostMovementModuleBase<TCollider, TRigidbody, TMovementSpace> : MovementModule<TRigidbody>, IPawnModule<TCollider, TRigidbody, TMovementSpace>
	{
		#region Public Fields

		#region (field) Pawn

		/// <summary>
		/// The pawn that this module should report to.
		///</summary>
		[Tooltip("The pawn that this module should report to.")]
		[SerializeField]

		private Pawn<TCollider, TRigidbody, TMovementSpace> _Pawn;

		/// <inheritdoc cref="_Pawn"/>

		public Pawn<TCollider, TRigidbody, TMovementSpace> Pawn => _Pawn;

		#endregion

		#region (field) OwnerController

		/// <summary>
		/// The controller that this module should report to.
		///</summary>
		[Tooltip("The controller that this module should report to.")]
		[SerializeField]

		private ModuleController _Controller;

		#endregion
		#region (field) ViewTransform

		/// <summary>
		/// The Transform used to determine the desired direction of movement.
		///</summary>
		[Tooltip("The Transform used to determine the desired direction of movement.")]
		[SerializeField]

		public Transform ViewTransform;

		#endregion
		#region (field) BecomeModeID

		/// <summary>
		/// If using a <see cref="ModuleController"/>, this is the mode that will be selected <see cref="OnInputBecomeMode"/>.
		///</summary>
		[Tooltip("If using a ModuleController, this is the mode that will be selected OnInputBecomeMode. It should be the mode that contains this module.")]
		[Range(0, 31)]
		[SerializeField]

		public int SelfModeIndex = 0;

		#endregion
		#region (field) FallbackModeID

		/// <summary>
		/// If using a <see cref="ModuleController"/>, this is the mode that will be returned to <see cref="OnInputCancelMode"/> by default.
		///</summary>
		[Tooltip("If using a ModuleController, this is the mode that will be returned to OnInputCancel by default. If set to -1, it will return the last mode")]
		[Range(-1, 31)]
		[SerializeField]

		private int _CancelModeIndex = -1;
		public int CancelModeIndex =>
			(_CancelModeIndex >= 0) ? _CancelModeIndex : _previousModeIndex;

		private int _previousModeIndex;

		#endregion

		#region (field) FlySpeed

		/// <summary>
		/// Default flying speed.
		///</summary>
		[Tooltip("Default flying speed.")]
		[SerializeField]

		public float FlySpeed = 10f;

		#endregion
		#region (field) BoostMultiplier

		/// <summary>
		/// Multiplies <see cref="FlySpeed"/> <see cref="OnInputBoost"/>.
		///</summary>
		[Tooltip("Multiplies FlySpeed OnInputBoost.")]
		[SerializeField]

		public float BoostMultiplier = 5f;

		#endregion

		#endregion
		#region Members

		#region (member) RawInputVector

		/// <summary>
		/// Raw input vector directly from Input.
		///</summary>
		protected TMovementSpace _rawInputVector = default(TMovementSpace);

		#endregion
		#region (member) InputVector

		/// <summary>
		/// Input vector in world space, adjusted by <see cref="ViewTransform"/>.
		///</summary>
		protected TMovementSpace _inputVector = default(TMovementSpace);

		#endregion
		#region (member) IsBoosting

		/// <summary>
		/// Whether or not this module should currently be boosting.
		///</summary>
		private bool _isBoosting = false;

		/// <inheritdoc cref="_isBoosting"/>

		public bool isBoosting => _isBoosting;

		#endregion

		#endregion

		#region Properties

		public float desiredFlySpeed => FlySpeed * (isBoosting ? BoostMultiplier : 1f);

		#endregion
		#region Methods

		protected override void OnValidate()
		{
			_Pawn ??= GetComponent<Pawn<TCollider, TRigidbody, TMovementSpace>>();
			_Controller ??= GetComponent<ModuleController>();

			base.OnValidate();


			SelfModeIndex = SelfModeIndex.Min(_Controller.ModeRegistry.Count);
			_CancelModeIndex = _CancelModeIndex.Min(_Controller.ModeRegistry.Count);
		}

		protected override void Awake()
		{
			base.Awake();

			_ResetPreviousModeIndex();
		}

		private void _ResetPreviousModeIndex()
		{
			if (_Controller.activeModeIndex == SelfModeIndex)
				_previousModeIndex = SelfModeIndex == 0 ? 1 : 0;
			else
				_previousModeIndex = _Controller.activeModeIndex;
		}

		#region (input) OnInputToggleMode

		public void OnInputToggleMode(InputContext context)
		{
			if (context.started)
			{
				if (enabled)
				{
					_Controller.SetActiveMode(CancelModeIndex);
				}

				else
				{
					_ResetPreviousModeIndex();
					_Controller.SetActiveMode(SelfModeIndex);
#if UNITY_EDITOR
					UnityEngine.Debug.Log("You feel ethereal within");
#endif
				}
			}
		}

		#endregion

		#region (input) (abstract) OnInputMove

		public abstract void OnInputMove(InputContext context);

		#endregion
		#region (input) OnInputSprint

		public void OnInputBoost(InputContext context)
		{
			if (context.started)
				_isBoosting = true;
			else if (context.canceled)
				_isBoosting = false;
		}

		#endregion

		#endregion
	}

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>
	public class GhostMovementModule : GhostMovementModuleBase<Collider, Rigidbody, Vector3>
	{
		#region Methods

		#region OnEnabled

		protected override void OnEnable()
		{
			Pawn.Collider.enabled = false;
			Pawn.velocity = Vector3.zero;
		}

		#endregion
		#region OnDisabled

		protected override void OnDisable()
		{
			Pawn.Collider.enabled = true;
			Pawn.velocity = Vector3.zero;
		}

		#endregion

		#region (override) Apply Motion

		public override void ApplyMotion()
		{
			_inputVector = ViewTransform.rotation * _rawInputVector;

			Pawn.velocity = _inputVector * desiredFlySpeed * Time.fixedDeltaTime;
		}

		#endregion

		#region (input) (override) OnInputMove

		public sealed override void OnInputMove(InputContext context) =>
			_rawInputVector = context.ReadValue<Vector3>();

		public void OnInputMoveBiaxial(InputContext context) =>
			_rawInputVector = context.ReadValue<Vector2>().XZ();

		#endregion

		#endregion
	}
}
