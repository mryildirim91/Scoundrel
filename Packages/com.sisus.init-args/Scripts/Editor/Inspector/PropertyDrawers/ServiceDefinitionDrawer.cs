using System;
using System.Collections.Generic;
using Sisus.Init.Internal;
using Sisus.Init.Serialization;
using Sisus.Init.ValueProviders;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Custom property drawer for <see cref="ServiceDefinition"/>.
	/// </summary>
	[CustomPropertyDrawer(typeof(ServiceDefinition), true)]
	internal class ServiceDefinitionDrawer : PropertyDrawer
	{
		private sealed class State
		{
			public SerializedProperty serviceProperty;
			public SerializedProperty definingTypeProperty;
			public TypeDropdownButton definingTypeButton;
		}

		private const float AsLabelWidth = 25f;

		private static readonly string definingTypeTooltip = "The defining type for the service, which can be used to retrieve the service.\n\nThis must be an interface that {0} implements, a base type that the it derives from, or its exact type.";
		private static readonly GUIContent asLabel = new(" as ");
		private readonly Dictionary<string, State> states = new();

		public override void OnGUI(Rect position, SerializedProperty serviceDefinitionProperty, GUIContent label)
		{
			if(!states.TryGetValue(serviceDefinitionProperty.propertyPath, out State state))
			{
				state = new();
				Setup(serviceDefinitionProperty, state);
				states.Add(serviceDefinitionProperty.propertyPath, state);
			}
			else if(state.serviceProperty == null || state.serviceProperty.serializedObject != serviceDefinitionProperty.serializedObject)
			{
				Setup(serviceDefinitionProperty, state);
			}

			var serviceRect = position;
			var service = state.serviceProperty.objectReferenceValue;
			var hasValue = (bool)service;
			float controlWidth = hasValue ? (position.width - AsLabelWidth) * 0.5f : position.width;
			serviceRect.width = controlWidth;

			EditorGUI.BeginChangeCheck();

			if(!hasValue && InitializerEditorUtility.TryGetTintForNullGuardResult(NullGuardResultType.Error, out var errorColor))
			{
				GUI.color = errorColor;
			}

			EditorGUI.PropertyField(serviceRect, state.serviceProperty, GUIContent.none);

			GUI.color = Color.white;

			if(EditorGUI.EndChangeCheck())
			{
				serviceDefinitionProperty.serializedObject.ApplyModifiedProperties();

				var newService = state.serviceProperty.objectReferenceValue;
				switch(newService)
				{
					case GameObject gameObject:
					{
						using var components = gameObject.GetComponentsNonAlloc<Component>();
						var menu = new GenericMenu();
						menu.AddItem(new("GameObject"), false, () =>
						{
							Undo.RecordObjects(state.serviceProperty.serializedObject.targetObjects, "Set Service");
							state.serviceProperty.objectReferenceValue = gameObject;
							state.definingTypeProperty.SetValue(new _Type(typeof(GameObject)));
							state.serviceProperty.serializedObject.ApplyModifiedProperties();
							states.Remove(serviceDefinitionProperty.propertyPath);
							serviceDefinitionProperty.serializedObject.Update();
							RebuildDefiningTypeButton(serviceDefinitionProperty, state);
						});

						var addedItems = new HashSet<string> { "GameObject" };

						for(int i = 0, count = components.Count; i < count; i++)
						{
							var component = components[i];
							if(!component)
							{
								continue;
							}

							if(component is Services or ServiceTag || component.hideFlags.HasFlag(HideFlags.HideInInspector))
							{
								continue;
							}

							var name = ObjectNames.NicifyVariableName(component.GetType().Name);
							if(!addedItems.Add(name))
							{
								int nth = 2;
								string uniqueName;
								do
								{
									uniqueName = name + " (" + nth + ")";
									nth++;
								}
								while(!addedItems.Add(uniqueName));
								name = uniqueName;
							}

							menu.AddItem(new(name), false, () =>
							{
								Undo.RecordObjects(state.serviceProperty.serializedObject.targetObjects, "Set Service");
								state.serviceProperty.objectReferenceValue = component;
								state.serviceProperty.serializedObject.ApplyModifiedProperties();
								state.definingTypeProperty.SetValue(new _Type(component.GetType()));
								states.Remove(serviceDefinitionProperty.propertyPath);
								serviceDefinitionProperty.serializedObject.Update();
								RebuildDefiningTypeButton(serviceDefinitionProperty, state);
							});
						}

						menu.DropDown(serviceRect);
						break;
					}
					// If a user tries to register a class by dragging its script to the services component,
					// inform the user that this is not supported.
					case MonoScript script:
					{
						if(script.GetClass() is { } scriptClassType)
						{
							if(scriptClassType == typeof(MonoBehaviour))
							{
								Debug.LogWarning($"You can not register a script asset as a service. If you want to register an instance of type {scriptClassType.Name} as a service, you can attach the component to a GameObject, and then drag-and-drop the GameObject here instead.", script);
							}
							else if(scriptClassType == typeof(ScriptableObject))
							{
								Debug.LogWarning($"You can not register a script asset as a service. If you want to register an instance of type {scriptClassType.Name} as a service, you can create an asset from it using the [CreateAssetMenu] attribute, and drag-and-drop the asset here instead.", script);
							}
							else
							{
								Debug.LogWarning($"You can not register a script asset as a service. If you want to register an instance of type {scriptClassType.Name} as a service, you can create a component class that derives from Wrapper<{scriptClassType.Name}>, or a ScriptableObject class that implements IValueProvider<{scriptClassType.Name}>, and drag-and-drop an instance of that class here instead.", script);
							}
						}
						else
						{
							Debug.LogWarning($"You can not register a script asset as a service. If you want to register an instance of the type defined inside the script as a service, you can create a component class that derives from Wrapper<TService>, or a ScriptableObject class that implements IValueProvider<TService>, and drag-and-drop an instance of that class here instead.", script);
						}

						state.serviceProperty.objectReferenceValue = null;
						serviceDefinitionProperty.serializedObject.ApplyModifiedProperties();
						break;
					}
					default:
					{
						if(!newService)
						{
							SetDefiningType(state, null, serviceDefinitionProperty);
						}
						else if(state.definingTypeProperty.GetValue() is _Type { Value: { } t } && !t.IsInstanceOfType(newService) && !ValueProviderUtility.IsValueTypeSupported(newService, t))
						{
							SetDefiningType(state, newService.GetType(), serviceDefinitionProperty);
						}
						else
						{
							states.Remove(serviceDefinitionProperty.propertyPath);
						}

						break;
					}
				}
			}

			if(!hasValue)
			{
				if(state.definingTypeProperty.GetValue() is _Type { TypeNameAndAssembly: { Length: > 0 } })
				{
					SetDefiningType(state, null, serviceDefinitionProperty);
				}

				return;
			}

			var asLabelRect = serviceRect;
			asLabelRect.x += serviceRect.width;
			asLabelRect.width = AsLabelWidth;
			GUI.Label(asLabelRect, asLabel);

			var dropdownRect = asLabelRect;
			dropdownRect.x += AsLabelWidth;
			dropdownRect.width = controlWidth;

			bool showMixedValueWas = EditorGUI.showMixedValue;
			if(state.definingTypeProperty.hasMultipleDifferentValues)
			{
				EditorGUI.showMixedValue = true;
			}

			var drawnItemNullGuardResultWas = AnyPropertyDrawer.CurrentlyDrawnItemNullGuardResult;
			var definingTypeInfo = state.definingTypeProperty.GetValue() as _Type;
			var definingType = definingTypeInfo?.Value;
			if(definingType is null)
			{
				if(service & string.IsNullOrEmpty(definingTypeInfo?.TypeNameAndAssembly))
				{
					SetDefiningType(state, service.GetType(), serviceDefinitionProperty);
				}
			}
			else if(!definingType.IsInstanceOfType(service) && !ValueProviderUtility.IsValueTypeSupported(service, definingType))
			{
				var concreteType = service.GetType();
				var concreteTypeToString = TypeUtility.ToString(concreteType);
				var definingTypeToString = TypeUtility.ToString(definingType);
				AnyPropertyDrawer.CurrentlyDrawnItemNullGuardResult = NullGuardResult.Error(definingType.IsInterface
					? $"The service {concreteTypeToString} does not implement the defining type {definingTypeToString} or IValueProvider<{definingTypeToString}>."
					: $"The service {concreteTypeToString} does not derive from the defining type {definingTypeToString} or implement IValueProvider<{definingTypeToString}>");
			}

			state.definingTypeButton.Draw(dropdownRect);

			AnyPropertyDrawer.CurrentlyDrawnItemNullGuardResult = drawnItemNullGuardResultWas;
			EditorGUI.showMixedValue = showMixedValueWas;
		}

		private void SetDefiningType(State state, Type definingType, SerializedProperty serviceDefinitionProperty)
		{
			Undo.RecordObjects(serviceDefinitionProperty.serializedObject.targetObjects, "Set Defining Type");
			state.definingTypeProperty.SetValue(new _Type(definingType));
			serviceDefinitionProperty.serializedObject.ApplyModifiedProperties();
			RebuildDefiningTypeButton(serviceDefinitionProperty, state);
		}

		private void Setup(SerializedProperty property, State state)
		{
			state.serviceProperty = property.FindPropertyRelative("service");
			state.definingTypeProperty = property.FindPropertyRelative("definingType");

			RebuildDefiningTypeButton(property, state);
		}

		private void RebuildDefiningTypeButton(SerializedProperty property, State state)
		{
			var serviceValue = state.serviceProperty.objectReferenceValue;
			var definingType = (state.definingTypeProperty.GetValue() as _Type)?.Value;
			var definingTypeLabel = new GUIContent(TypeUtility.ToString(definingType), string.Format(definingTypeTooltip, serviceValue is null ? "the service class" : TypeUtility.ToString(serviceValue.GetType())));

			state.definingTypeButton = new
			(
				GUIContent.none,
				definingTypeLabel,
				new DefiningTypeDataSource("Defining Type", serviceValue, new() { definingType }),
				setType =>
				{
					SetDefiningType(state, setType, property);
					GUI.changed = true;
				}
			);
		}
	}
}