
/** PawnDescriptor.cs
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
	#region (class) PawnBase

	/// <summary>
	/// The base class that defines a pawn and collates its data from multiple other sources.
	///</summary>

	public abstract class PawnBase : MonoBehaviour
	{
		#region Fields

		#region (field) SkinWidth

		/// <summary>
		/// Denotes the thickness around this <see cref="UnityEngine.Collider"/> that can be cast in a <see cref="PawnSensorBase"/>.
		///</summary>

		[Tooltip("This value is used in PawnSensor components. It denotes the thickness around this Pawn's Collider that will be cast. Smaller numbers are stiffer but more accurate, larger numbers are looser but less accurate.")]

		public float SkinWidth = 0.01f;

		#endregion

		#region (field) Debug Show Speed
#if UNITY_EDITOR
		public bool Debug_ShowSpeed = false;
#endif
		#endregion

		#endregion
		#region Members

		#region PreviousFixedDeltaTime

		/// <summary>
		/// Record of <see cref="Time.fixedDeltaTime"/> from the previous frame.
		///</summary>

		protected float _fixedDeltaTime_Previous = 1f;

		#endregion

		#endregion
		#region Properties

		#region (abstract) Height

		/// <summary>
		/// Shorthand for the length of the Pawn's collider's vertical axis.
		///</summary>

		public abstract float height { get; set; }

		#endregion
		#region (abstract) Speed

		/// <summary>
		/// The magnitude (absolute value) of <see cref="Velocity"/>.
		///</summary>

		public abstract float speed { get; set; }

		#endregion

		#endregion
		#region Methods

		#region FixedUpdate

		protected virtual void FixedUpdate()
		{
			_fixedDeltaTime_Previous = Time.fixedDeltaTime;
		}

		#endregion

		#region () OnDrawGizmosSelected

		protected virtual void OnDrawGizmosSelected()
		{
#if UNITY_EDITOR
			UnityEditor.Handles.Label(transform.position - Vector3.up * ((height / 2f) + 0.25f), speed.ToString("0.0"));
#endif
		}

		#endregion

		#endregion
	}

	#endregion
	#region (class) Pawn<TCollider, TRigidbody, TMovementSpace>

	public abstract class Pawn<TCollider, TRigidbody, TMovementSpace> : PawnBase
	{
		#region Fields

		#region (field) Collider

		/// <summary>
		/// Generic reference to the Collider associated with this Pawn.
		///</summary>

		[Tooltip("Generic reference to the Collider associated with this Pawn.")]
		[SerializeField]

		private TCollider _Collider;

		/// <inheritdoc cref="_Collider"/>

		public TCollider Collider => _Collider;

		#endregion

		#region (field) Rigidbody

		/// <summary>
		/// The Rigidbody associated with this Pawn.
		///</summary>

		[Tooltip("The Rigidbody associated with this Pawn.")]
		[SerializeField]

		private TRigidbody _Rigidbody;

		/// <inheritdoc cref="_Rigidbody"/>

		public TRigidbody Rigidbody => _Rigidbody;

		#endregion

		#region (field) Velocity

		/// <summary>
		/// The current velocity of the Rigidbody.
		///</summary>

		[Tooltip("The current velocity of the Rigidbody. Modifying this value in-editor will set the initial velocity.")]
		[SerializeField]

		private TMovementSpace _velocity = default(TMovementSpace);

		#endregion

		#endregion
		#region Private Members

		#region PreviousVelocity

		/// <summary>
		/// Record of <see cref="velocity"/> from the previous frame.
		///</summary>

		protected TMovementSpace _velocity_Previous = default(TMovementSpace);

		#endregion

		#endregion

		#region Properties

		#region (abstract) MoveUpVector

		/// <summary>
		/// When inputting controls to this Pawn, this is the direction which this Pawn perceives as up.
		///</summary>

		public abstract TMovementSpace moveUpVector { get; }

		#endregion
		#region (abstract) MoveForwardVector

		/// <summary>
		/// When inputting controls to this Pawn, this is the direction which this Pawn perceives as forward.
		///</summary>

		public abstract TMovementSpace moveForwardVector { get; }

		#endregion

		#region (abstract) Velocity

		/// <inheritdoc cref="_velocity"/>

		public virtual TMovementSpace velocity
		{
			get => _velocity;
			set => _velocity = value;
		}

		#endregion

		#region (abstract) VerticalVelocity

		/// <summary>
		/// The current uniaxial velocity of this Pawn, relative to <see cref="moveUpVector"/>.
		///</summary>

		public abstract float verticalVelocityLocal { get; set; }

		#endregion
		#region VerticalSpeed

		/// <summary>
		/// The absolute value of <see cref="verticalVelocityLocal"/>.
		///</summary>

		public float verticalSpeed
		{
			get => verticalVelocityLocal.Abs();
			set => verticalVelocityLocal = System.Math.Max(value, 0f) * verticalVelocityLocal.Sign();
		}

		#endregion

		#region (abstract) Acceleration

		/// <summary>
		/// The current acceleration of the <see cref="Rigidbody"/> based on its <see cref="velocity"/>.
		///</summary>

		public abstract TMovementSpace acceleration { get; }

		#endregion

		#endregion
		#region Methods

		#region (override) FixedUpdate

		protected override void FixedUpdate()
		{
			base.FixedUpdate();

			_velocity_Previous = velocity;
		}

		#endregion

		#region ExtrapolatePosition

		/**
		*   __TODO_REVIEW__
		*   These functions don't need to use an input position/velocity...or do they?
		*/

		/// <summary>
		/// Predicts where this Pawn will be in a specified number of <paramref name="frames"/>.
		///</summary>

		public abstract TMovementSpace ExtrapolatePosition(in TMovementSpace position, in TMovementSpace velocity, in float deltaTime, int frames = 1);

		public abstract TMovementSpace ProjectVelocityOntoSurface(in TMovementSpace position, in TMovementSpace velocity, in TMovementSpace planeNormal);

		public abstract TMovementSpace ProjectVelocityOntoSurface(in TMovementSpace planeNormal);

		#endregion

		#endregion
	}

	#endregion
	public abstract class Pawn : Pawn<Collider, Rigidbody, Vector3>
	{
		#region Properties

		#region MoveRightVector

		/// <summary>
		/// When inputting controls to this Pawn, this is the direction which this Pawn perceives as right.
		///</summary>

		public Vector3 moveRightVector => transform.right;

		#endregion
		#region (sealed override) MoveUpVector

		public override Vector3 moveUpVector => Vector3.up;

		#endregion
		#region (sealed override) MoveForwardVector

		public override Vector3 moveForwardVector => transform.forward;

		#endregion

		#region Velocity

		public sealed override Vector3 velocity
		{
			get => Rigidbody.velocity;
			set => Rigidbody.velocity = value;
		}

		public Vector3 velocityRelative
		{
			get => lateralVelocityLocal.XZ() + Vector3.up * verticalVelocityLocal;
		}

		public sealed override float speed
		{
			get => velocity.magnitude;
			set => velocity = velocity.normalized * value;
		}

		#endregion

		#region LateralVelocity

		/// <summary>
		/// The current biaxial velocity of this Pawn, relative to <see cref="moveRightVector"/> and <see cref="moveForwardVector"/>.
		///</summary>

		public Vector2 lateralVelocityLocal
		{
			get => new Vector2(
				Vector3.Dot(velocity, moveRightVector),
				Vector3.Dot(velocity, moveForwardVector)
			);
			set => velocity = verticalVelocityWorld +
				(moveRightVector * value.x) +
				(moveForwardVector * value.y)
			;
		}

		/// <summary>
		/// The vector in 3D space that represents only the <see cref="lateralVelocityLocal"/>.
		///</summary>

		public Vector3 lateralVelocityWorld =>
			Vector3.Scale(velocity, Vector3.one - moveUpVector);

		/// <summary>
		/// The absolute value magnitude of <see cref="lateralVelocityLocal"/>.
		///</summary>

		public float lateralSpeed
		{
			get => lateralVelocityLocal.magnitude;
			set => lateralVelocityLocal = lateralVelocityLocal.normalized * value;
		}

		#endregion

		#region VerticalVelocity

		public sealed override float verticalVelocityLocal
		{
			get => Vector3.Dot(velocity, moveUpVector);
			set => velocity =
				lateralVelocityWorld +
				(moveUpVector * value);
		}

		/// <summary>
		/// The vector in 3D space that represents only the <see cref="verticalVelocityLocal"/>.
		///</summary>

		public Vector3 verticalVelocityWorld =>
			Vector3.Scale(velocity, moveUpVector);

		#endregion

		#region (sealed override) Acceleration

		public sealed override Vector3 acceleration =>
			(velocity - _velocity_Previous) / _fixedDeltaTime_Previous;

		#endregion

		#endregion

		#region Methods

		#region (sealed override) ExtrapolatePosition

		public sealed override Vector3 ExtrapolatePosition(in Vector3 position, in Vector3 velocity, in float deltaTime, int frames = 1)
		{
			return position + (velocity * deltaTime * frames);
		}

		#endregion
		#region (sealed override) ProjectVelocityOntoSurface

		public sealed override Vector3 ProjectVelocityOntoSurface(in Vector3 planeNormal)
		{
			return ProjectVelocityOntoSurface(Rigidbody.position, Rigidbody.velocity, planeNormal);
		}

		public sealed override Vector3 ProjectVelocityOntoSurface(in Vector3 position, in Vector3 velocity, in Vector3 planeNormal)
		{
			Vector3 projected_current = Vector3.ProjectOnPlane(position, planeNormal);
			Vector3 projected_next = Vector3.ProjectOnPlane(ExtrapolatePosition(position, velocity, Time.fixedDeltaTime, 5), planeNormal);

			return (projected_next - projected_current).normalized * velocity.magnitude;
		}

		#endregion

		#endregion
	}

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public abstract class Pawn<TCollider> : Pawn
	where TCollider : Collider
	{
		#region Fields
		#endregion
		#region Members
		#endregion
		#region Properties

		#region (field) Collider

		/// <summary>
		/// The specific shape of this pawn's <see cref="Collider"/>.
		///</summary>

		public TCollider ColliderShape => (TCollider)Collider;

		#endregion

		#endregion
	}
}
