using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoClick.Model
{
    public class ActionEntry
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int? Value { get; set; }

        public ButtonType ButtonType { get; set; }

        public ClickType ClickType { get; set; } = ClickType.click;
    }
}
