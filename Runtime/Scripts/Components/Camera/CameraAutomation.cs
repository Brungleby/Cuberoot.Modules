
/** CameraAutomation.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.
*/

#region Includes

using UnityEngine;
using InputContext = UnityEngine.InputSystem.InputAction.CallbackContext;

#endregion

namespace Cuberoot.Modules
{
	#region (class) CameraAutomationBase

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public abstract class CameraAutomationBase : CasterModule<HitResult>
	{
		#region Fields

		/// <summary>
		/// The CameraController that this will affect.
		///</summary>
		[Tooltip("The CameraController that this will affect.")]

		public CameraController Controller;

		/// <summary>
		/// The base strength (speed) of the automation effect.
		///</summary>
		[Tooltip("The base strength (speed) of the automation effect.")]

		public Vector2 Strength = Vector2.one;

		/// <summary>
		/// This timeline defines how to reconstitute this automation effect after a period of disabling it.
		///</summary>
		[Tooltip("This timeline defines how to reconstitute this automation effect after a period of disabling it.")]

		public Timer AutoResetTimeline;

		#endregion
		#region Members

		private Vector2 _rawInputVector;
		public Vector2 rawInputVector => _rawInputVector;

		#endregion
		#region Properties

		public virtual Transform castTransform => transform;

		#endregion
		#region Methods
		#region (override) Awake

		protected override void Awake()
		{
			base.Awake();

			AutoResetTimeline.Start();
		}

		#endregion

		#region (override) Update

		protected override void Update()
		{
			AutoResetTimeline.Update();
		}

		#endregion

		#region (input) OnInputAxis

		public void OnInputAxis(InputContext context)
		{
			_rawInputVector = context.ReadValue<Vector2>();
		}

		#endregion
		#region (input) OnInputCheckReset

		public void OnInputCheckRestart(InputContext context)
		{
			var input = context.ReadValue<Vector2>();

			if (input != Vector2.zero)
				AutoResetTimeline.Restart();
		}

		#endregion
		#region (input) OnInputReset

		public void OnInputReset(InputContext context)
		{
			AutoResetTimeline.Cancel();
		}

		#endregion

		#endregion
	}

	#endregion
	#region (class) CameraAutomation

	public sealed class CameraAutomation : CameraAutomationBase
	{
		#region Fields

		/// <summary>
		/// The transform from which to perform a sensory cast.
		///</summary>
		[Tooltip("The transform from which to perform a sensory cast.")]
		[SerializeField]

		private Transform _CastTransform;

		/// <summary>
		/// The size of the cast to perform.
		///</summary>
		[Tooltip("The size of the cast to perform.")]
		[SerializeField]

		private float _CastRadius;

		public float PitchHeight = 45f;

		public float PitchSpeed = 1f;

		#endregion
		#region Properties

		public override Transform castTransform => _CastTransform;

		#endregion
		#region Methods
		#region (override) Update

		protected override void Update()
		{
			Vector3 __flatNormal = Vector3.Scale(transform.forward, new Vector3(1f, 0f, 1f)).normalized;
			CastResult = HitResult.SphereCast(castTransform.position, _CastRadius, __flatNormal, MaxDistance, Layers);

			base.Update();

			float __percent = AutoResetTimeline.isPlaying ? AutoResetTimeline.Evaluate() : 1f;

			float __squaredInputX = Math.Pow(rawInputVector.x, 2f) * rawInputVector.x.Sign();
			float __y = transform.localEulerAngles.y + (__squaredInputX * __percent * Strength.x);

			float __x;
			if (CastResult.isValid)
				__x = Mathf.Lerp(transform.localEulerAngles.x, PitchHeight * CastResult.percent, PitchSpeed * __percent * Time.deltaTime);
			else
				__x = transform.localEulerAngles.x;

			// print(CastTransform.position);

			Vector3 __angles = new Vector3(__x, __y, transform.localEulerAngles.z);

			transform.localEulerAngles = __angles;
		}

		#endregion

		#endregion
		#region OnDrawGizmos

		private void OnDrawGizmos()
		{
			CastResult.DrawLinecast();
		}

		#endregion
	}

	#endregion
}
