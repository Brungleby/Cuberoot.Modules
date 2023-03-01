
/** PositionFollower.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using UnityEngine;
using Unity.Mathematics;

#endregion

namespace Cuberoot
{
	/// <summary>
	/// Updates this transform to follow its parent's position instantly or over time.
	///</summary>

	public class PositionFollower : FollowerBase
	{
		#region Public Fields

		/// <summary>
		/// The amount of time it takes for this follower to catch up with its parent's position, in seconds.
		///</summary>
		[Tooltip("The amount of time it takes for this follower to catch up with its parent's position, in seconds.")]
		[Min(0f)]

		public float PositionalLagTime = 0f;

		/// <summary>
		/// The maximum distance this follower can be apart from its parent.
		///</summary>
		[Tooltip("The maximum distance this follower can be apart from its parent.")]
		[Min(0f)]

		public float MaxDistance = 1f;

		/// <summary>
		/// Enabled axes will be affected; disabled axes will move independently.
		///</summary>
		[Tooltip("Enabled axes will be affected; disabled axes will move independently.")]

		public bool3 EnableAxes = new bool3(true);

		/// <summary>
		/// This value will snap the target position to a grid of this size.
		///</summary>
		[Tooltip("This value will snap the target position to a grid of this size. Set to 0 for no step.")]
		[Min(0f)]

		public float SnapGridSize = 0f;

		public Vector3 SnapGridOffset;

		#endregion
		#region Members

		/// <summary>
		/// Private variable used to calculate <see cref="transform.position"/>.
		///</summary>

		private Vector3 _positionVelocity;

		#endregion

		#region Methods

		#region Update

		protected override void Update()
		{
			Vector3 __targetPosition;
			if (SnapGridSize == 0f)
				__targetPosition = Anchor.position;
			else
				__targetPosition = Anchor.position.SnapToGrid(SnapGridSize, SnapGridOffset);

			Vector3 __adjustedPosition;
			if (PositionalLagTime == 0f)
				__adjustedPosition = __targetPosition;
			else
			{
				Vector3 __startPosition;
				Vector3 __differenceVector = transform.position - __targetPosition;
				if (__differenceVector.magnitude > MaxDistance)
					__startPosition = __targetPosition + __differenceVector.normalized * MaxDistance;
				else
					__startPosition = transform.position;

				__adjustedPosition = Vector3.SmoothDamp(__startPosition, __targetPosition, ref _positionVelocity, PositionalLagTime);
			}

			transform.position = new Vector3(
				EnableAxes.x ? __adjustedPosition.x : transform.position.x,
				EnableAxes.y ? __adjustedPosition.y : transform.position.y,
				EnableAxes.z ? __adjustedPosition.z : transform.position.z
			);
		}

		#endregion

		#endregion
	}
}
