
/** GravityRegion.cs
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
	#region GravityRegionBase<TMovementSpace>

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public abstract class GravityRegionBase<TMovementSpace> : MonoBehaviour
	{
		#region Fields

		#region Strength

		/// <summary>
		/// The strength of the force of this region.
		///</summary>
		[Tooltip("The strength of the force of this region.")]
		[SerializeField]

		public float Strength = 9.8f;

		#endregion
		#region (field) InnerRadius

		/// <summary>
		/// The radius around the source of gravity at which the full force of gravity is experienced.
		///</summary>
		[Tooltip("The radius around the source of gravity at which the full force of gravity is experienced.")]
		[SerializeField]

		public float InnerRadius = 0f;

		#endregion

		#region (field) FalloffCurve

		/// <summary>
		/// Curve used to determine contextual strength between <see cref="InnerRadius"/> and <see cref="OuterRadius"/>.
		///</summary>
		[Tooltip("Curve used to determine contextual strength between InnerRadius and OuterRadius.")]
		[SerializeField]

		private AnimationCurve FalloffCurve = AnimationCurve.Constant(0f, 1f, 1f);

		#endregion

		#endregion

		#region Properties

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		public abstract float OuterRadius { get; set; }

		#endregion
		#region Methods

		#region GetGravityForce

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		public abstract TMovementSpace GetForce(TMovementSpace atPosition);

		#endregion
		#region GetFalloffPercent

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		public float GetFalloffPercent(float atDistance) => FalloffCurve.Evaluate(atDistance.Remap(InnerRadius, OuterRadius));

		#endregion

		#endregion
		#region Editor
#if UNITY_EDITOR

		protected virtual void OnDrawGizmosSelected() { }

#endif
		#endregion
	}

	#endregion
	#region GravityRegionBase<TCollider, TMovementSpace>

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public abstract class GravityRegionBase<TCollider, TMovementSpace> : GravityRegionBase<TMovementSpace>
	{
		#region Fields

		#region (field) Collider

		/// <summary>
		/// The affected trigger collider to get information from.
		///</summary>

		[Tooltip("The affected trigger collider to get information from.")]
		[SerializeField]

		private TCollider _Collider;

		/// <inheritdoc cref="_Collider"/>

		public TCollider Collider
		{
			get => _Collider;
			set
			{
				_Collider = value;
				RefreshShapeFuncs();
			}
		}

		#endregion
		#region (field) ForceLinear

		/// <summary>
		/// If TRUE, only the linear function will be used and the collider will simply be the trigger region.
		///</summary>

		[Tooltip("If TRUE, only the linear function will be used and the collider will simply be the trigger region.")]
		[SerializeField]

		private bool _ForceLinear = false;

		/// <inheritdoc cref="_ForceLinear"/>

		public bool ForceLinear => _ForceLinear;

		#endregion

		#endregion
		#region Members

		#region (func) GetForceFromShape

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		private System.Func<TMovementSpace, (TMovementSpace, float)> _GetForceFromShape;

		/// <inheritdoc cref="_GetForceFromShape"/>

		protected System.Func<TMovementSpace, (TMovementSpace, float)> GetForceFromShape => _GetForceFromShape;

		#endregion
		#region (func) GetRadiusFromShape

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		private System.Func<float> _GetRadiusFromShape;

		#endregion
		#region (func) SetRadiusFromShape

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		private System.Action<float> _SetRadiusFromShape;

		#endregion
		#endregion

		#region Properties

		public override float OuterRadius
		{
			get => _GetRadiusFromShape();
			set => _SetRadiusFromShape(value);
		}

		#endregion
		#region Methods

		#region OnValidate

		protected virtual void OnValidate()
		{
			if (_Collider == null)
				_Collider = GetComponent<TCollider>();

			RefreshShapeFuncs();
		}

		#endregion
		#region Awake

		protected virtual void Awake() => OnValidate();

		#endregion

		#region RefreshShapes

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		private void RefreshShapeFuncs()
		{
			_GetForceFromShape = GetShapeForceFunc(Collider);
			_GetRadiusFromShape = GetShapeOuterRadiusFunc(Collider);
			_SetRadiusFromShape = SetShapeOuterRadiusFunc(Collider);

#if UNITY_EDITOR
			_OnDrawGizmosSelectedFunc = GetOnDrawGizmosSelectedFunc(Collider);
#endif
		}

		#endregion
		#region GetShapeFunc

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		public abstract System.Func<TMovementSpace, (TMovementSpace, float)> GetShapeForceFunc(TCollider collider);

		#endregion
		#region GetShapeOuterRadiusFunc

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		public abstract System.Func<float> GetShapeOuterRadiusFunc(TCollider collider);

		#endregion
		#region SetShapeOuterRadiusFunc

		/// <summary>
		/// __TODO_ANNOTATE__
		///</summary>

		public abstract System.Action<float> SetShapeOuterRadiusFunc(TCollider collider);

		#endregion

		#endregion

		#region Debug
#if UNITY_EDITOR

		#region Public Fields

		#region (field) DebugDensity

		/// <summary>
		/// How many debug arrows should be drawn per cubic meter.
		///</summary>

		[Tooltip("How many debug arrows should be drawn per cubic meter.")]
		[SerializeField]

		private float _DebugDensity = 0.1f;

		/// <inheritdoc cref="_DebugDensity"/>

		protected float DebugDensity => _DebugDensity;

		#endregion
		#region (field) DebugMaxArrows

		/// <summary>
		/// The maximum number of arrows that can be drawn on screen.
		///</summary>

		[Tooltip("The maximum number of arrows that can be drawn on screen.")]
		[SerializeField]

		private int _DebugMaxArrows = 32;

		/// <inheritdoc cref="_DebugMaxArrows"/>

		protected int DebugMaxArrows => _DebugMaxArrows;

		#endregion

		#endregion
		#region Private Members

		#region (func) OnDrawGizmosSelectedFunc

		/// <summary>
		/// Function used to draw gravity in the editor.
		///</summary>

		private System.Action<float> _OnDrawGizmosSelectedFunc;

		/// <inheritdoc cref="_OnDrawGizmosSelectedFunc"/>

		protected System.Action<float> OnDrawGizmosSelectedFunc => _OnDrawGizmosSelectedFunc;

		#endregion

		#endregion

		#region Methods

		#region OnDrawGizmosSelected

		protected override void OnDrawGizmosSelected()
		{
			OnDrawGizmosSelectedFunc(DebugDensity);
		}


		#endregion
		#region GetOnDrawGizmosSelectedFunc

		protected virtual System.Action<float> GetOnDrawGizmosSelectedFunc(TCollider collider) => throw new System.NotImplementedException();

		#endregion

		#endregion
#endif
		#endregion
	}

	#endregion

	#region Gravity Region (3D)

	/// <summary>
	/// __TODO_ANNOTATE__
	///</summary>

	public class GravityRegion : GravityRegionBase<Collider, Vector3>
	{
		#region Fields
		#endregion
		#region Members
		#endregion
		#region Properties
		#endregion
		#region Methods

		#region (sealed override) GetForce

		public sealed override Vector3 GetForce(Vector3 atPosition)
		{
			var __forceSplit = GetForceFromShape(atPosition);
			return __forceSplit.Item1 * GetFalloffPercent(__forceSplit.Item2) * Strength;
		}

		#endregion
		#region (override) GetShapeForceFunc

		public override System.Func<Vector3, (Vector3, float)> GetShapeForceFunc(Collider collider)
		{
			if (collider.GetType() == typeof(BoxCollider))
				return GetForce_Box;
			if (collider.GetType() == typeof(SphereCollider))
				return GetForce_Sphere;
			if (collider.GetType() == typeof(CapsuleCollider))
				return GetForce_Capsule;

			return GetForce_Linear;
		}

		#endregion
		#region (override) GetShapeOuterRadiusFunc

		public override System.Func<float> GetShapeOuterRadiusFunc(Collider collider)
		{
			if (collider.GetType() == typeof(BoxCollider))
				return GetRadius_Box;
			if (collider.GetType() == typeof(SphereCollider))
				return GetRadius_Sphere;
			if (collider.GetType() == typeof(CapsuleCollider))
				return GetRadius_Capsule;

			return GetRadius_Linear;
		}

		#endregion
		#region (override) SetShapeOuterRadiusFunc

		public override System.Action<float> SetShapeOuterRadiusFunc(Collider collider)
		{
			if (collider.GetType() == typeof(BoxCollider))
				return SetRadius_Box;
			if (collider.GetType() == typeof(SphereCollider))
				return SetRadius_Sphere;
			if (collider.GetType() == typeof(CapsuleCollider))
				return SetRadius_Capsule;

			return SetRadius_Linear;
		}

		#endregion

		#region Shape Methods

		#region OuterRadius

		protected virtual float GetRadius_Box() => GetRadius_Linear();
		protected virtual void SetRadius_Box(float radius) => SetRadius_Linear(radius);

		protected virtual float GetRadius_Sphere() => ((SphereCollider)Collider).radius;
		protected virtual void SetRadius_Sphere(float radius) => ((SphereCollider)Collider).radius = radius;

		protected virtual float GetRadius_Capsule() => ((CapsuleCollider)Collider).radius;
		protected virtual void SetRadius_Capsule(float radius) => ((CapsuleCollider)Collider).radius = radius;

		protected virtual float GetRadius_Linear() => Collider.bounds.size.y;
		protected virtual void SetRadius_Linear(float radius)
		{
			Bounds t = Collider.bounds;
			t.size = new Vector3(Collider.bounds.size.x, radius, Collider.bounds.size.z);
		}

		#endregion

		#region GetForce

		/// <inheritdoc cref="GetForce"/>
		/// <remarks>
		/// __TODO_ANNOTATE__
		///</remarks>

		protected virtual (Vector3, float) GetForce_Box(Vector3 atPosition)
		{
			return GetForce_Linear(atPosition);
		}

		/// <inheritdoc cref="GetForce"/>
		/// <remarks>
		/// __TODO_ANNOTATE__
		///</remarks>

		protected virtual (Vector3, float) GetForce_Sphere(Vector3 atPosition)
		{
			var coagulate = atPosition - (transform.position + ((SphereCollider)Collider).center);

			return (-coagulate.normalized, coagulate.magnitude);
		}

		/// <inheritdoc cref="GetForce"/>
		/// <remarks>
		/// __TODO_ANNOTATE__
		///</remarks>

		protected virtual (Vector3, float) GetForce_Capsule(Vector3 atPosition)
		{
			var capsule = (CapsuleCollider)Collider;

			float height = Vector3.ProjectOnPlane(atPosition, capsule.GetDirectionAxis()).y;
			float capsuleHalfHeight = capsule.GetHalfHeightUncapped();

			Vector3 coagulate;

			if (height < -capsuleHalfHeight)
				coagulate = (atPosition - capsule.GetTailPositionUncapped());
			else if (height > capsuleHalfHeight)
				coagulate = (atPosition - capsule.GetHeadPositionUncapped());
			else
				coagulate = Vector3.ProjectOnPlane(atPosition, capsule.GetDirectionAxis()) - (transform.position + capsule.center);

			return (coagulate.normalized, coagulate.magnitude);
		}

		/// <inheritdoc cref="GetForce"/>
		/// <remarks>
		/// __TODO_ANNOTATE__
		///</remarks>

		protected virtual (Vector3, float) GetForce_Linear(Vector3 atPosition)
		{
			return (
				-transform.up,
				Vector3.ProjectOnPlane(atPosition, transform.up).magnitude
			);
		}

		#endregion
		#endregion

		#endregion

		#region Debug

		#region Methods

		protected override System.Action<float> GetOnDrawGizmosSelectedFunc(Collider collider)
		{
			if (Collider.GetType() == typeof(BoxCollider))
				return OnDrawGizmosSelected_Box;
			if (Collider.GetType() == typeof(SphereCollider))
				return OnDrawGizmosSelected_Sphere;
			if (Collider.GetType() == typeof(CapsuleCollider))
				return OnDrawGizmosSelected_Capsule;
			return OnDrawGizmosSelected_Linear;
		}

		#region Shape Methods

		protected virtual void OnDrawGizmosSelected_Box(float density)
		{
			DebugDraw.DrawArrow(transform.position + ((BoxCollider)Collider).center, GetForce(transform.position).normalized, Color.magenta, 0.5f);
		}

		protected virtual void OnDrawGizmosSelected_Sphere(float density)
		{
			DebugDraw.DrawArrow(transform.position + ((SphereCollider)Collider).center, GetForce(transform.position).normalized, Color.magenta, 0.5f);
		}

		protected virtual void OnDrawGizmosSelected_Capsule(float density)
		{
			DebugDraw.DrawArrow(transform.position + ((CapsuleCollider)Collider).center, GetForce(transform.position).normalized, Color.magenta, 0.5f);
		}

		protected virtual void OnDrawGizmosSelected_Linear(float density)
		{
			DebugDraw.DrawArrow(transform.position, GetForce(transform.position).normalized, Color.magenta, 0.5f);
		}

		#endregion

		#endregion

		#endregion
	}
	#endregion
}
