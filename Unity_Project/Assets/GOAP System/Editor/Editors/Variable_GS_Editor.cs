﻿using UnityEngine;
using UnityEditor;
using GOAP_S.Blackboard;

public class Variable_GS_Editor
{
    //Conten fields
    private Variable_GS _target_variable;
    private Blackboard_GS _target_bb;

    //Constructors ====================
    public Variable_GS_Editor(Variable_GS target_variable, Blackboard_GS target_bb)
    {
        //Set target variable
        _target_variable = target_variable;
        //Set target bb
        _target_bb = target_bb;
    }

    //Loop Methods ====================
    public void DrawUI()
    {
        GUILayout.BeginHorizontal();
        
        //Remove button
        if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
        {
            //Remove the current var
            _target_bb.RemoveVariable(_target_variable.id);
        }

        //Show/Edit variable value
        string type_string = _target_variable.type.ToString();
        GUILayout.Label(type_string);
        GUILayout.Label(_target_variable.system_type.IsClass.ToString());

        /*switch (type_string)
        {
            case "System.Single":
                _target_variable.value = EditorGUILayout.FloatField(ProTools.BasicTypeFromSystemType(_target_variable.type.ToString()) + " " + _target_variable.name,(float)_target_variable.value,GUILayout.ExpandWidth(true));
                break;

            case "System.Int32":
                _target_variable.value = EditorGUILayout.IntField(ProTools.BasicTypeFromSystemType(_target_variable.type.ToString()) + " " + _target_variable.name, (int)_target_variable.value, GUILayout.Width(50), GUILayout.ExpandWidth(true));
                break;

            default:
                //EditorGUILayout.ObjectField(_target_variable.value, _target_variable.type);
                break;
        }*/

        GUILayout.EndHorizontal();
    }

    //Get/Set Methods =================
    public Variable_GS target_variable
    {
        get
        {
            return _target_variable;
        }
        set
        {
            _target_variable = value;
        }
    }

    public Blackboard_GS target_bb
    {
        get
        {
            return _target_bb;
        }
        set
        {
            _target_bb = value;
        }
    }
}