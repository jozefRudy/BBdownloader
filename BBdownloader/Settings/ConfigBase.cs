using System;
using System.Collections;
using System.IO;
using System.Diagnostics;

class ConfigBase
{   
    public ConfigBase()
    {}

    public bool Load(String fname)
    {
        Trace.WriteLine("ConfigBase.Load [" + fname + "]");

        _values.Clear();

        String[] lines;

        lines = File.ReadAllLines(fname);

        for (int ln = 0; ln < lines.Length; ++ln)
        {
            String line = lines[ln].TrimStart();
            if (line.Length == 0 || line[0] == '#') // comment
                continue;
            int eq = line.IndexOf('=');
            if (eq <= 0)
            {
                Trace.WriteLine("error on line [" + (ln + 1) + "] - skip");
                continue;
            }
            String param = line.Substring(0, eq).Trim();
            ++eq;
            String value = line.Substring(eq, line.Length - eq).Trim();
            if (_values.Contains(param))
            {
                Trace.WriteLine("param [" + param + "] already defined - skip");
                continue;
            }
            _values.Add(param, value);
            Trace.WriteLine("[" + param + "]=[" + value + "]");
        }

        return true;
    }

    public String GetValue(String key)
    {
        if (!_values.Contains(key))
            return "";
        return (String)_values[key];
    }

    public int GetValueAsInt(String key)
    {
        int value = 0;
        String str = GetValue(key);
        if (str.Length > 0)
            int.TryParse(str, out value);
        return value;
    }

    protected SortedList _values = new SortedList();
}
