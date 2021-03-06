﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.IO;

namespace GOAP_S.Tools
{
    public enum EditorUIMode
    {
        HIDE_STATE = 0,
        SET_STATE, //State in which the user can set action node attributes
        EDIT_STATE //State in which the user can set node description/name
    }

    public enum PropertyUIMode
    {
        IS_UNDEFINED = 0,
        IS_CONDITION,
        IS_EFFECT,
        IS_VARIABLE
    }

    public enum VariableType
    {
        _undefined_var_type = 0,
        _bool,
        _int,
        _float,
        _char,
        _string,
        _vector2,
        _vector3,
        _vector4
    }

    public enum OperatorType
    {
        _undefined_operator = 0,
        _bigger,
        _bigger_or_equal,
        _smaller,
        _smaller_or_equal,
        _equal_equal,
        _different,
        _is_equal,
        _plus_equal,
        _minus_equal
    }

    public enum VariableLocation
    {
        _undefined_location = 0,
        _local,
        _global
    }

    public static class ProTools
    {
        //Defines ===============================
        public const int MIN_CANVAS_WIDTH = 500;
        public const int MIN_CANVAS_HEIGHT = 400;
        public const int NODE_EDITOR_CANVAS_SIZE = 4000;
        public const int BEHAVIOUR_EDITOR_CANVAS_SIZE = 2500;
        public const int BLACKBOARD_MARGIN = 400;
        public const int INITIAL_ARRAY_SIZE = 10;
        public const int TRIES_LIMIT = 4;
        public const int ITERATION_LIMIT = 300;
        public const float MIN_PROPERTY_DISTANCE = 0.00001f;

        //Assemblies ============================
        private static List<Assembly> _assemblies = null;
        public static List<Assembly> assemblies
        {
            get
            {
                //Check if assamblies were stored before
                if (_assemblies == null)
                {
                    _assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                }
                return _assemblies;
            }
        }

        //T Methods =============================
        public static T CreateDelegate<T>(this MethodInfo method_info, object instance)
        {
            return (T)(object)Delegate.CreateDelegate(typeof(T), instance, method_info);
        }

        public static T AllocateClass<T>(this object myobj)
        {
            #if UNITY_EDITOR
            //Get the class to allocate type
            System.Type class_ty = ((MonoScript)myobj).GetClass();
            //Instantiate a class of type class_ty
            object x = System.Activator.CreateInstance(class_ty, false);
            //Return the allocated class casted to type T
            return (T)x;
            #else
            return default(T);
            #endif
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            #if UNITY_EDITOR
            List<T> assets = new List<T>();
            //Get all the assets GUID
            string[] guids = AssetDatabase.FindAssets(null, new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                //Transform GUIDs to paths
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                //Get type T assets using the paths
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (asset != null)
                {
                    //Add the asset to the list of type T
                    assets.Add(asset);
                }
            }
            return assets;
            #else
            return null;
            #endif
        }

        public static T FindAssetByPath<T>(string path) where T : UnityEngine.Object
        {
            #if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets(null, path.Split('\\'));

            if (guids.Length == 0 || guids.Length > 1)
            {
                return null;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            //Get type T assets using the paths
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            return asset;
            #else
            return null;
            #endif
        }

        //Properties Methods ====================
        public static PropertyInfo[] FindAllPropertiesInGameObject(GameObject target)
        {
            //Total strings to allocate in the array
            int total_paths_count = 0;

            //Count game object properties
            int game_object_properties_count = target.GetType().GetProperties().Length;
            total_paths_count += game_object_properties_count;

            //Count components properties
            Component[] agent_components = target.GetComponents(typeof(Component));
            foreach (Component comp in agent_components)
            {
                total_paths_count += comp.GetType().GetProperties().Length;
            }

            //Allocate array
            PropertyInfo[] properties_info = new PropertyInfo[total_paths_count];
            int index = 0;

            //Add game object properties
            foreach (PropertyInfo property_info in typeof(GameObject).GetProperties())
            {
                properties_info[index] = property_info;
                index += 1;
            }

            //Add components properties
            foreach (Component comp in agent_components)
            {
                PropertyInfo[] comp_properties = comp.GetType().GetProperties();
                foreach (PropertyInfo comp_property_info in comp_properties)
                {
                    properties_info[index] = comp_property_info;
                    index += 1;
                }
            }

            return properties_info;

        }

        public static PropertyInfo[] FindConcretePropertiesInGameObject(GameObject target, Type target_property_type)
        {
            //Allocate the properties list
            List<PropertyInfo> properties_list = new List<PropertyInfo>();

            //Collect game object properties that match with the target type
            foreach (PropertyInfo property_info in typeof(GameObject).GetProperties())
            {
                if (property_info.PropertyType == target_property_type)
                {
                    properties_list.Add(property_info);
                }
            }

            //Collect components properties that match with the target type
            Component[] object_components = target.GetComponents(typeof(Component));
            foreach (Component comp in object_components)
            {
                PropertyInfo[] comp_properties = comp.GetType().GetProperties();
                foreach (PropertyInfo comp_property_info in comp_properties)
                {
                    if (comp_property_info.PropertyType == target_property_type)
                    {
                        properties_list.Add(comp_property_info);
                    }
                }
            }

            //Return list converted to array
            return properties_list.ToArray();
        }

        public static FieldInfo [] FindConcreteFieldsInGameObject(GameObject target, Type target_field_type)
        {
            //Allocate the fields list
            List<FieldInfo> fields_list = new List<FieldInfo>();

            //Collect game object fields that match with the target type
            foreach(FieldInfo field_info in typeof(GameObject).GetFields())
            {
                if(field_info.FieldType == target_field_type)
                {
                    fields_list.Add(field_info);
                }
            }

            //Collect components fields that match with the target type
            Component[] agent_components = target.GetComponents(typeof(Component));
            foreach(Component comp in agent_components)
            {
                FieldInfo[] comp_fields = comp.GetType().GetFields();
                foreach(FieldInfo comp_field_info in comp_fields)
                {
                    if(comp_field_info.FieldType == target_field_type)
                    {
                        fields_list.Add(comp_field_info);
                    }
                }
            }

            //Return list converted to array
            return fields_list.ToArray();
        }

        //Methods Methods =======================
        public static KeyValuePair<MethodInfo, object>[] FindConcreteGameObjectMethods(GameObject target_object, Type target_return_type)
        {
            //Allocate the methods list
            List<KeyValuePair<MethodInfo,object>> methods_list = new List<KeyValuePair<MethodInfo, object>>();

            //Collect game object methods
            foreach(MethodInfo method_info in typeof(GameObject).GetMethods())
            {
                //Methods with the same return type as the target are stored in the methods list
                if(method_info.ReturnType == target_return_type)
                {
                    methods_list.Add(new KeyValuePair<MethodInfo, object>(method_info, target_object));
                }
            }

            //Collect components methods
            Component[] object_components = target_object.GetComponents(typeof(Component));
            foreach (Component comp in object_components)
            {
                MethodInfo[] comp_methods = comp.GetType().GetMethods();
                foreach(MethodInfo comp_method in comp_methods)
                {
                    //Methods with the same return type as the target are stored in the methods list
                    if (comp_method.ReturnType == target_return_type)
                    {
                        methods_list.Add(new KeyValuePair<MethodInfo, object>(comp_method, comp));
                    }
                }
            }

            //Return the list converted to array
            return methods_list.ToArray();
        }

        public static KeyValuePair<MethodInfo, object>[] FindConcreteAgentMethods(AI.Agent_GS target_agent, Type target_return_type)
        {
            //Allocate the methods list
            List<KeyValuePair<MethodInfo, object>> methods_list = new List<KeyValuePair<MethodInfo, object>>();

            //Get action nodes methods
            for (int k = 0; k < target_agent.action_nodes_num; k++)
            {
                //Check action node action
                Planning.Action_GS node_action = target_agent.action_nodes[k].action;
                if (node_action != null)
                {
                    //Get action methods
                    MethodInfo[] action_methods = node_action.GetType().GetMethods();
                    foreach(MethodInfo action_method in action_methods)
                    {
                        //Methods with the same return type as the target are stored in the methods list
                        if (action_method.ReturnType == target_return_type)
                        {
                            methods_list.Add(new KeyValuePair<MethodInfo, object>(action_method, node_action));
                        }
                    }
                }
            }

            //Get behaviour methods
            if(target_agent.behaviour != null)
            {
                MethodInfo[] behaviour_methods = target_agent.behaviour.GetType().GetMethods();
                foreach(MethodInfo behaviour_method in behaviour_methods)
                {
                    //Methods with the same return type as the target are stored in the methods list
                    if (behaviour_method.ReturnType == target_return_type)
                    {
                        methods_list.Add(new KeyValuePair<MethodInfo, object>(behaviour_method, target_agent.behaviour));
                    }
                }
            }
            
            //Get idle action methods
            if(target_agent.idle_action != null)
            {
                MethodInfo[] idle_action_methods = target_agent.idle_action.GetType().GetMethods();
                foreach(MethodInfo idle_action_method in idle_action_methods)
                {
                    //Methods with the same return type as the target are stored in the methods list
                    if(idle_action_method.ReturnType == target_return_type)
                    {
                        methods_list.Add(new KeyValuePair<MethodInfo, object>(idle_action_method, target_agent.idle_action));
                    }
                }
            }

            //Return the list converted to array
            return methods_list.ToArray();
        }

        public static KeyValuePair<MethodInfo,object> FindMethodFromPath(string method_path, Type target_return_type, GameObject target_object)
        {
            //Return method info
            KeyValuePair<MethodInfo, object>[] methods_to_compare = null;
            string[] method_path_parts = method_path.Split('.');
            

            //First check if the method is inside the gameobject or the agent logic
            //Gameobject case
            if (string.Compare("GameObject", method_path_parts[0]) == 0)
            {
                methods_to_compare = FindConcreteGameObjectMethods(target_object, target_return_type);
            }
            //Agent case
            else if (string.Compare("Agent", method_path_parts[0]) == 0)
            {
                methods_to_compare = FindConcreteAgentMethods(target_object.GetComponent<AI.Agent_GS>(), target_return_type);
            }
            
            //Iterate the collected methods
            foreach (KeyValuePair<MethodInfo, object> method_data in methods_to_compare)
            {
                //Compare method name
                if (string.Compare(method_data.Key.Name, method_path_parts[method_path_parts.Length - 1]) == 0)
                {
                    //Compare method script name
                    string method_full_name = method_data.Key.ReflectedType.FullName;
                    if(method_full_name.Contains('.'))
                    {
                        string[] parts = method_full_name.Split('.');
                        method_full_name = parts[parts.Length - 1];
                    }
                    if (string.Compare(method_path_parts[method_path_parts.Length - 2], method_full_name) == 0)
                    {
                        return method_data;
                    }
                }
            }

            //Return null if the method is not found
            return new KeyValuePair<MethodInfo, object>();
        }


        //Types Methods =========================
        public static void AllocateFromVariableType(VariableType variable_type, ref object value)
        {
            //Here we basically allocate diferent elements depending of the variable type and set the allocated field to the variable value
            switch (variable_type)
            {
                case VariableType._undefined_var_type:
                    {
                        value = null;
                    }
                    break;
                case VariableType._bool:
                    {
                        bool new_bool = false;
                        value = new_bool;
                    }
                    break;
                case VariableType._int:
                    {
                        int new_int = 0;
                        value = new_int;
                    }
                    break;
                case VariableType._float:
                    {
                        float new_float = 0.0f;
                        value = new_float;
                    }
                    break;
                case VariableType._char:
                    {
                        string new_char = "";
                        value = new_char;
                    }
                    break;
                case VariableType._string:
                    {
                        string new_string = "";
                        value = new_string;
                    }
                    break;
                case VariableType._vector2:
                    {
                        Vector2 new_vector2 = new Vector2(0.0f, 0.0f);
                        value = new_vector2;
                    }
                    break;
                case VariableType._vector3:
                    {
                        Vector3 new_vector3 = new Vector3(0.0f, 0.0f, 0.0f);
                        value = new_vector3;
                    }
                    break;
                case VariableType._vector4:
                    {
                        Vector4 new_vector4 = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
                        value = new_vector4;
                    }
                    break;
            }
        }

        private static Dictionary<string, System.Type> system_type_map = new Dictionary<string, System.Type>();

        //Get all system types in assemblies ====
        public static System.Type[] GetAllSystemTypes()
        {
            //Allocate types list
            List<Type> types = new List<Type>();
            //Iterate all the loaded assemblies
            foreach (Assembly asm in assemblies)
            {
                //Add all the exportes type of the current asm
                types.AddRange(asm.GetExportedTypes());
            }
            //Return the generated list as array
            return types.ToArray();
        }

        public static Type ToSystemType(this VariableType var_type)
        {
            switch (var_type)
            {
                case VariableType._bool: return typeof(bool);
                case VariableType._int: return typeof(int);
                case VariableType._float: return typeof(float);
                case VariableType._char: return typeof(char);
                case VariableType._string: return typeof(string);
                case VariableType._vector2: return typeof(Vector2);
                case VariableType._vector3: return typeof(Vector3);
                case VariableType._vector4: return typeof(Vector4);
            }

            //No found type return
            return null;
        }

        public static string ToShortString(this VariableType var_type)
        {
            switch (var_type)
            {
                case VariableType._bool: return "bool";
                case VariableType._int: return "int";
                case VariableType._float: return "float";
                case VariableType._char: return "char";
                case VariableType._string: return "string";
                case VariableType._vector2: return "vector2";
                case VariableType._vector3: return "vector3";
                case VariableType._vector4: return "vector4";
            }

            //No found type return
            return "undef";
        }

        public static VariableType ToVariableType(this string system_type)
        {
            switch(system_type)
            {
                case "System.Boolean": return VariableType._bool;
                case "System.Int32": return VariableType._int;
                case "System.Single": return VariableType._float;
                case "System.Double": return VariableType._float;
                case "System.Char": return VariableType._char;
                case "System.String": return VariableType._string;
                case "System.Vector2": return VariableType._vector2;
                case "System.Vector3": return VariableType._vector3;
                case "System.Vector4": return VariableType._vector4;
            }
            return VariableType._undefined_var_type;
        }

        public static VariableType ToVariableType(this System.Type type)
        {
            if (type == typeof(bool)) return VariableType._bool;
            if (type == typeof(int)) return VariableType._int;
            if (type == typeof(float)) return VariableType._float;
            if (type == typeof(double)) return VariableType._float;
            if (type == typeof(char)) return VariableType._char;
            if (type == typeof(string)) return VariableType._string;
            if (type == typeof(Vector2)) return VariableType._vector2;
            if (type == typeof(Vector3)) return VariableType._vector3;
            if (type == typeof(Vector4)) return VariableType._vector4;

            return VariableType._undefined_var_type;
        }

        public static OperatorType[] GetValidPassiveOperatorTypes(this VariableType variable_type)
        {
            switch (variable_type)
            {
                case VariableType._string:
                case VariableType._bool: return new OperatorType[] { OperatorType._equal_equal, OperatorType._different };
                case VariableType._int:
                case VariableType._float:
                case VariableType._char:
                case VariableType._vector2:
                case VariableType._vector3:
                case VariableType._vector4: return new OperatorType[] { OperatorType._equal_equal, OperatorType._different, OperatorType._smaller, OperatorType._smaller_or_equal, OperatorType._bigger, OperatorType._bigger_or_equal };
            }

            //No found type return
            return null;
        }

        public static OperatorType[] GetValidActiveOperatorTypes(this VariableType variable_type)
        {
            switch (variable_type)
            {
                case VariableType._string:
                case VariableType._bool: return new OperatorType[] { OperatorType._is_equal };
                case VariableType._int:
                case VariableType._float:
                case VariableType._char:
                case VariableType._vector2:
                case VariableType._vector3:
                case VariableType._vector4: return new OperatorType[] { OperatorType._plus_equal, OperatorType._minus_equal, OperatorType._is_equal };
            }

            //No found type return
            return null;
        }

        public static string ToShortString(this OperatorType type)
        {
            switch (type)
            {
                case OperatorType._undefined_operator: return "Undefined";
                case OperatorType._equal_equal: return "==";
                case OperatorType._different: return "!=";
                case OperatorType._smaller: return "<";
                case OperatorType._smaller_or_equal: return "<=";
                case OperatorType._bigger: return ">";
                case OperatorType._bigger_or_equal: return ">=";
                case OperatorType._is_equal: return "=";
                case OperatorType._plus_equal: return "+=";
                case OperatorType._minus_equal: return "-=";
            }
            return "Undefined";
        }

        public static string[] ToShortStrings(this OperatorType[] operator_types)
        {
            //First allocate a strings array with the size of the operator types
            string[] strings = new string[operator_types.Length];
            //Iterate the operator types and transform them to strings
            for (int k = 0; k < operator_types.Length; k++)
            {
                strings[k] = operator_types[k].ToShortString();
            }
            //Finally return the generated strings array
            return strings;
        }

        public static Type ToSystemType(this string type_string)
        {
            Type system_type = null;

            //Try get the system type value in the map, value will be found if we already use it before
            if (system_type_map.TryGetValue(type_string, out system_type))
            {
                //Return the found type
                return system_type;
            }

            //Try to get type by system current assembly
            system_type = Type.GetType(type_string);
            if (system_type != null)
            {
                //If we find the type we store int the dictionary for the next search
                return system_type_map[type_string] = system_type;
            }

            //Try to find the type in the loded assemblies
            foreach (Assembly asm in assemblies)
            {
                //Try get type 
                system_type = asm.GetType(type_string);
                //If not continue search
                if (system_type == null) continue;
                //If found  store in the dictionary
                return system_type_map[type_string] = system_type;
            }

            //Finally try to find the type in all the assemblies in the project
            System.Type[] system_types = GetAllSystemTypes();
            //Iterate all found system types in assemblies exported ones
            foreach (System.Type s_type in system_types)
            {
                //Compare using names
                if (s_type.Name == type_string)
                {
                    return system_type_map[type_string] = system_type;
                }
            }

            return system_type;
        }

        public static string ToShortString(this System.Type type)
        {
            if (type == typeof(bool)) return "bool";
            if (type == typeof(int)) return "int";
            if (type == typeof(float) || type == typeof(double)) return "float";
            if (type == typeof(char)) return "char";
            if (type == typeof(string)) return "string";
            if (type == typeof(Vector2)) return "vector2";
            if (type == typeof(Vector3)) return "vector3";
            if (type == typeof(Vector4)) return "vector4";

            //No found type return
            return "undef";
        }

        //UI Generation Methods =================
        public static void ValueFieldByVariableType(VariableType variable_type, ref object value)
        {
            #if UNITY_EDITOR
            //Generate an input field adapted to the type of the variable
            switch (variable_type)
            {
                case VariableType._undefined_var_type:
                    {
                        GUILayout.Label("Type Error");
                    }
                    break;
                case VariableType._bool:
                    {
                        value = GUILayout.Toggle((bool)value, "", GUILayout.MaxWidth(70.0f));
                    }
                    break;
                case VariableType._int:
                    {
                        value = EditorGUILayout.IntField((int)value, GUILayout.MaxWidth(70.0f));
                    }
                    break;
                case VariableType._float:
                    {
                        value = EditorGUILayout.FloatField((float)value, GUILayout.MaxWidth(70.0f));
                    }
                    break;
                case VariableType._char:
                    {
                        value = EditorGUILayout.TextField("", (string)value, GUILayout.MaxWidth(70.0f));
                        //Limit value to one char
                        if (!string.IsNullOrEmpty((string)value))
                        {
                            value = ((string)value).Substring(0, 1);
                        }
                    }
                    break;
                case VariableType._string:
                    {
                        value = EditorGUILayout.TextField("", (string)value, GUILayout.MaxWidth(70.0f));
                    }
                    break;
                case VariableType._vector2:
                    {
                        //Value field
                        value = EditorGUILayout.Vector2Field("", (Vector2)value, GUILayout.MaxWidth(110.0f));
                    }
                    break;
                case VariableType._vector3:
                    {
                        //Value field
                        value = EditorGUILayout.Vector3Field("", (Vector3)value, GUILayout.MaxWidth(110.0f));
                    }
                    break;
                case VariableType._vector4:
                    {
                        //Value field
                        value = EditorGUILayout.Vector4Field("", (Vector4)value, GUILayout.MaxWidth(150.0f));
                    }
                    break;
            }
            #endif
        }

        //Dropdowns system ======================
        //Dropdowns index
        private static int[] dropdown_select = new int[INITIAL_ARRAY_SIZE] { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2 };
        
        //Dropdown data class
        public class DropDownData_GS
        {
            public int selected_index = -1;
            public string[] paths = null;
            public string[] display_paths = null;
            public int dropdown_slot = -2;
        }

        //Get dropdown slot method
        public static int GetDropdownSlot()
        {
            for (int k = 0; k < dropdown_select.Length; k++)
            {
                if (dropdown_select[k] == -2)
                {
                    dropdown_select[k] = -1;
                    return k;
                }
            }

            int[] new_array = new int[dropdown_select.Length * 2];

            for (int k = INITIAL_ARRAY_SIZE - 1; k < new_array.Length; k++)
            {
                new_array[k] = -2;
            }

            for (int k = 0; k < dropdown_select.Length; k++)
            {
                new_array[k] = dropdown_select[k];
            }

            dropdown_select = new_array;

            return GetDropdownSlot();
        }

        //Free dropdown slot method
        public static void FreeDropdownSlot(int index)
        {
            //Reset an specific dropdown after checking if index fits inside the array size
            if (index < dropdown_select.Length)
            {
                dropdown_select[index] = -2;
            }
        }
        
        //Free all dropdowns method
        public static void ResetDropdowns()
        {
            for (int k = 0; k < dropdown_select.Length; k++)
            {
                dropdown_select[k] = -2;
            }
        }
        
        //Set specific dropdown index method
        public static void SetDropdownIndex(int dropdown_id, int new_index)
        {
            if (dropdown_id >= dropdown_select.Length || dropdown_id < 0)
            {
                Debug.LogError("You are trying to access a non allocated dropdown index!");
            }
            else dropdown_select[dropdown_id] = new_index;
        }

        //Generate dropdown UI method
        public static void GenerateButtonDropdownMenu(ref int index, string[] options, string button_string, bool show_selection, float button_width, int dropdown_id)
        {
            #if UNITY_EDITOR
            if (GUILayout.Button(dropdown_select[dropdown_id] != -1 && show_selection ? options[dropdown_select[dropdown_id]] : button_string, GUILayout.MaxWidth(button_width)))
            {
                GenericMenu dropdown = new GenericMenu();
                for (int k = 0; k < options.Length; k++)
                {
                    dropdown.AddItem(
                        //Generate gui content from property path strin
                        new GUIContent(options[k]),
                        //show the currently selected item as selected
                        k == index,
                        //lambda to set the selected item to the one being clicked
                        selectedIndex => dropdown_select[dropdown_id] = (int)selectedIndex,
                        //index of this menu item, passed on to the lambda when pressed.
                        k
                   );
                }
                dropdown.ShowAsContext(); //finally show the dropdown
            }
            index = dropdown_select[dropdown_id];
            #endif
        }

        public static bool OpenScriptEditor(System.Type target_type)
        {
            #if UNITY_EDITOR

            //Get asset path by adding folder and type
            string[] file_names = Directory.GetFiles(Application.dataPath, target_type.ToString() + ".cs", SearchOption.AllDirectories);
            //Check if there's more than one asset or no asset, in both cases the result is negative
            if (file_names.Length == 0 || file_names.Length > 1)
            {
                Debug.LogError("Asset not found!");
                return false;
            }
            //Asset found case
            else
            {
                //Get asset full path
                string final_file_name = Path.GetFullPath(file_names[0]);
                //Open asset in the correct file editor
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(final_file_name, 1);
                return true;
            }
            
            #else
            return false;
            #endif
        }

        //Path Methods ==========================
        public static string PathToName(this string original)
        {
            string result;
            int folder_index = original.LastIndexOf('/') + 1;
            int format_index = original.LastIndexOf('.');
            result = original.Substring(folder_index, format_index - folder_index);
            return result;
        }

        public static string FolderToName(this string original)
        {
            string result;
            int folder_index = original.LastIndexOf('/');
            result = original.Substring(folder_index, original.Length - folder_index);
            return result;
        }

        //Rect Methods ==========================
        public static Rect Scale(this Rect rect, float scale, Vector2 pivot)
        {
            Rect scaled = rect;
            scaled.x -= pivot.x;
            scaled.y -= pivot.y;
            scaled.xMin *= scale;
            scaled.xMax *= scale;
            scaled.yMin *= scale;
            scaled.yMax *= scale;
            scaled.x += pivot.x;
            scaled.y += pivot.y;
            return scaled;
        }

        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }

        public static Vector2 MiddleLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin + (rect.height * 0.5f));
        }

        public static Vector2 MiddleRight(this Rect rect)
        {
            return new Vector2(rect.xMax, rect.yMin + (rect.height * 0.5f));
        }

        //Extra Methods =========================
        //Change key in a dictionary
        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey original_key, TKey new_key)
        {
            //First get the value stored by the original key
            TValue value = dictionary[original_key];
            //Next remove the variable with the original key
            dictionary.Remove(original_key);
            //Finally add a new variable with the value that we get and store it with the new key
            dictionary[new_key] = value;
        }
    }
}