
/** FollowerBase.cs
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
	/// This is the base class for any kind of script that creates (or uses) an <see cref="Anchor"/> to replace its parent, and then follow that anchor in some way.
	///</summary>

	public abstract class FollowerBase : MonoBehaviour
	{
		#region Fields

		[Tooltip("Specify a different Transform for this object to follow. If none is select, this object's parent will be used.")]

		public Transform ParentOverride;

		[Tooltip("Quick fix for a larger issue that requires more in-depth debugging. For now, set this value to TRUE if this is the end of a chain of Followers.")]

		public bool IsEndOfChain = false;

		#endregion
		#region Members

		/// <summary>
		/// This is the transform that this GameObject will follow. It is NOT the transform that this GameObject was originally attached to.
		///</summary>

		[HideInInspector]

		public Transform Anchor;

		private List<FollowerBase> FollowerChildren = new List<FollowerBase>();

		#endregion
		#region Properties

		/// <summary>
		/// This is the transform that the <see cref="Anchor"/> is currently attached to.
		///</summary>

		public Transform Leader
		{
			get => Anchor.parent;
			set => Anchor.SetParent(value, false);
		}

		#endregion
		#region Functions

		#region Awake

		protected virtual void Awake()
		{
			foreach (var follower in GetComponents<FollowerBase>())
			{
				if (follower.Anchor)
				{
					Anchor = follower.Anchor;
					break;
				}
			}

			if (!Anchor)
			{
				Anchor = new GameObject($"{gameObject.name} (Anchor)").transform;
				Anchor.position = transform.position;
				Anchor.rotation = transform.rotation;
				Anchor.SetParent(ParentOverride != null ? ParentOverride : transform.parent, true);

				transform.parent = null;
			}

			if (IsEndOfChain)
				Invoke("RefreshHierarchy", 0f);
		}

		#endregion
		#region Update

		protected abstract void Update();

		#endregion
		#region RefreshHierarchy

		public void RefreshHierarchy()
		{
			enabled = false;
			enabled = true;
		}

		#endregion

		#endregion
	}
}
