using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
#if __MonoCS__
using Mono.Unix;
using System.Threading;
#endif

namespace WCell.Util
{
    public static class AppUtil
    {
        #region System Helpers

        /// <summary>
        /// Gets a value indicating if the operating system is a UNIX derivative.
        /// </summary>
        public static bool IsMONO
        {
            get
            {
                var t = Type.GetType("Mono.Runtime");
                return t != null;
            }

        }

        /// <summary>
        /// Gets a value indicating if the operating system is a UNIX derivative.
        /// </summary>
        public static bool IsUNIX
        {
            get
            {
                var p = (int)Environment.OSVersion.Platform;
                return ((p == 4) || (p == 6) || (p == 128));
            }
        }

        /// <summary>
        /// Gets a value indicating if the operating system is a Windows 2000 or a newer one.
        /// </summary>
        public static bool IsWindows2000OrNewer
        {
            get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 5); }
        }

        /// <summary>
        /// Gets a value indicating if the operating system is a Windows XP or a newer one.
        /// </summary>
        public static bool IsWindowsXpOrNewer
        {
            get
            {
                return
                    (Environment.OSVersion.Platform == PlatformID.Win32NT) &&
                    (
                        (Environment.OSVersion.Version.Major >= 6) ||
                        (
                            (Environment.OSVersion.Version.Major == 5) &&
                            (Environment.OSVersion.Version.Minor >= 1)
                        )
                    );
            }
        }

        /// <summary>
        /// Gets a value indicating if the operating system is a Windows Vista or a newer one.
        /// </summary>
        public static bool IsWindowsVistaOrNewer
        {
            get { return (Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.Major >= 6); }
        }

        #endregion

        #region Application Exit Handling
#if !__MonoCS__
        /// <summary>
        /// see: http://geekswithblogs.net/mrnat/archive/2004/09/23/11594.aspx
        /// </summary>
        /// <param name="consoleCtrlHandler"></param>
        /// <param name="Add"></param>
        /// <returns></returns>
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler consoleCtrlHandler, bool Add);

        /// <summary>
        /// Needed for <see cref="SetConsoleCtrlHandler"/>
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
        public delegate bool ConsoleCtrlHandler(CtrlTypes CtrlType);

        /// <summary>
        /// Needed for <see cref="SetConsoleCtrlHandler"/>
        /// </summary>
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        static readonly List<ConsoleCtrlHandler> ctrlHandlers = new List<ConsoleCtrlHandler>();
#endif
        static readonly List<EventHandler> processHooks = new List<EventHandler>();

        /// <summary>
        /// Removes all previously added hooks
        /// </summary>
        public static void UnhookAll()
        {
#if !__MonoCS__
            foreach (var hook in ctrlHandlers)
            {
                SetConsoleCtrlHandler(hook, false);
            }
            ctrlHandlers.Clear();
#endif

            foreach (var hook in processHooks)
            {
                AppDomain.CurrentDomain.ProcessExit -= hook;
            }


            processHooks.Clear();
        }

        /// <summary>
        /// Adds an action that will be executed when the Application exists.
        /// </summary>
        /// <param name="action"></param>
        public static void AddApplicationExitHandler(Action action)
        {
            EventHandler evtHandler = (sender, evt) => action();
            processHooks.Add(evtHandler);

            AppDomain.CurrentDomain.ProcessExit += evtHandler;

#if __MonoCS__
		    SignalHandler unixSignalHandler = () => action();
            SetupSignalHandlers(unixSignalHandler);
            SignalHandlers.Add(unixSignalHandler);
			
#else
            ConsoleCtrlHandler ctrlConsoleCtrlHandler = type =>
            {
                action();
                return false;
            };
            ctrlHandlers.Add(ctrlConsoleCtrlHandler);

            SetConsoleCtrlHandler(ctrlConsoleCtrlHandler, true);
#endif


        }

#if __MonoCS__
	    private static volatile bool _shutdownRequested;

        public delegate void SignalHandler();

        static readonly List<SignalHandler> SignalHandlers = new List<SignalHandler>();

        public static void SetupSignalHandlers(SignalHandler signalHandler)
        {
            UnixSignal[] signals = new UnixSignal[] {
				new UnixSignal (Mono.Unix.Native.Signum.SIGINT),
				new UnixSignal (Mono.Unix.Native.Signum.SIGTERM),
				new UnixSignal (Mono.Unix.Native.Signum.SIGHUP),
			};
            // Ignore SIGPIPE
            Mono.Unix.Native.Stdlib.SetSignalAction(Mono.Unix.Native.Signum.SIGPIPE, Mono.Unix.Native.SignalAction.Ignore);

            var signalThread = new Thread(delegate()
            {
                var signalHandlerTimeout = -1;
                while (!_shutdownRequested)
                {
                    var index = UnixSignal.WaitAny(signals, -1);

                    if (index > 2)
                        continue;

                    if (signalHandler == null)
                        continue;

                    _shutdownRequested = true;
                    signalHandler();
                }
            });

            signalThread.Start();
        }
#endif



        #endregion
    }
}