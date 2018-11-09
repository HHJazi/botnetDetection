using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Client.ProcessMonitor
{
    public class ErrorEventArgs2 : EventArgs
    {
        public string Message { get; set; }

        public ErrorEventArgs2(string msg)
            : base()
        {
            Message = msg;
        }

    }
}
