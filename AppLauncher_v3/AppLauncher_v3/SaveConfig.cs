using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLauncher_v3
{
    [Serializable]
    public struct SaveConfig
    {

        public Dictionary<string, string[]> dict;
        public SaveConfig(Dictionary<string,string[]> dict)
        {
            this.dict = dict;
        }
    }
}
