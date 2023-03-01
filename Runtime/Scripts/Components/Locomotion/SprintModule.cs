
/** SprintModule.cs
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
	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public abstract class SprintModule<TPawn, TWalkModule> : ActuationModule
	{
		#region Public Fields

		#region WalkModule

		/// <summary>
		/// Reference to the Walk Movement Module that this module modifies.
		///</summary>

		private TWalkModule _Walk;

		/// <inheritdoc cref="_Walk"/>

		public TWalkModule Walk => _Walk;

		#endregion

		#region Speed

		/// <summary>
		/// Speed to use when we want to sprint.
		///</summary>

		[Tooltip("Speed to use when we want to sprint.")]
		[Min(0f)]

		public float Speed = 15f;

		#endregion
		#region Angle

		/// <summary>
		/// Acceptance angle for sprinting. If our character forward direction and our movement direction diverges beyond this angle limit, we cannot sprint and/or must cancel sprinting.
		///</summary>

		[Tooltip("Acceptance angle for sprinting. If our character forward direction and our movement direction diverges beyond this angle limit, we cannot sprint and/or must cancel sprinting. A value of 180 will nullify this limitation.")]
		[Range(0f, 180f)]

		public float MaxAngle = 180f;

		#endregion
		#region ForceSprintWhenActive

		/// <summary>
		/// If enabled, we will force our player to move at top sprinting speed if they are holding sprint.
		///</summary>

		[Tooltip("If enabled, we will force our player to move at top sprinting speed if they are holding sprint.")]

		public bool ForceSprintWhenActive = true;

		#endregion

		#endregion
		#region Members

		#region UnsprintWalkSpeed

		/// <summary>
		/// The standard walking speed while not sprinting. This value is initialized <see cref="OnValidate"/>.
		///</summary>

		protected float _UnsprintWalkSpeed;

		public float UnsprintWalkSpeed => _UnsprintWalkSpeed;

		#endregion
		#endregion
		#region Properties
		#endregion
		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_Walk ??= GetComponent<TWalkModule>();
		}

		#endregion

		#endregion
	}

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public class SprintModule : SprintModule<Pawn, WalkModule>, IPawnModule<Pawn>
	{
		#region Properties

		#region Pawn

		/// <inheritdoc cref="IPawnModule.Pawn"/>

		private Pawn _Pawn;
		public Pawn Pawn => _Pawn;

		#endregion
		#region IsMovingInSprintRadius

		protected bool isMovingInSprintRadius
		{
			get
			{
				if (MaxAngle >= 180f) return true;

				float angle = Vector3.Angle(
					Pawn.lateralVelocityWorld.normalized,
					Pawn.moveForwardVector
				);

				return angle <= MaxAngle;
			}
		}

		#endregion

		#endregion
		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_UnsprintWalkSpeed = Walk.MaxWalkSpeed;

			Handler.OnBeginActuation.AddListener(() =>
			{
				Walk.MaxWalkSpeed = Speed;
			});

			Handler.OnCeaseActuation.AddListener(() =>
			{
				Walk.MaxWalkSpeed = UnsprintWalkSpeed;
			});
		}

		#endregion
		#region BeginCondition

		protected override bool BeginCondition() =>
			isMovingInSprintRadius;

		#endregion
		#region CeaseCondition

		protected override bool CeaseCondition() => base.CeaseCondition() ||
			!Walk.Ground.isGrounded;

		#endregion

		#endregion
	}
}
