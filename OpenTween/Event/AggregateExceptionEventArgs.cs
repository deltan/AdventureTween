using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenTween.Event
{
    class AggregateExceptionEventArgs : EventArgs
    {
        public AggregateException Exception { get; private set; }

        public AggregateExceptionEventArgs(AggregateException ex)
        {
            Exception = ex;
        }
    }
}
