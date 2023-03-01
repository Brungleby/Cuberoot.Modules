
/** SensorModule.cs
*
*	Created by LIAM WOFFORD, USA-TX, for the Public Domain.
*
*	Repo: https://github.com/Brungleby/Cuberoot
*	Kofi: https://ko-fi.com/brungleby
*/

#region Includes

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#endregion

namespace Cuberoot.Modules
{
	#region (class) CasterModule

	[DefaultExecutionOrder(-10)]

	public abstract class CasterModule<THitResult> : Module, ISensorCasterModule<THitResult>
	where THitResult : HitResultBase, new()
	{
		#region Fields

		#region Layers

		/// <inheritdoc cref="Layers"/>
		[Tooltip("Layers that will block this module's cast.")]
		[SerializeField]

		private LayerMask _Layers;

		public LayerMask Layers
		{
			get => _Layers;
			set => _Layers = value;
		}

		#endregion
		#region MaxDistance

		/// <inheritdoc cref="MaxDistance"/>
		[Tooltip("")]
		[Min(0f)]
		[SerializeField]

		private float _MaxDistance = 1f;
		public float MaxDistance
		{
			get => _MaxDistance;
			set => _MaxDistance = value;
		}

		#endregion

		#endregion
		#region Members

		#region CastResult

		/// <inheritdoc cref="CastResult"/>

		private THitResult _CastResult;
		public THitResult CastResult
		{
			get => _CastResult;
			set => _CastResult = value;
		}

		#endregion

		#endregion

		#region Methods

		#region OnValidate

		protected override void OnValidate()
		{
			base.OnValidate();
		}

		#endregion

		#endregion
	}

	#endregion
	#region (class) ShapeCasterModule

	public abstract class ShapeCasterModule<THitResult, TColliderShape> : CasterModule<THitResult>
	where THitResult : HitResultBase, new()
	{
		#region Fields

		#region (field) ColliderShape

		[SerializeField]

		private TColliderShape _ColliderShape;

		/// <inheritdoc cref="_ColliderShape"/>

		public TColliderShape ColliderShape => _ColliderShape;

		#endregion

		#endregion
		#region Methods

		protected override void OnValidate()
		{
			base.OnValidate();

			_ColliderShape = GetComponent<TColliderShape>();
			UpdateCast_ShapeMethod = DetermineShapeMethod(_ColliderShape.GetType());
		}

		private THitResult UpdateCast(float deltaTime) =>
			UpdateCast_ShapeMethod(deltaTime);

		private System.Func<float, THitResult> UpdateCast_ShapeMethod;

		protected virtual System.Func<float, THitResult> DetermineShapeMethod(System.Type colliderType)
		{
			if (colliderType == typeof(CapsuleCollider) || colliderType == typeof(CapsuleCollider2D))
				return UpdateCast_WithCapsule;
			else if (colliderType == typeof(SphereCollider) || colliderType == typeof(CircleCollider2D))
				return UpdateCast_WithSphere;
			else if (colliderType == typeof(BoxCollider) || colliderType == typeof(BoxCollider2D))
				return UpdateCast_WithBox;

			throw new System.ArgumentException("This type of collider is not supported. Please implement a new method.");
		}

		protected virtual THitResult UpdateCast_WithCapsule(float deltaTime) =>
			throw new System.NotImplementedException();
		protected virtual THitResult UpdateCast_WithSphere(float deltaTime) =>
			throw new System.NotImplementedException();
		protected virtual THitResult UpdateCast_WithBox(float deltaTime) =>
			throw new System.NotImplementedException();


		#endregion
	}

	#endregion
}
