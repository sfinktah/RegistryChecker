using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SteamEdit
{
    public class RegistryRow
    {
            public string Key { get; set; }
            public string Value { get; set; }
            public string Data {
            get { return Convert.ToString(Registry.GetValue(Key, Value, "")); }
            set { } }
    }
}
