﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using GOAP_S.Blackboard;
using GOAP_S.PT;

namespace GOAP_S.UI
{
    public class VariableSelectMenu_GS : PopupWindowContent
    {
        //Content fields
        static private NodeEditor_GS _target_node_editor = null; //Node editor where this window is shown
        static private string _variable_name = null;
        VariableType _variable_type = VariableType._undefined;
        Variable_GS _variable = new Variable_GS("",null);
        //State fields
        static private bool on_type_change = false;

        //Contructors =================
        public VariableSelectMenu_GS(NodeEditor_GS target_node_editor)
        {
            //Set the node editor
            _target_node_editor = target_node_editor;
        }

        //Loop Methods ================
        public override void OnGUI(Rect rect)
        {
            //Menu title
            GUILayout.BeginHorizontal("Box");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Variable Select", _target_node_editor.UI_configuration.select_menu_title_style, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            //Separator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            //Variable name field
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.ExpandWidth(true));
            _variable_name = EditorGUILayout.TextField(_variable_name, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            //Variable type field
            GUILayout.BeginHorizontal();
            GUILayout.Label("Type", GUILayout.ExpandWidth(true));
            VariableType current_type = _variable_type;
            _variable_type = (VariableType)EditorGUILayout.EnumPopup(_variable_type,GUILayout.Width(150), GUILayout.ExpandWidth(true));
            //Check if var type is changed
            on_type_change = current_type != _variable_type;
            GUILayout.EndHorizontal();
            
            //Custom field value
            GUILayout.BeginVertical();
            switch(_variable_type)
            {
                case VariableType._short:
                    {
                        if(on_type_change)
                        {
                            int k = 0;
                            _variable.value = k;
                        }

                        _variable.value = EditorGUILayout.IntField("Value", (int)_variable.value);

                        GUILayout.BeginHorizontal();
                        GUILayout.EndHorizontal();
                    }
                    break;
                case VariableType._int:
                    {

                    }
                    break;
                case VariableType._long:
                    {

                    }
                    break;
                case VariableType._float:
                    {

                    }
                    break;
            }
            GUILayout.EndVertical();

            //Separation
            GUILayout.FlexibleSpace(); 

            GUILayout.BeginHorizontal();
            //Add button
            if (GUILayout.Button("Add", _target_node_editor.UI_configuration.node_modify_button_style, GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true)))
            {
                //Add the new variable if the data is correct
                //_target_node_editor.selected_agent.blackboard.AddVariable()
            }
            //Close button
            if (GUILayout.Button("Close", _target_node_editor.UI_configuration.node_modify_button_style, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                editorWindow.Close();
            }
            GUILayout.EndHorizontal();
        }

        public override void OnClose()
        {
            _variable_name = "";
        }

        //Functionality Methods =======
        public Variable_GS GenerateVariable()
        {
            //TODO set var name,type,etc
            return _variable;
        }

        public int GetAllClassInstances(System.Type type, out ArrayList elements)
        {
            //Allocate a new array
            elements = new ArrayList();
            //Get the root gameobjects of the current scene
            GameObject[] root_gameojects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            //Iterate root gameobjects
            foreach(GameObject g_obj in root_gameojects)
            {
                //Get all the child components with the specified type
                Component[] childs_components = g_obj.GetComponentsInChildren(type);
                foreach(Component comp in childs_components)
                {
                    //Add the found components to the array
                    elements.Add(comp);
                }
            }

            return elements.Count;
        }
    }
}