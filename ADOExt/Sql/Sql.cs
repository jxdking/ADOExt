﻿using System;
using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public class Sql
    {
        public static int DefaultCommandTimeout = 30;

        public string Text { get; set; }

        public HashSet<Parameter> Parameters { get; set; }

        public int CommandTimeout { get; set; }

        protected virtual void Init(string cmdText, IEnumerable<Parameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(cmdText))
            {
                throw new ArgumentNullException("sql text is required!");
            }
            Parameters = new HashSet<Parameter>(parameters);
            Text = cmdText;
            CommandTimeout = DefaultCommandTimeout;
        }

        public Sql(string cmdText, IEnumerable<Parameter> parameters)
        {
            Init(cmdText, parameters);
        }

        public Sql(string cmdText, params Parameter[] parameters)
        {
            Init(cmdText, parameters);
        }

        public override string ToString()
        {
            return Text;
        }

        public static implicit operator Sql(string cmdText) => new Sql(cmdText);
    }
}
