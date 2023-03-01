
/** GravityModule.cs
*
*   Created by LIAM WOFFORD, USA-TX.
*/

#region Includes

using System.Collections.Generic;

using UnityEngine;

#endregion

namespace Cuberoot.Modules
{
	/// <summary>
	/// This is the base class for a module that constantly accelerates its owner in a certain direction. This class defaults to using <see cref="UnityEngine.Physics.gravity"/>.
	///</summary>

	public class GravityModuleBase : MovementModule<Rigidbody>
	{
		#region Public Fields

		#region (field) Scale

		/// <summary>
		/// The final scalar to apply to the gravity force.
		///</summary>

		[Tooltip("The final scalar to apply to the gravity force.")]
		[SerializeField]

		public float Scale = 1f;

		#endregion

		#endregion

		#region Properties

		#region GravityForce

		/// <summary>
		/// The base gravity vector (direction and strength) in which to accelerate the owner.
		///</summary>

		public virtual Vector3 force => UnityEngine.Physics.gravity;

		#endregion
		#endregion
		#region Methods

		public sealed override void ApplyMotion()
		{
			Rigidbody.AddForce(force * Scale, ForceMode.Acceleration);
		}

		#endregion
	}

	/// <summary>
	/// This module allows for a change in gravity force, depending on the presence of nearby <see cref="GravityRegion"/>s.
	///</summary>

	public class GravityModule : GravityModuleBase, ISensorTriggerFeelerModule
	{
		#region Public Fields



		#endregion
		#region Members

		private List<GravityRegion> _affectingRegions = new List<GravityRegion>();

		#endregion

		#region Properties

		public virtual Vector3 evalPosition => transform.position;

		public virtual Vector3 defaultForce => base.force;

		public sealed override Vector3 force
		{
			get
			{
				if (_affectingRegions.Count == 0)
					return defaultForce;

				Vector3 result = Vector3.zero;

				foreach (var region in _affectingRegions)
					result += region.GetForce(evalPosition);

				return result;
			}
		}

		#endregion
		#region Methods

		public void OnTriggerEnter(Collider other)
		{
			var __region = other.GetComponent<GravityRegion>();
			if (__region != null && !_affectingRegions.Contains(__region))
				_affectingRegions.Add(__region);
		}

		public void OnTriggerExit(Collider other)
		{
			var __region = other.GetComponent<GravityRegion>();
			if (__region != null)
				_affectingRegions.Remove(__region);
		}

		#endregion
	}
}
