﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GOAP_S.Blackboard
{
    public class Blackboard_GS
    {
        [SerializeField] private string _id;
        [SerializeField] private Dictionary<string, Variable_GS> _variables = new Dictionary<string, Variable_GS>();

        //Varibles methods ================
        public Variable_GS AddVariable(string name, PT.VariableType type, object value)
        {
            //Generate the new variable
            //First get system type of the object value
            System.Type variable_system_type = typeof(TVariable_GS<>).MakeGenericType(new System.Type[] { value.GetType() });
            //Instantiate a variable with the system type
            Variable_GS new_variable = (Variable_GS)System.Activator.CreateInstance(variable_system_type);
            //Set new var name 
            new_variable.name = name;
            //Set new var GOAP type
            new_variable.type = type;
            //Set new var object value
            new_variable.object_value = value;

            //Add the new var to the bb list
            _variables.Add(new_variable.id, new_variable);

            return new_variable;
        }

        public bool RemoveVariable(string key)
        {
            Variable_GS find_var = null;
            //Variable found case
            if (_variables.TryGetValue(key, out find_var))
            {
                return _variables.Remove(key);
            }
            //Variable not found case
            else
            {
                return false;
            }
        }

        public void ClearBlackboard()
        {
            //Clear all the variables in the blackboard
            _variables.Clear();
        }

        //Get/Set methods =================
        public Dictionary<string, Variable_GS> variables
        {
            get
            {
                return _variables;
            }
        }

        public int id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = System.Guid.NewGuid().ToString();
                }
                return _id.GetHashCode();
            }
        }
    }
}