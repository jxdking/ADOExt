using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public class Sql
    {
        private string text;

        public string Text
        {
            get => text; 
            set
            {
                text = value;
                Command = null;
            }
        }

        public Dictionary<string, IDbDataParameter> Parameters { get; set; }
      
        public IDbCommand Command { get; private set; }

        /// <summary>
        /// When the sql is query command. Application will cache the DataReader's schema to speedup object mapping.
        /// Set the priority of the cache.
        /// </summary>
        public CacheItemPriority SchemaCachePriority { get; set; } = CacheItemPriority.Low;

        protected virtual void Init(string cmdText, IEnumerable<IDbDataParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(cmdText))
            {
                throw new ArgumentNullException("sql text is required!");
            }
            Parameters = parameters.ToDictionary(i => i.ParameterName);
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

        public void SetupCommand(IDbCommand command) {
            foreach (var p in Parameters.Values.ToList()) {
                if (p is Parameter) {
                    // p has to be transformed to native Parameter type.
                    var native = command.CreateParameter();
                    native.ParameterName = p.ParameterName;
                    native.Value = p.Value;
                    native.Direction = p.Direction;
                    Scrub(native);
                    command.Parameters.Add(native);
                    Parameters[p.ParameterName] = native;
                    continue;
                }

                // p should be a native type, such as SqlParamter, or OracleParameter.
                Scrub(p);
                command.Parameters.Add(p);
            }
            Command = command;
        }

        private void Scrub(IDbDataParameter parameter)
        {
            if (parameter.Value == null)
            {
                parameter.Value = DBNull.Value;
            }
            if (parameter.Direction != ParameterDirection.Input)
            {
                parameter.Size = short.MaxValue; // remove the size limitation of the parameter.
            }
        }

        public override string ToString()
        {
            return Text;
        }

        public static implicit operator Sql(string cmdText) => new Sql(cmdText);
    }
}
