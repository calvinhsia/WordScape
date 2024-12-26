using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WordScape;

namespace WordScapeTests
{
    public class BaseTestClass : ILogger
    {
        public TestContext TestContext { get; set; }

        public List<string> _lstLoggedStrings;

        [TestInitialize]
        public void TestInitialize()
        {
            _lstLoggedStrings = new List<string>();
            LogMessage($"Starting test {TestContext.TestName}");

        }
        public void LogMessage(string str, params object[] args)
        {
            var dt = string.Format("[{0}],",
                DateTime.Now.ToString("hh:mm:ss:fff")
                ) + $"{Thread.CurrentThread.ManagedThreadId,2} ";
            str = string.Format(dt + str, args);
            var msgstr = $" {str}";

            this.TestContext.WriteLine(msgstr);
            if (Debugger.IsAttached)
            {
                Debug.WriteLine(msgstr);
            }
            _lstLoggedStrings.Add(msgstr);
        }
        /// <summary>
        /// Creates a custom STA thread on which UI elements can run. Has execution context that allows asynchronous code to work
        /// </summary>
        public static async Task RunInSTAExecutionContextAsync(Func<Task> actionAsync, string description = "", int maxStackSize = 512 * 1024)
        {
            Dispatcher mySTADispatcher = null;
            var tcsGetExecutionContext = new TaskCompletionSource<int>();
            //            var tcsStaThreadDone = new TaskCompletionSource<int>();
            var myStaThread = new Thread(() =>
            {
                mySTADispatcher = Dispatcher.CurrentDispatcher;
                var syncContext = new DispatcherSynchronizationContext(mySTADispatcher); // Create/install the context
                SynchronizationContext.SetSynchronizationContext(syncContext);
                tcsGetExecutionContext.SetResult(0);// notify that sync context is ready
                try
                {
                    Dispatcher.Run();  // Start the Dispatcher Processing
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception) { }
                Debug.WriteLine($"Thread done {description}");
                //                tcsStaThreadDone.SetResult(0);
            }, maxStackSize: maxStackSize)
            {
                IsBackground = true,
                Name = $"MySta{description}" // can be called from within the same context (e.g. a prog bar) so distinguish thread names
            };
#pragma warning disable CA1416 // Validate platform compatibility
            myStaThread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416 // Validate platform compatibility
            myStaThread.Start();
            await tcsGetExecutionContext.Task; // wait for thread to set up STA sync context
            var tcsCallerAction = new TaskCompletionSource<int>();
            if (mySTADispatcher == null)
            {
                throw new NullReferenceException(nameof(mySTADispatcher));
            }

            await mySTADispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await actionAsync();
                }
                catch (Exception ex)
                {
                    tcsCallerAction.SetException(ex);
                    return;
                }
                finally
                {
                    Debug.WriteLine($"User code done. Shutting down dispatcher {description}");
                    mySTADispatcher.InvokeShutdown();
                }
                //              await tcsStaThreadDone.Task; // wait for STA thread to exit
                Debug.WriteLine($"StaThreadTask done");
                tcsCallerAction.SetResult(0);
            });
            await tcsCallerAction.Task;
            Debug.WriteLine($"sta thread finished {description}");
        }

    }

}