
/** MovementModule.cs
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
	#region (class) MovementModule

	public abstract class MovementModule : Module, IMovementModule
	{
		protected override void FixedUpdate() =>
			ApplyMotion();

		public abstract void ApplyMotion();
	}

	#endregion
	#region (class) MovementModule<TRigidbody>

	/// <summary>
	/// This type of module will move a provided Rigidbody on <see cref="FixedUpdate"/>.
	///</summary>

	public abstract class MovementModule<TRigidbody> : MovementModule
	{
		#region Members

		private TRigidbody _Rigidbody;
		public TRigidbody Rigidbody => _Rigidbody;

		#endregion
		#region Methods

		protected override void OnValidate()
		{
			base.OnValidate();

			_Rigidbody = GetComponent<TRigidbody>();
		}

		#endregion
	}

	#endregion
	#region (class) (abstract) MovementModule_RequirePawn

	public abstract class MovementModule_RequirePawn<TPawn> : MovementModule, IPawnModule<TPawn>
	{
		#region Fields

		#region Pawn

		[SerializeField]

		private TPawn _Pawn;
		public TPawn Pawn => _Pawn;

		#endregion

		#endregion
		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_Pawn = GetComponent<TPawn>();
		}

		#endregion

		#endregion
	}

	#endregion
	#region (class) MovementModule_OptionalPawn

	public abstract class MovementModule_OptionalPawn<TRigidbody, TPawn> : MovementModule<TRigidbody>, IPawnModule<TPawn>
	{
		#region Members

		#region Pawn

		private TPawn _Pawn;
		public TPawn Pawn => _Pawn;

		#endregion

		#endregion

		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_Pawn = GetComponent<TPawn>();

			_ApplyMotion_Func = _Pawn == null ? ApplyMotion_WithoutPawn : ApplyMotion_WithPawn;
		}

		#endregion

		#region ApplyMotion

		private System.Action _ApplyMotion_Func;

		public override void ApplyMotion() =>
			_ApplyMotion_Func();

		protected abstract void ApplyMotion_WithoutPawn();
		protected virtual void ApplyMotion_WithPawn() =>
			ApplyMotion_WithoutPawn();

		#endregion

		#endregion
	}

	#endregion
}
