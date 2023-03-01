
/** InteractableFeeler.cs
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
	/// Attach this to any <see cref="GameObject"/> with a <see cref="Collider"/> to allow that collider to sense for <see cref="Interactable"/>s.
	///</summary>

	[RequireComponent(typeof(Collider))]

	public class InteractableTriggerFeelerModule : InteractableSensorModule, ISensorTriggerFeelerModule
	{
		#region Collision Functions

		public void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent<Interactable>(out Interactable interactable) && interactable != null)
				AddTarget(interactable);
		}
		public void OnTriggerExit(Collider other)
		{
			if (other.TryGetComponent<Interactable>(out Interactable interactable) && interactable != null)
				RemoveTarget(interactable);
		}

		#endregion
	}

}
