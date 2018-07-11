using System;
using System.Linq;
using LibRetro.Types;

namespace RetroLite.RetroCore
{
    public class CoreVariable
    {
        private string _value;

        public string Name { get; }

        public string Description { get; }

        public string[] ExpectedValues { get; }

        public string Value
        {
            get => _value;
            set
            {
                if (value != null && !ExpectedValues.Contains(value))
                {
                    throw new Exception("Invalid variable");
                }
                
                _value = value;
            }
        }

        public CoreVariable(in RetroVariable variable)
        {
            Name = variable.Key;

            var data = variable.Value.Split(';');
            
            Description = data[0];
            ExpectedValues = data[1].Split('|');
            Value = ExpectedValues[0];
        }
    }
}