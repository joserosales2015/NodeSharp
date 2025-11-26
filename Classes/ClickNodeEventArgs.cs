using NodeSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeSharp.Classes
{
    public class ClickNodeEventArgs : EventArgs
    {
        public FlowNode Node { get; set; }

        public ClickNodeEventArgs(FlowNode node)
        {
            Node = node;
        }
    }
}
