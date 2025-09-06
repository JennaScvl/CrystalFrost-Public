using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An example of how to set the properties of a joystick.
/// </summary>
public class JoystickSetterExample : MonoBehaviour
{
    /// <summary>
    /// The joystick to modify.
    /// </summary>
    public VariableJoystick variableJoystick;
    /// <summary>
    /// The text that displays the current value of the joystick.
    /// </summary>
    public Text valueText;
    /// <summary>
    /// The background image of the joystick.
    /// </summary>
    public Image background;
    /// <summary>
    /// The sprites for the different axis options.
    /// </summary>
    public Sprite[] axisSprites;

    /// <summary>
    /// Changes the mode of the joystick.
    /// </summary>
    /// <param name="index">The index of the mode to change to.</param>
    public void ModeChanged(int index)
    {
        switch(index)
        {
            case 0:
                variableJoystick.SetMode(JoystickType.Fixed);
                break;
            case 1:
                variableJoystick.SetMode(JoystickType.Floating);
                break;
            case 2:
                variableJoystick.SetMode(JoystickType.Dynamic);
                break;
            default:
                break;
        }     
    }

    /// <summary>
    /// Changes the axis options of the joystick.
    /// </summary>
    /// <param name="index">The index of the axis option to change to.</param>
    public void AxisChanged(int index)
    {
        switch (index)
        {
            case 0:
                variableJoystick.AxisOptions = AxisOptions.Both;
                background.sprite = axisSprites[index];
                break;
            case 1:
                variableJoystick.AxisOptions = AxisOptions.Horizontal;
                background.sprite = axisSprites[index];
                break;
            case 2:
                variableJoystick.AxisOptions = AxisOptions.Vertical;
                background.sprite = axisSprites[index];
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Sets whether the joystick should snap to the X axis.
    /// </summary>
    /// <param name="value">True to snap, false otherwise.</param>
    public void SnapX(bool value)
    {
        variableJoystick.SnapX = value;
    }

    /// <summary>
    /// Sets whether the joystick should snap to the Y axis.
    /// </summary>
    /// <param name="value">True to snap, false otherwise.</param>
    public void SnapY(bool value)
    {
        variableJoystick.SnapY = value;
    }

    private void Update()
    {
        valueText.text = "Current Value: " + variableJoystick.Direction;
    }
}