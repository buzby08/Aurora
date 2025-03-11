using System.ComponentModel.Design;

namespace Aurora
{
    // public class Variables
    // {
    //     public Dictionary<string, Type> allVars = [];

    //     public void Add(string name, Type variable)
    //     {
    //         allVars.Add(name, variable);
    //     }

    //     public void Remove(string name)
    //     {
    //         allVars.Remove(name);
    //     }
    // }

    public class Variable
    {
        private string? _value;
        private readonly bool _constant;
        private readonly string? _name;
        private bool _undefined;

        public Variable(string name, bool constant, string? value)
        {
            _constant = constant;
            _value = value;

            _undefined = _value is null;

            bool nameValidCharsOnly = name.All(c => char.IsLetterOrDigit(c) || c == '_');
            bool nameStartValid = !char.IsDigit(name[0]);

            if (nameValidCharsOnly && nameStartValid) { _name = name; }

            else { throw new FormatException("Variable names must not start with a digit!"); }
        }

        public string? Value
        {
            get => _value;
            set 
            {
                bool isConstant = _value is not null || _constant;
                bool canBeModified = !isConstant;
                if (canBeModified)
                {
                    _value = value;
                    _undefined = value is null;
                    return;
                }

                if (isConstant)
                {
                    throw new InvalidOperationException($"'{GlobalVariables.ReprString(_name)}' is a constant and cannot be modified");
                }

                throw new ArgumentException("The given value is not valid for variable type 'String'");
            }
        }

        public bool Constant
        {
            get => _constant;
        }

        public string? Name
        {
            get => _name;
        }

        public bool Undefined
        {
            get => _undefined;
        }
    }
}