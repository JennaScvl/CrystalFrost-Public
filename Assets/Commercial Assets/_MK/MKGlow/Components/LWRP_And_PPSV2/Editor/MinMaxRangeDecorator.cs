//////////////////////////////////////////////////////
// MK Glow Range Drawer    	    	       			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace MK.Glow.LWRP
{
	[UnityEditor.Rendering.PostProcessing.Decorator(typeof(MinMaxRangeAttribute))]
	internal class MinMaxRangeDecorator : UnityEditor.Rendering.PostProcessing.AttributeDecorator
	{
		public override bool OnGUI(SerializedProperty property, SerializedProperty overrideState, GUIContent title, System.Attribute attribute)
        {
			MinMaxRangeAttribute range = attribute as MinMaxRangeAttribute;
			SerializedProperty minRange = property.FindPropertyRelative("minValue");
			SerializedProperty maxRange = property.FindPropertyRelative("maxValue");
			float minValue = minRange.floatValue;
			float maxValue = maxRange.floatValue;

			Rect startRect = EditorGUILayout.GetControlRect();

			Rect minRect = new Rect(EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth * 0.7f, startRect.y, EditorGUIUtility.fieldWidth, startRect.height);
			float p = minRect.x + EditorGUIUtility.standardVerticalSpacing * 2f + EditorGUIUtility.fieldWidth;
			Rect sliderRect = new Rect(p, startRect.y, EditorGUIUtility.currentViewWidth - p - EditorGUIUtility.fieldWidth * 1.5f, startRect.height);
			Rect maxRect = new Rect(sliderRect.x + sliderRect.width + EditorGUIUtility.standardVerticalSpacing * 2f, startRect.y, EditorGUIUtility.fieldWidth, startRect.height);

			EditorGUI.LabelField(startRect, title.text);
			minValue = EditorGUI.FloatField(minRect, minValue);

			EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, range.minLimit, range.maxLimit);
			maxValue = EditorGUI.FloatField(maxRect, maxValue);

			minRange.floatValue = minValue;
			maxRange.floatValue = maxValue;

			return true;
		}
	}
}
#endif