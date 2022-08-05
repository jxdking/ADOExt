using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public class Sql
    {
        private Dictionary<string, IDbDataParameter> parameters;
        private string text;

        public object DynaParameters { get; private set; }

        public string Text
        {
            get => text; 
            set
            {
                text = value;
                Command = null;
            }
        }

        public Dictionary<string, IDbDataParameter> Parameters
        {
            get
            {
                if (parameters == null)
                {
                    throw new InvalidOperationException("When Sql is initialized with Dynamic Object as the Parameters, Parameters dictionary" +
                        " will not be available until the Sql is run.");
                }
                return parameters;
            }
            set
            {
                parameters = value;
                DynaParameters = null;
            }
        }
        public int CommandTimeout { get; set; } = 30;
        public bool CacheDataReaderSchema { get; set; } = false;
        public IDbCommand Command { get; set; }

        protected virtual void Init(string cmdText, IEnumerable<IDbDataParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(cmdText))
            {
                throw new ArgumentNullException("sql text is required!");
            }
            this.parameters = parameters.ToDictionary(i => i.ParameterName);
            Text = cmdText;
        }

        public Sql(string cmdText, IEnumerable<IDbDataParameter> parameters)
        {
            Init(cmdText, parameters);
        }

        public Sql(string cmdText, params IDbDataParameter[] parameters)
        {
            Init(cmdText, parameters);
        }

        public Sql(string cmdText, object parameters)
        {
            if (string.IsNullOrWhiteSpace(cmdText))
            {
                throw new ArgumentNullException("sql text is required!");
            }
            Text = cmdText;
            DynaParameters = parameters;
        }

        public T[] ParseParameters<T>() where T : IDbDataParameter, new()
        {
            if (parameters != null)
            {
                return parameters.Values.Cast<T>().ToArray();
            }
            var ps = DynaParameters.GetType().GetProperties().Select(i => new T()
            {
                ParameterName = i.Name,
                Direction = ParameterDirection.Input,
                Value = i.GetValue(DynaParameters)
            }).ToArray();
            Parameters = ps.ToDictionary(i => i.ParameterName, i => (IDbDataParameter)i);
            return ps;
        }

        public override string ToString()
        {
            return Text;
        }

        public static implicit operator Sql(string cmdText) => new Sql(cmdText);
    }
}
