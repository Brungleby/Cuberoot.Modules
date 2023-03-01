
/** CameraController.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.

*/


#region Includes

using UnityEngine;
using Unity.Mathematics;

using InputContext = UnityEngine.InputSystem.InputAction.CallbackContext;

#endregion

namespace Cuberoot.Modules
{
	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public class CameraController : Module, IPawnModule<Pawn>
	{
		#region Fields

		#region Pawn

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		[Tooltip("__TODO_ANNOTATE__")]
		[SerializeField]

		private Pawn _Pawn;

		/// <inheritdoc cref="_Pawn"/>

		public Pawn Pawn => _Pawn;

		#endregion

		#region Camera

		/// <summary>
		/// Reference to the controlled camera.
		///</summary>

		[Tooltip("Reference to the controlled camera. Can be used to control FOV and other settings.")]

		public Camera Camera;

		#endregion
		#region ApplyRotation

		/// <summary>
		/// This component will drive the rotation of the pawn along each axis set to TRUE.
		///</summary>

		[Tooltip("This component will drive the rotation of the pawn along each axis set to TRUE. NOTE: If you are going to apply Yaw (Y) rotation, and are also using a WalkMovement component, please ensure that \"YawRotationMode\" is set to EXPLICIT. (This is typically used for tank controls or any first-person application.)")]

		public bool3 ApplyRotationToPawn = new bool3(false);

		#endregion

		[Space(10f)]

		#region Speed

		/// <summary>
		/// Rotation amount to apply per input unit.
		///</summary>

		[Tooltip("Rotation amount to apply per input unit.")]

		public Vector3 Speed = Vector3.one * 100f;


		#endregion
		#region (field) MouseSpeed

		/// <summary>
		/// Rotation amount to apply per input unit when using the mouse.
		///</summary>

		[Tooltip("Rotation amount to apply per input unit when using the mouse.")]
		[SerializeField]

		private Vector2 _MouseSpeed = Vector2.one * 100f;

		/// <inheritdoc cref="_MouseSpeed"/>

		public Vector2 MouseSpeed
		{
			get => _MouseSpeed / 120f;
			set => _MouseSpeed = value * 120f;
		}

		#endregion
		#region InvertInputAxes

		[Tooltip("Checked boxes will invert corresponding axes. X = pitch, Y = yaw, z = roll")]

		public bool3 InvertInputAxes = new bool3(true, false, false);

		#endregion
		#region SwapXYInput

		[Tooltip("If enabled, this will swap the X and Y components during 2D (biaxial) input processing. This is usually enabled because [X Mouse/Stick] usually controls [Y Camera Yaw].")]

		public bool SwapXYInput = true;

		#endregion

		[Space(10f)]

		#region EnableAngleLimits

		public bool3 EnableAngleLimits = new bool3(true, false, false);

		#endregion
		#region AngleLimits

		/// <summary>
		/// Maximum value each axis can be, positive and negative.
		///</summary>

		[Tooltip("Maximum value each axis can be, positive and negative.")]
		[SerializeField]

		private Vector3 _AngleLimits = new Vector3(90f, 0f, 0f);

		/// <inheritdoc cref="_AngleLimits"/>

		public Vector3 AngleLimits
		{
			get => _AngleLimits;
			set => _AngleLimits = value.Clamp(180f);
		}

		#endregion
		#region SpeedLimits

		/// <summary>
		/// Max angular speed.
		///</summary>
		[Tooltip("Max angular speed. Useful if you're using a camera with a RotationFollower and a mouse.")]
		[Min(0f)]
		public Vector3 SpeedLimits = Vector3.one * 15f;

		#endregion

		#endregion

		#region Members

		private Vector2 _rawMouseInputVector;
		private Vector3 _rawInputVector;

		#region Input Vector

		/// <summary>
		/// The adjusted world input euler angles vector.
		///</summary>

		private Vector3 _inputVector;

		/// <inheritdoc cref="_inputVector"/>

		public Vector3 inputVector => _inputVector;

		#endregion

		private Quaternion _initialRotation;

		private Vector3 _localPawnAngles;

		#endregion
		#region Properties

		#endregion
		#region EnableSpeedLimits

		public bool3 EnableSpeedLimits => new bool3(SpeedLimits.x == 0f, SpeedLimits.y == 0f, SpeedLimits.z == 0f);

		#endregion
		#region Methods

		protected override void OnValidate()
		{
			base.OnValidate();

			_Pawn = GetComponent<Pawn>();

			_AngleLimits = _AngleLimits.Clamp(180f);

			_initialRotation = transform.localRotation;

			if (_Pawn)
			{
				_localPawnAngles = Pawn.Rigidbody.transform.localEulerAngles;
			}
		}

		protected override void Update()
		{
			/** Calculate proper input vector
			*/
			_inputVector = new Vector3
			(
				_rawInputVector.x * (InvertInputAxes.x ? -1f : 1f),
				_rawInputVector.y * (InvertInputAxes.y ? -1f : 1f),
				_rawInputVector.z * (InvertInputAxes.z ? -1f : 1f)
			);

			Vector2 __adjRawMouseVector = new Vector2
			(
				_rawMouseInputVector.x * InvertInputAxes.x.ToFloatUnit(),
				_rawMouseInputVector.y * InvertInputAxes.y.ToFloatUnit()
			);
			Vector3 __mouseVector = __adjRawMouseVector.XY();

			// _InputVector = Controller.transform.up * _InputVector;

			/** Limit speed values on input
			*/
			if (EnableSpeedLimits.x)
				_inputVector.x = _inputVector.x.ClampAbs(SpeedLimits.x);
			if (EnableSpeedLimits.y)
				_inputVector.y = _inputVector.y.ClampAbs(SpeedLimits.y);
			if (EnableSpeedLimits.z)
				_inputVector.z = _inputVector.z.ClampAbs(SpeedLimits.z);

			/** Apply input to local euler angles
			*/
			Vector3 __deltaRotationEuler = Vector3.Scale(inputVector, Speed);
			Vector3 __deltaRotationMouse = Vector3.Scale(__mouseVector, MouseSpeed);

			Vector3 __localCameraAngles = transform.localEulerAngles + __deltaRotationEuler * Time.deltaTime + __deltaRotationMouse;
			_localPawnAngles += __deltaRotationEuler * Time.deltaTime + __deltaRotationMouse;

			/** Limit values on local euler angles
			*/
			if (EnableAngleLimits.x)
			{
				__localCameraAngles.x = __localCameraAngles.x.ClampAngle(_AngleLimits.x);
				_localPawnAngles.x = _localPawnAngles.x.ClampAngle(_AngleLimits.x);
			}
			if (EnableAngleLimits.y)
			{
				__localCameraAngles.y = __localCameraAngles.y.ClampAngle(_AngleLimits.y);
				_localPawnAngles.y = _localPawnAngles.y.ClampAngle(_AngleLimits.y);
			}
			if (EnableAngleLimits.z)
			{
				__localCameraAngles.z = __localCameraAngles.z.ClampAngle(_AngleLimits.z);
				_localPawnAngles.z = _localPawnAngles.z.ClampAngle(_AngleLimits.z);
			}

			/** Apply LocalAngles to rotation
			*/
			transform.localEulerAngles = __localCameraAngles;
		}

		public virtual void ResetRotation()
		{
			float __y = Pawn.transform.eulerAngles.y;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, __y, transform.eulerAngles.z);
		}

		public void OnInputCamera_Mouse(InputContext context)
		{
			Vector2 __input = context.ReadValue<Vector2>();

			if (SwapXYInput)
				Math.SwapValues(ref __input.x, ref __input.y);

			_rawMouseInputVector = __input;
		}

		/// <summary>
		/// Updates the <see cref="_rawInputVector"/> from a <see cref="Vector2"/>.
		///</summary>
		public void OnInputCamera_Biaxial(InputContext context)
		{
			Vector2 __input = context.ReadValue<Vector2>();

			if (SwapXYInput)
				Math.SwapValues(ref __input.x, ref __input.y);

			_rawInputVector = __input.XY();
		}

		/// <summary>
		/// Updates the <see cref="_rawInputVector"/> from a <see cref="Vector3"/>.
		///</summary>
		public void OnInputCamera_Triaxial(InputContext context)
		{
			_rawInputVector = context.ReadValue<Vector3>();
		}

		public void OnInputReset(InputContext context)
		{
			if (context.started)
				ResetRotation();
		}

		#endregion
	}
}
