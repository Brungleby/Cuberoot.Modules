
/** SpringArm.cs
*
*	Created by LIAM WOFFORD of CUBEROOT SOFTWARE, LLC.
*
*	Free to use or modify, with or without creditation,
*	under the Creative Commons 0 License.

*/


#region Includes

using UnityEngine;

#endregion

namespace Cuberoot
{
	/// <summary>
	/// This is a script that controls the position and rotation of <see cref="Child"/>. It will attempt to keep it at the <see cref="MaxDistance"/> away from this transform's position but pull in <see cref="Child"/> closer if it is blocked.
	///</summary>
	/// <remarks>
	/// Great for use as a "camera boom".
	///</remarks>
	public class SpringArm : MonoBehaviour
	{
		#region Fields

		/// <summary>
		/// Layers that will block the spring arm.
		///</summary>
		[Tooltip("Layers that will block the spring arm.")]

		public LayerMask SensorLayers;

		/// <summary>
		/// The radius of the sensor that will detect interference.
		///</summary>
		[Tooltip("The radius of the sensor that will detect interference.")]
		[Min(0f)]

		public float SensorRadius = 0.35f;

		/// <summary>
		/// Length of time that it takes for this gameObject to adjust to the correct distance, in seconds.
		///</summary>
		[Tooltip("Length of time that it takes for this gameObject to adjust to the correct distance, in seconds.")]
		[Min(0f)]

		public float SmoothTime = 0f;

		/// <summary>
		/// The position (relative to this transform's parent) at which the spring arm will "compress" towards.
		///</summary>
		[InspectorName("Desired Offset")]
		[Tooltip("The position (relative to this transform's parent) at which the spring arm will \"compress\" towards.")]

		public Vector3 _OriginPositionLocal;
		/// <summary>
		/// Minimum distance this gameObject is allowed to be from its parent.
		///</summary>
		[Tooltip("Minimum distance this gameObject is allowed to be from its parent.")]
		[Min(0f)]
		[SerializeField]

		private float _MinDistance;

		/// <inheritdoc cref="_MinDistance"/>

		public float MinDistance
		{
			get => _MinDistance;
			set => _MinDistance = value;
		}

		#endregion
		#region Members

		/// <summary>
		/// The desired location that this transform will try to stay at. It is defined by <see cref="transform.position"/> in <see cref="OnValidate"/>.
		///</summary>

		private Vector3 _extentPositionLocal;

		/// <summary>
		/// Private variable used to determine <see cref="transform.position"/>.
		///</summary>

		private float _distanceVelocity;

		#endregion
		#region Properties

		/// <summary>
		/// Maximum distance this gameObject is allowed to be from its parent. It is defined by the magnitude of <see cref="transform.localPosition"/>.
		///</summary>

		public float MaxDistance
		{
			get => _extentPositionLocal.magnitude;
			set => _extentPositionLocal = _extentPositionLocal.normalized * value;
		}

		/// <summary>
		/// The current distance the gameObject is from its parent.
		///</summary>
		/// <returns>
		/// The magnitude of <see cref="transform.localPosition"/>.
		///</returns>

		public float distance =>
			(transform.localPosition - _OriginPositionLocal).magnitude;

		/// <summary>
		/// The percentage (between 0 and 1) that our <see cref="distance"/> is between <see cref="MinDistance"/> and <see cref="MaxDistance"/>.
		///</summary>

		public float distancePercent =>
			Mathf.InverseLerp(MinDistance, MaxDistance, distance);

		/// <summary>
		/// The world position at which we will compress to.
		///</summary>
		/// <returns>
		/// <see cref="transform.parent.position"/> + <see cref="_OriginPositionLocal"/>, relative to <see cref="transform.rotation"/>.
		///</returns>

		public Vector3 originPosition =>
			transform.parent.position + transform.rotation * _OriginPositionLocal;

		/// <summary>
		/// The world position at which we will extend to.
		///</summary>
		/// <returns>
		/// <see cref="transform.parent.position"/> + <see cref="_extentPositionLocal"/>, relative to <see cref="transform.rotation"/>.
		///</returns>

		public Vector3 extentPosition =>
			transform.parent.position + transform.rotation * _extentPositionLocal;

		#endregion
		#region Methods

		private void OnValidate()
		{
			_extentPositionLocal = transform.localPosition;
			MinDistance = _MinDistance;
		}

		private void Awake() => OnValidate();

		private void Update()
		{
			#region Perform Cast

			Vector3 __castVector = extentPosition - originPosition;

			RaycastHit[] __hits = UnityEngine.Physics.SphereCastAll(
				originPosition, SensorRadius,
				__castVector.normalized, __castVector.magnitude,
				SensorLayers, QueryTriggerInteraction.Ignore
			);

			RaycastHit? __shortestHit = null;
			float __shortest = MaxDistance;
			foreach (RaycastHit hit in __hits)
			{
				if (hit.distance < __shortest)
				{
					__shortestHit = hit;
					__shortest = hit.distance;
				}
			}

			#endregion
			#region Adjust Position

			float __targetDistance = Mathf.Max(MinDistance, __shortestHit.HasValue ? __shortestHit.Value.distance : __castVector.magnitude);
			float __percent = Mathf.SmoothDamp(distance, __targetDistance, ref _distanceVelocity, SmoothTime) / __castVector.magnitude;

			transform.position = Vector3.Lerp(originPosition, extentPosition, __percent);

			#endregion
		}

		private void OnDrawGizmosSelected()
		{
			DebugDraw.DrawSphereCast(originPosition, extentPosition, SensorRadius, transform.position);
		}

		#endregion
	}
}
