using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Biotracker.Client
{
    public abstract class WorkerThread
    {
        public enum WorkerState
        { 
            STARTING = 0,
            RUNNING,
            PENDING,
            STOPPED
        }
        protected Thread _thread;
        private WorkerState _state;

        protected EventWaitHandle[] _eventArray;

        public WorkerThread()
        {
            _state = WorkerState.STARTING;

            _eventArray = new EventWaitHandle[] {
                new ManualResetEvent(false), //Exit thread event
            };

            _thread = new Thread(new ThreadStart(this.RunThread));
        }

        public void Start()
        {
            if (_thread != null && _thread.IsAlive == false)
            {
                try
                {
                    _thread.Start();

                    while (!_thread.IsAlive) ;

                    SetThreadState(WorkerState.RUNNING);
                }
                catch (ThreadStateException tse)
                {
                    //The thread state doesn't allow the Start() operation
                    //BTDebug.BTTrace("MKr", tse.ToString()); 
                }
            }
        }
        
        public WorkerState GetThreadState()
        {
            return _state;
        }

        public void SetThreadState(WorkerState state)
        {
            _state = state;
        }

        /// <summary>
        /// Caller uses this function to ask worker thread exit gracefully
        /// </summary>
        public virtual void RequestStop()
        {
            _eventArray[0].Set();

        }

        public abstract void RunThread();
    }
}
