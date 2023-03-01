
/** ModuleController.cs
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
using UnityEngine.Events;

#endregion

namespace Cuberoot.Modules
{
	/// <summary>
	/// This component controls other Modules and tells which Modules to execute, and when.
	///</summary>

	public class ModuleController : Module
	{
		#region (class) Mode

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		[System.Serializable]

		public class Mode : List<Module>
		{
			#region Constructors

			public Mode(string _id, Module[] _components, bool _enabled = false)
			{
				ID = _id;

				_Components = new List<Module>();
				_Components.AddAll(_components);

				Enabled = _enabled;
			}

			#endregion

			#region Static Constants

			public const string DEFAULT_NAME = "New Component Mode";

			#endregion

			#region Public Fields

			#region (field) ID

			/// <summary>
			/// The name of the mode, used to compare equality.
			///</summary>

			[Tooltip("The name of this mode, used to compare equality.")]
			[SerializeField]

			public string ID = DEFAULT_NAME;

			#endregion
			#region (field) Components

			/// <summary>
			/// List of components that this Mode enables.
			///</summary>

			[Tooltip("List of components that this Mode enables.")]
			[SerializeField]

			private List<Module> _Components;

			/// <inheritdoc cref="_Components"/>

			public List<Module> Components => _Components;

			#endregion

			#region (field) OnEnabled

			/// <summary>
			/// This event is called when this mode becomes enabled after being disabled.
			///</summary>

			[Tooltip("This event is called when this mode becomes enabled after being disabled.")]
			[SerializeField]

			private UnityEvent _OnEnabled;

			/// <inheritdoc cref="_OnEnabled"/>

			public UnityEvent OnEnabled => _OnEnabled;

			#endregion
			#region (field) OnDisabled

			/// <summary>
			/// This event is called when this mode becomes disabled after being enabled.
			///</summary>

			[Tooltip("This event is called when this mode becomes disabled after being enabled.")]
			[SerializeField]

			private UnityEvent _OnDisabled;

			/// <inheritdoc cref="_OnDisabled"/>

			public UnityEvent OnDisabled => _OnDisabled;

			#endregion

			#endregion
			#region Members

			#region Enabled

			/// <summary>
			/// Whether or not all of this <see cref="Components"/> should be enabled or not.
			///</summary>

			private bool _Enabled;

			/// <inheritdoc cref="_Enabled"/>

			private bool Enabled
			{
				get => _Enabled;
				set
				{
					if (_Enabled == value)
						return;

					_Enabled = value;

					if (value)
						OnEnabled.Invoke();
					else
						OnDisabled.Invoke();

					// foreach (var component in _Components)
					// 	component.Enabled = value;
				}
			}

			#endregion

			#endregion
			#region Methods

			public void SwitchFromThisTo(Mode other)
			{
				foreach (var module in _Components)
					if (!other._Components.Contains(module))
						module.Enabled = false;

				foreach (var module in other._Components)
					if (!_Components.Contains(module))
						module.Enabled = true;

				Enabled = false;
				other.Enabled = true;
			}

			public void Refresh()
			{
				foreach (var module in _Components)
				{
					module.Enabled = false;
					module.Enabled = true;
				}
			}

			#endregion
		}

		#endregion

		#region Fields

		#region (field) ActiveMovementMode

		/// <summary>
		/// The currently active movement mode.
		///</summary>

		[Tooltip("The currently active movement mode.")]
		[Range(0, 31)]
		[SerializeField]

		private int _activeModeIndex = 0;

		/// <inheritdoc cref="_activeModeIndex"/>

		public virtual int activeModeIndex
		{
			get => _activeModeIndex;
			set
			{
				if (!Enabled || _activeModeIndex == value)
					return;

				var __i = System.Math.Clamp(value, 0, ModeRegistry.Count - 1);
				ModeRegistry[_activeModeIndex].SwitchFromThisTo(ModeRegistry[__i]);
				_activeModeIndex = __i;
			}
		}

		#endregion
		#region (field) ModeRegistry

		/// <summary>
		/// The registry of modes that this controller governs.
		///</summary>

		[Tooltip("The registry of modes that this controller governs.")]
		[SerializeField]

		private List<Mode> _ModeRegistry = new List<Mode>();

		/// <inheritdoc cref="_ModeRegistry"/>

		public List<Mode> ModeRegistry => _ModeRegistry;

		#endregion

		#endregion
		#region Members
		#endregion

		#region Properties

		#region ActiveMode

		/// <inheritdoc cref="_activeModeIndex"/>

		public Mode activeMode
		{
			get => _ModeRegistry[_activeModeIndex];
			set
			{
				if (!Enabled) return;

				for (var i = 0; i < _ModeRegistry.Count; i++)
				{
					if (_ModeRegistry[i] == value)
					{
						activeModeIndex = i;
						return;
					}
				}

				throw new System.ArgumentOutOfRangeException();
			}
		}

		#endregion

		#endregion
		#region Methods

		protected override void OnValidate()
		{
			_activeModeIndex = System.Math.Clamp(_activeModeIndex, 0, System.Math.Max(0, ModeRegistry.Count - 1));

			foreach (var mode in ModeRegistry)
			{
				mode.Components.Remove(this);
			}

			Refresh();
		}

		#region IsControllingComponent

		/// <returns>
		/// TRUE if any <see cref="Component"/>s in the <see cref="ModeRegistry"/> match the <see cref="System.Type"/> <paramref name="query"/>.
		///</returns>

		public bool IsControllingComponent(System.Type query)
		{
			foreach (var mode in ModeRegistry)
				foreach (var component in mode.Components)
					if (query == component.GetType())
						return true;

			return false;
		}

		/// <returns>
		/// TRUE if the exact <see cref="Component"/> <paramref name="query"/> exists within the <see cref="ModeRegistry"/>.
		///</returns>

		public bool IsControllingComponent(Component query)
		{
			foreach (var mode in ModeRegistry)
				foreach (var component in mode.Components)
					if (query == component)
						return true;

			return false;
		}

		#endregion

		#region SetActiveMode

		/// <summary>
		/// Sets the active mode of this controller.
		///</summary>
		/// <param name="i">
		/// The index of the mode to activate.
		///</param>
		/// <returns>
		/// The newly active <see cref="Mode"/>. 
		///</returns>

		public Mode SetActiveMode(int i)
		{
			if (i >= ModeRegistry.Count)
				throw new System.IndexOutOfRangeException();

			activeModeIndex = i;
			return activeMode;
		}

		/// <inheritdoc cref="SetActiveMode"/>
		/// <param name="id">
		/// The ID of the mode in the registry to activate.
		///</param>

		public Mode SetActiveMode(string id)
		{
			for (var i = 0; i < _ModeRegistry.Count; i++)
				if (_ModeRegistry[i].ID == id)
				{
					activeModeIndex = i;
					return activeMode;
				}

			throw new KeyNotFoundException();
		}

		#endregion

		#region RefreshActiveMode

		public void Refresh()
		{
			foreach (var mode in _ModeRegistry)
				foreach (var module in mode.Components)
					module.Enabled = false;

			activeMode.Refresh();
		}

		#endregion

		#endregion
	}
}
