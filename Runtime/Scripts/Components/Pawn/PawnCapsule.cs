
/** PawnCapsule.cs
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

	public sealed class PawnCapsule : Pawn<CapsuleCollider>
	{
		#region SkinnedRadius

		public float SkinnedRadius => ColliderShape.radius + SkinWidth;

		#endregion
		#region (group) Members

		private float _DefaultHeight;

		#endregion

		#region (override) Height

		public override float height
		{
			get => ColliderShape.height;
			set => ColliderShape.height = value;
		}

		private void Awake()
		{
			_DefaultHeight = height;
		}


		/// <summary>
		/// Sets the height and offsets the capsule as if the pawn were on the ground.
		///</summary>

		public void SetHeightOnGround(float value)
		{
			height = value;
			ColliderShape.center = Vector3.down * (_DefaultHeight - value) / 2f;
		}

		#endregion
	}
}
