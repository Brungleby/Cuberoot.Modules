
/** Interactor.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using System.Collections.Generic;

using UnityEngine;

#endregion

namespace Cuberoot.Modules
{
	/// <summary>
	/// This component triggers interactions on <see cref="Interactable"/>s when an input is initiated.
	///</summary>

	public class Interactor : ActuationModule
	{
		#region Static Members

		#region DefaultInteractionID

		/// <inheritdoc cref="Interaction.DEFAULT_INTERACTION_NAME"/>

		readonly private static string _DefaultInteractionID = Interaction.DEFAULT_INTERACTION_NAME;

		#endregion

		#endregion
		#region Public Fields

		#region AttachedSensors

		/// <summary>
		/// The list of sensors that this Interactor may use to find <see cref="Interactable"/>s.
		///</summary>

		// [SerializeField] /** Unavailable because it is an interface **/
		private InteractableSensorModule[] _AttachedSensors;

		/// <inheritdoc cref="_AttachedSensors"/>

		public InteractableSensorModule[] AttachedSensors => _AttachedSensors;

		#endregion

		#endregion

		#region Properties

		#region TargetSensor

		/// <summary>
		/// The current Sensor with which we want to find <see cref="Interactable"/>s.
		///</summary>

		public virtual InteractableSensorModule targetSensor => AttachedSensors[0];

		#endregion
		#region InteractionID

		/// <summary>
		/// The current action we wish to perform. Used to filter which <see cref="Interactable"/>s are unlocked for us.
		///</summary>

		public virtual string InteractionID => _DefaultInteractionID;

		#endregion

		#endregion
		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_AttachedSensors = GetComponentsInChildren<InteractableSensorModule>();

			Handler.OnBeginActuation.AddListener(() =>
			{
				TryInteract();
			});
		}

		/// <summary>
		/// Attempts to interact using the the automatically assigned <see cref="targetSensor"/>.
		///</summary>
		/// <returns>
		/// TRUE if the interaction was allowed.
		///</returns>

		public bool TryInteract() => TryInteractUsing(targetSensor);

		/// <summary>
		/// Attempts to interact using the given <paramref name="sensor"/>.
		///</summary>
		/// <inheritdoc cref="TryInteract"/>

		public bool TryInteractUsing(InteractableSensorModule sensor) => TryInteractWith(sensor.Target);

		/// <summary>
		/// Attempts to interact using any sensor that has a valid target.
		///</summary>
		/// <inheritdoc cref="TryInteract"/>

		public bool TryInteractAny()
		{
			var sensorTargets = new List<Interactable>();

			foreach (var sensor in AttachedSensors)
			{
				if (sensor.Target != null)
					sensorTargets.Add(sensor.Target);
			}

			var theTarget = GetPreferredInteractable(sensorTargets);

			if (theTarget != null)
				return TryInteractWith(theTarget);

			return false;
		}

		/// <summary>
		/// Attempts to interact directly with a specified <see cref="interactable"/>.
		///</summary>
		/// <inheritdoc cref="TryInteract"/>

		public bool TryInteractWith(Interactable interactable)
		{
			if (interactable)
				return interactable.ReceiveInteractionFrom(this);
			return false;
		}

		/// <returns>
		/// The preferred or ideal selection, given a list of <paramref name="interactables"/>. 
		///</returns>

		protected virtual Interactable GetPreferredInteractable(List<Interactable> interactables) => interactables[0];

		#endregion

		#endregion
	}
}
