
/** Module.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

#endregion

namespace Cuberoot.Modules
{
	#region (class) Module

	/// <summary>
	/// A Module can employ functions that help a GameObject sense something and/or move. If a Module belongs to a ModuleController, it will only execute its updates when the Controller enables it.
	///</summary>

	public abstract class Module : MonoBehaviour
	{
		#region Members

		#region Controllers

		private HashSet<Module> _controllers = new HashSet<Module>();

		#endregion

		[HideInInspector]
		public UnityEvent OnEnabled;
		[HideInInspector]
		public UnityEvent OnDisabled;

		#endregion

		#region Properties

		public bool Enabled
		{
			get => enabled;
			set
			{
				if (enabled == value)
					return;

				enabled = value;

				if (value)
				{
					OnEnable();
					OnEnabled.Invoke();
				}
				else
				{
					OnDisable();
					OnDisabled.Invoke();
				}
			}
		}

		public bool isControlledExternally => _controllers.Count != 0;

		#endregion
		#region Methods

		#region OnValidate

		protected virtual void OnValidate() { }

		#endregion
		#region Awake

		protected virtual void Awake() => OnValidate();

		#endregion
		#region Update

		protected virtual void Update() { }

		#endregion
		#region FixedUpdate

		protected virtual void FixedUpdate() { }

		#endregion

		#region IsControlledBy

		public bool IsControlledBy(Module other) => _controllers.Contains(other);

		#endregion
		#region AddController

		public void AddController(Module other)
		{
			if (this == other) throw new UnityException("AddController exception: Modules cannot externally control themselves.");

			_controllers.Add(other);
		}

		#endregion
		#region RemoveController

		public bool RemoveController(Module other) => _controllers.Remove(other);

		#endregion

		#region OnEnabled

		protected virtual void OnEnable() { }

		#endregion
		#region OnDisabled

		protected virtual void OnDisable() { }

		#endregion

		#endregion
	}

	#endregion

	#region Interfaces

	#region IModuleAnimator

	public interface IModuleAnimator
	{
		Animator Animator { get; }

		void UpdateAnimation();
	}

	#endregion

	#region IPawnModule

	public interface IPawnModule<TPawn>
	{
		TPawn Pawn { get; }
	}

	public interface IPawnModule<TCollider, TRigidbody, TMovementSpace>
	{
		Pawn<TCollider, TRigidbody, TMovementSpace> Pawn { get; }
	}

	#endregion

	public interface IPawnModuleOptional<TPawn>
	{
#nullable enable

		TPawn? Pawn { get; }

#nullable restore
	}

	#region IMovementModule

	/**
	*	An IModuleConveyer moves or otherwise modifies the state of a Module.gameObject.
	*/

	public interface IMovementModule
	{
		void ApplyMotion();
	}

	#endregion

	#region Sensors

	/**
	*	A Sensor provides sensory information to the Module. It should never modify any aspect of a Module directly.
	*/

	public interface ISensorCollisionFeelerModule
	{
		void OnCollisionEnter(Collision other);
		void OnCollisionExit(Collision other);
	}

	public interface ISensorTriggerFeelerModule
	{
		void OnTriggerEnter(Collider other);
		void OnTriggerExit(Collider other);
	}

	public interface ISensorCasterModule<THitResult>
	{
		LayerMask Layers { get; set; }
		float MaxDistance { get; set; }

		THitResult CastResult { get; set; }
	}

	#endregion

	#endregion
}
