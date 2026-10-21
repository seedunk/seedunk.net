using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Transactions;

namespace  Seedunk.Net.Utils
{



    public class AccurateTimer
    {
        public delegate bool ManualTimerEventHandler(object sender);

        public event ManualTimerEventHandler Tick;

        private long clockFrequency;            // result of QueryPerformanceFrequency()

        CancellationTokenSource cts = new CancellationTokenSource();

        private Thread timerThread;

        private int intervalMs;                     // interval in mimliseccond;

        ///
        /// Timer inteval in milisecond
        ///
        public int Interval
        {
            get { return intervalMs; }
            set
            {
                intervalMs = value;
                intevalTicks = (long)(value * (double)clockFrequency / 1000);
            }
        }

        private long intevalTicks;

        ///
        /// Pointer to a variable that receives the current performance-counter value, in counts.
        ///
        ///
        /// If the function succeeds, the return value is nonzero.
        ///
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        ///
        /// Pointer to a variable that receives the current performance-counter frequency,
        /// in counts per second.
        /// If the installed hardware does not support a high-resolution performance counter,
        /// this parameter can be zero.
        ///
        ///
        /// If the installed hardware supports a high-resolution performance counter,
        /// the return value is nonzero.
        ///
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public AccurateTimer()
        {
            if (QueryPerformanceFrequency(out clockFrequency) == false)
            {
                // Frequency not supported
                throw new Win32Exception("QueryPerformanceFrequency() function is not supported");
            }
        }

        ///
        /// 进程主程序
        ///
        ///
        private void ThreadProc(CancellationToken token)
        {

            long currTime;
            long nextTriggerTime;               // the time when next task will be executed
            GetTick(out currTime);
            nextTriggerTime = currTime + intevalTicks;
            while (!token.IsCancellationRequested)
            {
                while (currTime < nextTriggerTime)
                {
                    GetTick(out currTime);
                }   // wailt an interval
                nextTriggerTime = currTime + intevalTicks;
                if (Tick != null)
                {
                    if (!Tick(this))
                    {
                        cts.Cancel();
                        break;
                    }
                }
            }

            Console.WriteLine("Cleaning up...");
            // 取消标记为请求状态，但实际取消还需要一段时间
            // token.ThrowIfCancellationRequested();



        }

        public bool GetTick(out long currentTickCount)
        {
            if (QueryPerformanceCounter(out currentTickCount) == false)
                throw new Win32Exception("QueryPerformanceCounter() failed!");
            else
                return true;
        }

        public bool Start()
        {
            if (timerThread == null)
            {
                timerThread = new Thread(() => ThreadProc(cts.Token));
                timerThread.Name = "HighAccuracyTimer";
                timerThread.Priority = ThreadPriority.Highest;
                timerThread.Start();

                //  timerThread.Join();
                // timerThread = null;
                return true;
            }
            else
            {
                return false;
            }



        }

        public void Stop()
        {
            cts.Cancel();
            timerThread.Join();
            timerThread = null;
        }


        ~AccurateTimer()
        {
            cts.Cancel();
        }


    }
}