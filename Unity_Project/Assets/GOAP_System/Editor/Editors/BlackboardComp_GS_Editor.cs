﻿using UnityEngine;
using UnityEditor;
using GOAP_S.Blackboard;
using GOAP_S.AI;

namespace GOAP_S.UI
{
    [CustomEditor(typeof(BlackboardComp_GS))]
    public class BlackboardComp_GS_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            //Blackboard component can be removed immediately so we need to check if still exists 
            if (((BlackboardComp_GS)target).agent == null)
            {
                //Destroy called at the end of ui update to avoid problems
                EditorApplication.delayCall += () => DestroyImmediate(target);
                return;
            }

            //Focus target blackboard
            Blackboard_GS target_blackboard = ((BlackboardComp_GS)target).blackboard;

            GUILayout.BeginVertical();
            //Display agent blackboard
            GUILayout.Label("Local Variables", UIConfig_GS.left_big_style);
            DisplayBlackboardVariables(target_blackboard);
            //Display global blackboard
            GUILayout.Label("Global Variables", UIConfig_GS.left_big_style);
            DisplayBlackboardVariables(GlobalBlackboard_GS.blackboard);

            GUILayout.EndVertical();
        }

        protected void DisplayBlackboardVariables(Blackboard_GS target_blackboard)
        {
            foreach (Variable_GS variable in target_blackboard.variables.Values)
            {
                GUILayout.BeginHorizontal();

                //Show variable type
                GUILayout.Label(variable.type.ToString().Replace('_', ' '), UIConfig_GS.left_bold_style, GUILayout.MaxWidth(40.0f));

                //Show variable name
                GUILayout.Label(variable.name, GUILayout.MaxWidth(100.0f));

                if (variable.is_field_binded)
                {
                    GUILayout.Label(variable.binded_field_short_path, GUILayout.MaxWidth(150.0f));
                }
                else if(variable.is_method_binded)
                {
                    GUILayout.Label(variable.binded_method_short_path, GUILayout.MaxWidth(150.0f));
                }
                else
                {
                    GUILayout.Label(variable.value.ToString(), GUILayout.MaxWidth(150.0f));
                }

                //Variable planning value
                GUILayout.Label(new GUIContent("Pv " + variable.planning_value, "Planning value of the variable. The planning generation algorithm priorizes goals that involve variables with higher planning values"), GUILayout.MaxWidth(40.0f));

                //Free space margin
                GUILayout.FlexibleSpace();
                
                //Delete variable button
                if (!Application.isPlaying)
                {
                    if (GUILayout.Button(new GUIContent("X", "Delete this variable and all the conditions and effects that include it"), GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        //Check if the target variable is global or local
                        bool global = target_blackboard.target_agent == null;

                        //Add remove the current var method to accept menu delegates callback
                        if (global)
                        {
                            //Add remove current var from global blackboard
                            SecurityAcceptMenu_GS.on_accept_delegate += () => target_blackboard.RemoveVariable(variable.name, global);

                            //Iterate all the scene agents and add a delete variable action for each local blackboard
                            Agent_GS[] scene_agents = FindObjectsOfType<Agent_GS>();
                            foreach(Agent_GS agent in scene_agents)
                            {
                                SecurityAcceptMenu_GS.on_accept_delegate += () => agent.blackboard.RemoveVariable(variable.name, global);
                            }
                            //Add delete editor for the global blackboard editor
                            SecurityAcceptMenu_GS.on_accept_delegate += () => GlobalBlackboard_GS_Editor.blackboard_editor.RemoveVariableEditor(variable.name, global);
                        }
                        else
                        {
                            //Add remove current var editor from local blackboard editor to accept menu delegates calback
                            SecurityAcceptMenu_GS.on_accept_delegate += () => target_blackboard.RemoveVariable(variable.name, global);
                            SecurityAcceptMenu_GS.on_accept_delegate += () => NodeEditor_GS.Instance.blackboard_editor.RemoveVariableEditor(variable.name, global);
                        }
                        //Get mouse current position
                        Vector2 mousePos = Event.current.mousePosition;
                        //Open security accept menu on mouse position
                        PopupWindow.Show(new Rect(mousePos.x, mousePos.y, 0, 0), new SecurityAcceptMenu_GS());
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}