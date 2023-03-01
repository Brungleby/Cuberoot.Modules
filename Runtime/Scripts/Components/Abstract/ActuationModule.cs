
/** ActuationModule.cs
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
using InputContext = UnityEngine.InputSystem.InputAction.CallbackContext;

#endregion

namespace Cuberoot.Modules
{
	#region (class) ActuationModule

	/// <summary>
	/// This type of module defines how an button input is processed.
	///</summary>

	public class ActuationModule : Module
	{
		#region (inner class) ActuationHandler

		/// <summary>
		/// Defines how a <see cref="PawnActuation"/> should operate.
		///</summary>

		[System.Serializable]

		protected class ActuationHandler
		{
			#region Public Fields

			#region (field) Mode

			/// <summary>
			/// This will determine how the actuation will occur. Use <see cref="ActuationType.Auto"/> or <see cref="ActuationType.Toggle"/> for actions that require long presses, use <see cref="ActuationType.Button"/> for short-burst actions.
			///</summary>
			[Tooltip("This will determine how the actuation will occur. Use Auto or Toggle for actions that require long presses, use Button for short-burst actions.")]

			public ActuationType Mode = ActuationType.Auto;

			#endregion
			#region (field) AutoDelay

			/// <summary>
			/// Amount of time to wait after holding the input to use <see cref="ActuationType.Button"/> mode. This value doesn't affect anything if the <see cref="Mode"/> is not set to <see cref="ActuationType.Auto"/>.
			///</summary>
			[Tooltip("Amount of time to wait after holding the input to use Button mode. This value doesn't affect anything if the Mode is not set to Auto.")]

			public float AutoDelay = 0.2f;

			#endregion
			#region (field) EnableInterruption

			/// <summary>
			/// If enabled, actuation will be interrupted if the <see cref="BeginCondition"/> fails. If unsure: use TRUE.
			///</summary>
			[Tooltip("If enabled, actuation will be interrupted if the BeginCondition fails. If unsure: use TRUE.")]

			public bool EnableInterruption = true;

			#endregion
			#region (field) EnableResume

			/// <summary>
			/// If enabled, actuation will resume if the <see cref="BeginCondition"/> passes. If unsure: use TRUE.
			///</summary>
			[Tooltip("If enabled, actuation will resume if the BeginCondition passes. If unsure: use TRUE.")]

			public bool EnableResume = true;

			#endregion

			#region (field) OnBeginActuation

			/// <summary>
			/// This event is called when Actuation has "officially" begun.
			///</summary>
			[Tooltip("This event is called when Actuation has \"officially\" begun.")]
			[SerializeField]

			private UnityEvent _OnBeginActuation;

			/// <inheritdoc cref="_OnBeginActuation"/>

			public UnityEvent OnBeginActuation => _OnBeginActuation;

			#endregion
			#region (field) OnCeaseActuation

			/// <summary>
			/// This event is called when Actuation has "officially" ended.
			///</summary>

			[Tooltip("This event is called when Actuation has \"officially\" ended.")]
			[SerializeField]
			private UnityEvent _OnCeaseActuation;

			/// <inheritdoc cref="_OnCeaseActuation"/>

			public UnityEvent OnCeaseActuation => _OnCeaseActuation;

			#endregion
			#region (field) OnBeginActuationFailed

			/// <summary>
			/// This event is called when we try to actuate but are unsuccessful.
			///</summary>
			[Tooltip("This event is called when we try to actuate but are unsuccessful.")]
			[SerializeField]

			private UnityEvent _OnBeginActuationFailed;

			/// <inheritdoc cref="_OnBeginActuationFailed"/>

			public UnityEvent OnBeginActuationFailed => _OnBeginActuationFailed;

			#endregion

			#endregion
			#region Members

			#region Owner

			/// <summary>
			/// Owner of this <see cref="Handler"/>. This value is required to be set on Awake/OnValidate in order to trigger the proper functions.
			///</summary>
			[HideInInspector]

			public ActuationModule Owner;

			#endregion
			#region IsActuated

			/// <summary>
			/// Whether or not this actuator is currently actuating/active.
			///</summary>

			private bool _isActuated;

			/// <inheritdoc cref="_isActuated"/>
			/// <remarks>
			/// Setting this value will invoke <see cref="OnBeginActuation"/> or <see cref="OnCeaseActuation"/>.
			///</remarks>

			public bool isActuated
			{
				get => _isActuated;
				set
				{
					if (_isActuated != value)
					{
						if (useToggle)
							_desiredValue = value;

						if (value)
						{
							if (Owner.BeginCondition())
							{
								_isActuated = true;
								_actuatedTime = Time.time;

								OnBeginActuation.Invoke();
							}
							else
							{
								OnBeginActuationFailed.Invoke();
							}
						}
						else
						{
							_isActuated = false;

							OnCeaseActuation.Invoke();
						}
					}
				}
			}

			#endregion
			#region DesiredValue

			/// <summary>
			/// Whether or not this actuator "wants to" be active or not. This is ONLY set by input and is used for interruption/resuming.
			///</summary>

			private bool _desiredValue = false;

			/// <inheritdoc cref="_desiredValue"/>

			public bool desiredValue => _desiredValue;

			#endregion
			#region ActuatedTime

			/// <summary>
			/// The recorded time at which we last successfully set <see cref="_isActuated"/> to TRUE. Used to determine <see cref="useToggle"/> when our <see cref="Mode"/> = <see cref="ActuationType.Auto"/>.
			///</summary>

			private float _actuatedTime = -1f;

			/// <inheritdoc cref="_actuatedTime"/>

			public float actuatedTime => _actuatedTime;

			#endregion

			#endregion

			#region Properties

			#region UseToggle

			/// <summary>
			/// Whether or not this Actuator should act like a Button or a Toggle.
			///</summary>

			private bool useToggle =>
				Mode == ActuationType.Button ?
					false : Mode == ActuationType.Toggle ?
					true : Time.time < _actuatedTime + AutoDelay;

			#endregion

			#endregion
			#region ToggleIsActuated

			public void ToggleIsActuated()
			{
				_desiredValue = !isActuated;
				isActuated = _desiredValue;
			}

			#endregion

			#region OnInputActuated

			/// <summary>
			/// Input method routed from <see cref="ActuationAbility"/>.
			///</summary>

			public void OnInputActuated(InputContext context)
			{
				if (useToggle)
				{
					if (context.started)
						ToggleIsActuated();
				}
				else
				{
					if (context.started)
					{
						_desiredValue = true;
						isActuated = true;
					}
					else if (context.canceled)
					{
						_desiredValue = false;
						isActuated = false;
					}
				}
			}

			#endregion
		}

		#endregion

		#region Fields

		#region (field) Actuation

		/// <summary>
		/// The Actuator to use for this PawnActuation.
		///</summary>

		[SerializeField]

		protected ActuationHandler Handler;

		#endregion

		#endregion

		#region IsActuated

		/// <inheritdoc cref="ActuationHandler.isActuated"/>

		public bool isActuated => Handler.isActuated;

		#endregion
		#region Methods

		protected override void OnValidate()
		{
			base.OnValidate();

			Handler.Owner = this;
		}

		/// <returns>
		/// TRUE if this module is allowed to become active. It is called when attempting to become active.
		///</returns>

		protected virtual bool BeginCondition() =>
			true;

		/// <returns>
		/// TRUE if this module should STOP being active while already active. It is called every Update for which this module is active.
		///</returns>

		protected virtual bool CeaseCondition() =>
			!BeginCondition();

		/// <summary>
		/// Toggles actuation between active and inactive.
		///</summary>
		/// <param name="forceState">
		/// If TRUE, the toggle will occur even if this Module is disabled.
		///</param>

		public void Toggle(bool forceState = false)
		{
			if (Enabled || forceState)
				Handler.ToggleIsActuated();
		}

		public void OnInputActuated(InputContext context)
		{
			if (Enabled)
				Handler.OnInputActuated(context);
		}

		#endregion
	}

	#endregion
	#region (class) ActuationModule_RequirePawn

	public class ActuationModule_RequirePawn<TPawn> : ActuationModule
	{
		#region Members

		#region Pawn

		/// <inheritdoc cref="Pawn"/>

		private TPawn _Pawn;

		public TPawn Pawn => _Pawn;

		#endregion

		#endregion

		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();

			_Pawn = GetComponent<TPawn>();
		}

		#endregion

		#endregion
	}

	#endregion

	#region (class) ActuationControllerModule

	/// <summary>
	/// This is a special type of ActuationModule that controls other <see cref="Modules"/> when it is pressed and released.
	///</summary>

	public class ActuationControllerModule : ActuationModule
	{
		#region (field) ControlledModules

		/// <summary>
		/// These Modules are enabled when this Actuation is pressed and disabled when it is released.
		///</summary>

		[Tooltip("These Modules are enabled when this Actuation is pressed and disabled when it is released.")]
		[SerializeField]

		private List<Module> _ModulesEnabledOnPressed;

		/// <inheritdoc cref="_ModulesEnabledOnPressed"/>

		public List<Module> ModulesEnabledOnPressed => _ModulesEnabledOnPressed;

		#endregion
		#region (field) ModulesEnabledOnRelease

		/// <summary>
		/// These Modules are enabled when this Actuation is released and disabled when it is pressed.
		///</summary>

		[Tooltip("These Modules are enabled when this Actuation is released and disabled when it is pressed.")]
		[SerializeField]

		private List<Module> _ModulesEnabledOnRelease = new List<Module>();

		/// <inheritdoc cref="_ModulesEnabledOnRelease"/>

		public List<Module> ModulesEnabledOnRelease => _ModulesEnabledOnRelease;

		#endregion

		#region Methods

		protected override void OnValidate()
		{
			base.OnValidate();

			SetModulesEnabled(Handler.isActuated);

			Handler.OnBeginActuation.AddListener(
				() => { SetModulesEnabled(true); }
			);

			Handler.OnCeaseActuation.AddListener(
				() => { SetModulesEnabled(false); }
			);
		}

		private void SetModulesEnabled(bool value)
		{
			foreach (var module in _ModulesEnabledOnPressed)
				module.Enabled = value;

			foreach (var module in _ModulesEnabledOnRelease)
				module.Enabled = !value;
		}

		#endregion
	}

	#endregion

	#region (enum) ActuationType

	/// <summary>
	/// Defines whether an Actuator should act like a Button or a Toggle switch.
	///</summary>

	public enum ActuationType
	{
		/// <summary>
		/// The actuator will act like a button if it is held for longer than the specified Delay. Otherwise, it will act like a switch.
		///</summary>

		[Tooltip("The actuator will act like a button if it is held for longer than the specified Delay. Otherwise, it will act like a switch.")]

		Auto,

		/// <summary>
		/// The actuator will only be active as long as the input is held (or the condition fails).
		///</summary>

		[Tooltip("The actuator will only be active as long as the input is held (or the condition fails).")]

		Button,

		/// <summary>
		/// The actuator will always remain active until the input is triggered twice (or the condition fails).
		///</summary>

		[Tooltip("The actuator will always remain active until the input is triggered twice (or the condition fails).")]

		Toggle,
	}

	#endregion
}
