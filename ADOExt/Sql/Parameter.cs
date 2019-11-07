using System.Data;

namespace MagicEastern.ADOExt
{
    public class Parameter
    {
        public string Name;
        public object Value;
        public object Output { get; internal set; }
        public ParameterDirection Direction;

        public Parameter(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            Output = null;
            Name = name;
            Value = value;
            Direction = direction;
        }
    }
}