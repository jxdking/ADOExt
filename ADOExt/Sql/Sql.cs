using System;
using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public class Sql : ISql
    {
        private const int _DefaultTimeOut = 30;

        public string Text { get; set; }

        public List<Parameter> Parameters { get; set; }

        public int CommandTimeout { get; set; }

        private void Init(string cmdText, IEnumerable<Parameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(cmdText))
            {
                throw new ArgumentNullException("sql text is required!");
            }
            Parameters = new List<Parameter>();
            Parameters.AddRange(parameters);
            Text = cmdText;
            CommandTimeout = _DefaultTimeOut;
        }

        public Sql(string cmdText, IEnumerable<Parameter> parameters)
        {
            Init(cmdText, parameters);
        }

        public Sql(string cmdText, params Parameter[] parameters)
        {
            Init(cmdText, parameters);
        }

        public Sql Clone()
        {
            // "ret" needs to have a new array of Parameter. Shallow copy(MemberwiseClone) will not work for this situation.
            var ret = new Sql(Text, Parameters);
            ret.CommandTimeout = CommandTimeout;
            return ret;
        }

        public override string ToString()
        {
            return Text;
        }

        public static implicit operator Sql(string cmdText) => new Sql(cmdText);
    }
}