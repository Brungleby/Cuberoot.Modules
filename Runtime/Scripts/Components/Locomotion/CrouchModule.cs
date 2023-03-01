
/** CrouchModule.cs
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
	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public class CrouchModule : ActuationModule_RequirePawn<PawnBase>
	{
		#region Public Fields

		#region (field) OffsetTransform

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		[Tooltip("__TODO_ANNOTATE__")]
		[SerializeField]

		private Transform _OffsetTransform;

		/// <inheritdoc cref="_OffsetTransform"/>

		public Transform OffsetTransform
		{
			get => _OffsetTransform;
			set
			{
				_OffsetTransform = value;

				if (value != null)
				{
					UpdateOffsetHeight = _UpdateOffsetHeight;
					_defaultOffsetHeight = value.localPosition.y;
				}
				else
				{
					UpdateOffsetHeight = (newHeight) => { };
					_defaultOffsetHeight = 0f;
				}
			}
		}

		#endregion

		#region (field) CrouchedHeight

		/// <summary>
		/// The height of this capsule when crouching is actuated.
		///</summary>

		[Tooltip("The height of this capsule when crouching is actuated.")]
		[SerializeField]

		public float CrouchedHeight = 1f;

		#endregion

		#endregion
		#region (field) CrouchTime

		/// <summary>
		/// How long it takes to fully crouch/uncrouch, in seconds.
		///</summary>

		[Tooltip("How long it takes to fully crouch/uncrouch, in seconds.")]
		[SerializeField]

		public float CrouchTime = 0.25f;

		#endregion
		#region Members

		#region UncrouchedHeight

		/// <summary>
		/// Initialized <see cref="OnValidate"/>, this is the default height of the pawn.
		///</summary>

		private float _UncrouchedHeight;

		/// <inheritdoc cref="_UncrouchedHeight"/>

		public float UncrouchedHeight
		{
			get => _UncrouchedHeight;
			set => _UncrouchedHeight = value;
		}

		#endregion

		#region TargetHeightVelocity

		/// <summary>
		/// Variable used to calculate SmoothDamp value.
		///</summary>

		private float _targetHeightVelocity;

		#endregion
		#region DefaultOffsetHeight

		private float _defaultOffsetHeight;

		#endregion

		#endregion

		#region Properties

		#region TargetHeight

		/// <summary>
		/// The target height that we want to be at depending on whether or not we are actuating.
		///</summary>
		public float targetHeight => isActuated ? CrouchedHeight : UncrouchedHeight;

		#endregion

		#region HeightPercent

		/// <summary>
		/// This is the current percentage of height that we are at, measured from <see cref="CrouchedHeight"/> to <see cref="UncrouchedHeight"/>.
		///</summary>

		public float heightPercent =>
			Math.Remap(Pawn.height, CrouchedHeight, UncrouchedHeight);

		#endregion

		#endregion
		#region Methods

		#region (override) OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			OffsetTransform = _OffsetTransform;

			UncrouchedHeight = Pawn.height;
		}

		#endregion

		protected override void Update()
		{
			base.Update();

			var __newHeight = Mathf.SmoothDamp(Pawn.height, targetHeight, ref _targetHeightVelocity, CrouchTime);
			Pawn.height = __newHeight;

			UpdateOffsetHeight(__newHeight);
		}

		#endregion

		#region UpdateOffsetHeight

		private System.Action<float> UpdateOffsetHeight = (newHeight) => { };

		private void _UpdateOffsetHeight(float newHeight)
		{
			var __offset = (1f - heightPercent) * newHeight / 2f;

			OffsetTransform.localPosition = new Vector3(
				OffsetTransform.localPosition.x,
				_defaultOffsetHeight + __offset,
				OffsetTransform.localPosition.z
			);
		}

		#endregion
	}
}
