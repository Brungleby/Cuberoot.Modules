
/** CuberootTemplate.cs
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
	/// Provides methods with which to find <see cref="Engine.Interaction.Interactable"/>s.
	///</summary>

	public abstract class InteractableSensorModule : Module
	{
		#region Fields

		#region User

		/// <summary>
		/// <see cref="Interactor"/> that is used to filter out unavailable <see cref="Interactables"/>.
		///</summary>

		[SerializeField]

		private Interactor _User;

		/// <inheritdoc cref="_User"/>

		public Interactor User => _User;

		#endregion

		#endregion
		#region Members

		#region Target

		/// <summary>
		/// The primary <see cref="Engine.Interaction.Interactable"/> that this sensor is currently focused on.
		///</summary>
		/// <remarks>
		/// <see cref="Target"/> will always exist within <see cref="ProspectiveTargets"/>.
		///</remarks>

		private Interactable _Target;

		/// <inheritdoc cref="_Target"/>

		public Interactable Target => _Target;

		#endregion
		#region ProspectiveTargets

		/// <summary>
		/// The list of ALL <see cref="Interactables"/> that this sensor is currently able to find.
		///</summary>

		private List<Interactable> _ProspectiveTargets = new List<Interactable>();

		/// <inheritdoc cref="_ProspectiveTargets"/>

		public List<Interactable> ProspectiveTargets => _ProspectiveTargets;

		#endregion

		#endregion
		#region Properties

		#region (runtime) IsInterested

		/// <summary>
		/// This value will be TRUE as long as this sensor has a valid <see cref="Target"/>.
		///</summary>

		public bool isInterested => Target != null;

		#endregion

		#endregion
		#region Functions

		#region (override) OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_User ??= GetComponent<Interactor>();
		}

		#endregion

		#region OnTargetsUpdated

		/// <summary>
		/// Refreshes <see cref="Target"/> when <see cref="ProspectiveTargets"/> is updated/modified.
		///</summary>

		public void OnTargetsUpdated() =>
			_Target = GetPreferredTarget(_ProspectiveTargets);

		#endregion

		#region GetPreferredTarget

		/// <remarks>
		/// This function is strictly concerned with prioritizing viable <paramref name="targets"/>. Only targets that <see cref="Engine.Interaction.Interactable.IsUnlockedForUser(Interactor)"/> should be considered for preferential filtering.
		///</remarks>
		/// <returns>
		/// The target which this sensor should primarily focus on, given a list of viable <paramref name="targets"/> which are enabled (but not necessarily unlocked).
		///</returns>

		public virtual Modules.Interactable GetPreferredTarget(List<Modules.Interactable> targets)
		{
			if (targets.Count == 0) return null;
			return targets[targets.Count - 1];
		}

		#endregion
		#region AddTarget

		/// <summary>
		/// Whenever an <see cref="Engine.Interaction.Interactable"/> moves into the sensable range of this sensor, this function is called to add it to our list of possible <see cref="ProspectiveTargets"/>, as long as it is unlocked for our <see cref="User"/>.
		///</summary>
		/// <param name="interactable">
		/// This is the <see cref="Engine.Interaction.Interactable"/> we have sensed and are considering to add to our list of viable targets.
		///</param>
		/// <returns>
		/// TRUE if the object was successfully added, else FALSE.
		///</returns>

		protected bool AddTarget(Modules.Interactable interactable)
		{
			if (interactable == null) return false;
			if (interactable.IsUnlockedForUser(User))
			{
				_ProspectiveTargets.Add(interactable);
				OnTargetsUpdated();

				return true;
			}

			return false;
		}

		#endregion
		#region RemoveTarget

		/// <summary>
		/// Whenever an <see cref="Engine.Interaction.Interactable"/> moves out of the sensable range of this sensor, this function is called to remove it from our list of possible <see cref="ProspectiveTargets"/>.
		///</summary>
		/// <param name="interactable">
		/// The <see cref="Engine.Interaction.Interactable"/> which has just left our sensable range.
		///</param>
		/// <returns>
		/// TRUE if it was successfully removed, FALSE if <paramref name="interactable"/> was never in the list to begin with.
		///</returns>

		protected bool RemoveTarget(Modules.Interactable interactable)
		{
			bool result = _ProspectiveTargets.Remove(interactable);

			if (result)
				OnTargetsUpdated();

			return result;
		}

		#endregion

		#endregion
	}


	#region InteractableCasterBase

	/// <summary>
	/// This senses for <see cref="Interactable"/>s by casting for them.
	///</summary>
	/// <summary>
	/// An <see cref="IInteractableSensor"/> that performs a linecast to find nearby <see cref="Interactable"/>s to interact with.
	///</summary>

	public abstract class InteractableCasterModule<THitResult, TVector> : InteractableSensorModule, ISensorCasterModule<THitResult[]>
	where THitResult : HitResult<TVector>
	where TVector : unmanaged
	{
		public LayerMask Layers { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		public float MaxDistance { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		public THitResult[] CastResult { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		public THitResult[] UpdateCast(float deltaTime)
		{
			throw new System.NotImplementedException();
		}

		#region Fields
		#endregion
		#region Members
		#endregion
		#region Properties
		#endregion
		#region Functions
		#endregion

		#region (interface) Targets

		/// <inheritdoc cref="IInteractableSensor.Targets_RT"/>

		public List<Interactable> targets
		{
			get
			{
				var result = new List<Interactable>();
				foreach (var combo in targetCombos)
				{
					result.Add(combo.Item1);
				}
				return result;
			}
		}

		#endregion
		#region TargetCombos

		/// <summary>
		/// Tuple list containing <see cref="Interactable"/>s sensed and each's corresponding <see cref="HitInfo"/>. 
		///</summary>

		private List<(Interactable, THitResult)> _targetCombos;

		/// <inheritdoc cref="_targetCombos"/>

		public List<(Interactable, THitResult)> targetCombos => _targetCombos;

		#endregion
		#region Target

		/// <inheritdoc cref="IInteractableSensor.Target_RT"/>

		private (Interactable, THitResult) _targetCombo;

		/// <inheritdoc cref="IInteractableSensor.Target_RT"/>

		public Interactable target => _targetCombo.Item1;

		/// <summary>
		/// The <see cref="HitInfo"/> associated with the current <see cref="target"/>.
		///</summary>

		public THitResult targetHitResult => _targetCombo.Item2;

		#endregion

		#endregion
		#region Properties
		#endregion
		#region Methods

		#region OnSenseUpdate

		protected override void Update()
		{
			_targetCombos = ConvertHitsToTargets(CastResult);
			_targetCombo = GetPreferredTarget(_targetCombos);

		}
		#endregion

		#region GetPreferredTarget

		/// <remarks>
		/// This function is strictly concerned with prioritizing viable <paramref name="targets"/>. Only targets that <see cref="Interactable.IsUnlockedForUser(Interactor)"/> should be considered for preferential filtering.
		///</remarks>
		/// <returns>
		/// The target which this sensor should primarily focus on, given a list of viable <paramref name="targets"/> which are enabled (but not necessarily unlocked).
		///</returns>

		public virtual (Interactable, THitResult) GetPreferredTarget(List<(Interactable, THitResult)> targets)
		{
			(Interactable, THitResult) result = (null, default);

			float closestDistance = targets[0].Item2.distance;

			foreach (var target in targets)
			{
				if (closestDistance <= target.Item2.distance)
				{
					result = target;
					closestDistance = target.Item2.distance;
				}
			}

			return result;
		}

		#endregion
		#region ConvertHitsToTargets

		/// <summary>
		/// Given the result of a HitInfo.CastAll, this creates a list of <see cref="Interactable"/>s and their associated <see cref="HitInfo"/>s.
		///</summary>

		private List<(Interactable, THitResult)> ConvertHitsToTargets(THitResult[] hits)
		{
			var result = new List<(Interactable, THitResult)>();

			foreach (var hit in hits)
			{
				/** Potentially VERY inefficient code
				*/
				var interactable = hit.transform.GetComponent<Interactable>();

				if (interactable != null) result.Add((interactable, hit));
			}

			return result;
		}

		#endregion

		#endregion
	}

}

