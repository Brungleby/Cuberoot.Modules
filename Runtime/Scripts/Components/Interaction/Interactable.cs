
/** Interactable.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.

*/

#region Includes

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Cuberoot.Modules
{
	/// <summary>
	/// Invokes an event when an <see cref="Interactor"/> triggers an interaction with this <see cref="GameObject"/>.
	///</summary>

	public class Interactable : MonoBehaviour
	{
		#region Fields

		#region (event) OnInteract

		/// <summary>
		/// This event is called whenever an <see cref="Interactor"/> successfully triggers and <see cref="Interaction"/> on this <see cref="Interactable"/>.
		///</summary>

		[Tooltip("This event is called whenever an Interactor successfully triggers and Interaction on this component.")]
		[SerializeField]

		private UnityEvent<Interaction> _OnInteract;
		public UnityEvent<Interaction> OnInteract => _OnInteract;

		#endregion

		#endregion
		#region Members
		#endregion
		#region Functions

		#region ReceiveInteractionFrom

		/// <summary>
		/// <see cref="Interactor"/> calls this function. This function will invoke <see cref="OnInteract"/> if all conditions are met.
		///</summary>

		public bool ReceiveInteractionFrom(Interactor user, out Interaction result)
		{
			if (IsUnlockedForUser(user))
			{
				result = CreateInteractionWith(user);
				OnInteract.Invoke(result);

				return true;
			}

			result = default(Interaction);
			return false;
		}

		/// <inheritdoc cref="ReceiveInteractionFrom(Interactor, out Interaction)"/>

		public bool ReceiveInteractionFrom(Interactor user)
		{
			if (IsUnlockedForUser(user))
			{
				Interaction result = CreateInteractionWith(user);
				OnInteract.Invoke(result);

				return true;
			}

			return false;
		}

		#endregion
		#region InteractWith

		/// <summary>
		/// Creates an <see cref="Interaction"/> object to return to <paramref name="user"/> and <see cref="OnInteract"/>.
		///</summary>

		protected virtual Interaction CreateInteractionWith(Interactor user)
		{
			Interaction result = new Interaction(user, this);

			return result;
		}

		#endregion
		#region IsUnlockedForUser

		/// <summary>
		/// Conditional function; determines if this <see cref="Interactable"/> is currently usable for the given <paramref name="user"/>.
		///</summary>

		public virtual bool IsUnlockedForUser(Interactor user) => true;

		#endregion

		#endregion
	}

	/// <summary>
	/// A record of an instance of an interaction between an <see cref="Interactor"/> "<see cref="User"/>" and an <see cref="Interactable"/> "<see cref="Target"/>".
	///</summary>

	public class Interaction : object
	{
		public Interaction(Interactor _user, Interactable _target)
		{
			User = _user;
			Target = _target;

			WhenTriggered = UnityEngine.Time.time;
		}

		/// <summary>
		/// Default name for any action.
		///</summary>

		readonly public static string DEFAULT_INTERACTION_NAME = "Interact";

		/// <summary>
		/// The <see cref="Interactor"/> who triggered this <see cref="Interaction"/> upon the <see cref="Target"/>.
		///</summary>

		readonly public Interactor User;

		/// <summary>
		/// The <see cref="Interactable"/> who received this <see cref="Interaction"/> from the <see cref="User"/>.
		///</summary>

		readonly public Interactable Target;

		/// <summary>
		/// The <see cref="UnityEngine.Time"/> at which this <see cref="Interaction"/> was triggered.
		///</summary>

		readonly public float WhenTriggered;
	}
}
