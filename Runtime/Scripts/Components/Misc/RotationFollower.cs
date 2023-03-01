
/** RotationFollower.cs
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

namespace Cuberoot
{
	/// <summary>
	/// Updates this transform to follow its parent's rotation instantly or over time.
	///</summary>

	public class RotationFollower : FollowerBase
	{
		#region Fields

		#region RotationalLagTime

		/// <summary>
		/// The amount of time it takes for this follower to catch up with its parent's rotation, in seconds.
		///</summary>

		[Tooltip("The amount of time it takes for this follower to catch up with its parent's rotation, in seconds.")]
		[Min(0f)]

		public float RotationalLagTime = 0f;

		#endregion

		#endregion
		#region Members

		#region RotationVelocity

		/// <summary>
		/// Private variable used to calculate <see cref="transform.rotation"/>.
		///</summary>

		private Vector3 _rotationVelocity;

		#endregion

		#endregion

		#region Methods

		#region Update

		protected override void Update()
		{
			if (RotationalLagTime == 0f)
			{
				transform.rotation = Anchor.rotation;
			}
			else
			{
				transform.eulerAngles = Math.SmoothDampEulerAngles(transform.eulerAngles, Anchor.eulerAngles, ref _rotationVelocity, RotationalLagTime);
			}
		}

		#endregion

		#endregion
	}
}
