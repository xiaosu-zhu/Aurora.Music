// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace Aurora.Shared.Logging
{
    static class Helper
    {
        public static string GetTimeStamp(this DateTime d)
        {
            return d.ToString("yy-MM-dd HH.mm.ss.ffff");
        }
    }

    /// <summary>
    /// LoggingScenario tells the UI what's happening by 
    /// using the following enums. 
    /// </summary>
    public enum LoggingScenarioEventType
    {
        BusyStatusChanged,
        LogFileGenerated,
        LogFileGeneratedAtDisable,
        LogFileGeneratedAtSuspend,
        LoggingEnabledDisabled
    }

    public class LoggingScenarioEventArgs : EventArgs
    {
        public LoggingScenarioEventArgs(LoggingScenarioEventType type)
        {
            Type = type;
        }

        public LoggingScenarioEventArgs(LoggingScenarioEventType type, string logFilePath)
        {
            Type = type;
            LogFilePath = logFilePath;
        }

        public LoggingScenarioEventArgs(bool enabled)
        {
            Type = LoggingScenarioEventType.LoggingEnabledDisabled;
            Enabled = enabled;
        }

        public LoggingScenarioEventType Type { get; private set; }
        public string LogFilePath { get; private set; }
        public bool Enabled { get; private set; }
    }


    public sealed class LoggingDispatcher : IDisposable
    {

        public void StartLogging()
        {
            CheckDisposed();

            // If no session exists, create one.
            // NOTE: There are use cases where an application
            // may want to create only a channel for sessions outside
            // of the application itself. See MSDN for details. This
            // sample is the common scenario of an app logging events
            // which it wants to place in its own log file, so it creates
            // a session and channel as a pair. The channel is created 
            // during construction of this LoggingScenario class so 
            // it already exsits by the time this function is called. 
            if (session == null)
            {
                session = new FileLoggingSession(DEFAULT_SESSION_NAME);
                session.LogFileGenerated += LogFileGeneratedHandler;
            }

            // This sample adds the channel at level "warning" to 
            // demonstrated how messages logged at more verbose levels
            // are ignored by the session. 
            session.AddLoggingChannel(channel, LoggingLevel.Warning);
        }

        /// <summary>
        /// Toggle the enabled/disabled status of logging. 
        /// </summary>
        /// <returns>True if the resulting new status is enabled, else false for disabled.</returns>
        public async Task<bool> ToggleLoggingEnabledDisabledAsync()
        {
            CheckDisposed();

            IsBusy = true;

            try
            {
                bool enabled;
                if (session != null)
                {
                    string finalLogFilePath = await CloseSessionSaveFinalLogFile();
                    session.Dispose();
                    session = null;
                    if (StatusChanged != null)
                    {
                        StatusChanged.Invoke(this, new LoggingScenarioEventArgs(LoggingScenarioEventType.LogFileGeneratedAtDisable, finalLogFilePath));
                    }
                    ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = false;
                    enabled = false;
                }
                else
                {
                    StartLogging();
                    ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = true;
                    enabled = true;
                }

                if (StatusChanged != null)
                {
                    StatusChanged.Invoke(this, new LoggingScenarioEventArgs(enabled));
                }

                return enabled;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Log(LoggingLevel l, params string[] msgs)
        {
            Task.Run(() =>
            {
                foreach (var item in msgs)
                {
                    channel.LogMessage(item, l);
                }
            });
        }

        public void Log(LoggingLevel l, IEnumerable<KeyValuePair<string, int>> msgs)
        {
            Task.Run(() =>
            {
                foreach (var item in msgs)
                {
                    channel.LogValuePair(item.Key, item.Value, l);
                }
            });
        }

        public void Log(LoggingLevel l, params KeyValuePair<string, int>[] msgs)
        {
            Task.Run(() =>
            {
                foreach (var item in msgs)
                {
                    channel.LogValuePair(item.Key, item.Value, l);
                }
            });
        }

        public void LogEvent(LoggingLevel l, string name, LoggingFields f = null)
        {
            Task.Run(() =>
            {
                channel.LogEvent(name, f, l);
            });
        }

        public void Log(Exception e)
        {
            Task.Run(() =>
            {
                var a = new LoggingFields();
                a.AddString("Type", e.GetType().Name);
                a.AddString("Message", e.Message);

                PushException(ref a, e);

                channel.LogEvent("Exception_Occured", a, LoggingLevel.Error);
            });
        }

        private static void PushException(ref LoggingFields a, Exception m)
        {
            if (m != null)
            {
                a.BeginStruct("Exception");
                a.AddString("Type", m.GetType().Name);
                a.AddString("Message", m.Message ?? string.Empty);
                a.AddString("Source", m.Source ?? string.Empty);
                a.AddInt32("HResult", m.HResult);
                a.AddString("StackTrace", m.StackTrace ?? string.Empty);
                PushException(ref a, m.InnerException);
                a.EndStruct();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// This handler is called by the FileLoggingSession instance when a log
        /// file reaches a size of 256MB. When FileLoggingSession calls this handler, 
        /// it's effectively giving the developer a chance to take ownership of the
        /// log file. Typically, this handler should move or rename the log file.
        /// Note that once this handler has returned, the FileLoggingSession is free
        /// to reuse the original log file name for a new log file at any time.
        /// </summary>
        /// <param name="sender">The FileLoggingSession which has generated a new file.</param>
        /// <param name="args">Contains a StorageFile field LogFileGeneratedEventArgs.File representing the new log file.</param>
        private async void LogFileGeneratedHandler(IFileLoggingSession sender, LogFileGeneratedEventArgs args)
        {
            LogFileGeneratedCount++;
            StorageFolder folder =
                await ApplicationData.Current.LocalFolder.CreateFolderAsync(LOG_FILE_FOLDER_NAME,
                                                                            CreationCollisionOption.OpenIfExists);
            string newLogFileName = $"Log-{DateTime.Now.GetTimeStamp()}.etl";
            await args.File.MoveAsync(folder, newLogFileName);
            if (IsPreparingForSuspend == false)
            {
                StatusChanged?.Invoke(this, new LoggingScenarioEventArgs(LoggingScenarioEventType.LogFileGenerated, System.IO.Path.Combine(folder.Path, newLogFileName)));
            }
        }

        private async Task<string> CloseSessionSaveFinalLogFile()
        {
            // Save the final log file before closing the session.
            StorageFile finalFileBeforeSuspend = await session.CloseAndSaveToFileAsync();
            if (finalFileBeforeSuspend != null)
            {
                LogFileGeneratedCount++;
                // Get the the app-defined log file folder. 
                StorageFolder folder =
                    await ApplicationData.Current.LocalFolder.CreateFolderAsync(LOG_FILE_FOLDER_NAME,
                                                                                CreationCollisionOption.OpenIfExists);
                // Create a new log file name based on a date/time stamp.
                string newLogFileName = $"Log-{DateTime.Now.GetTimeStamp()}.etl";
                // Move the final log into the app-defined log file folder. 
                await finalFileBeforeSuspend.MoveAsync(folder, newLogFileName);
                // Return the path to the log folder.
                return System.IO.Path.Combine(folder.Path, newLogFileName);
            }
            else
            {
                return null;
            }
        }


        #region Scenario code for tracking a LoggingChannel's enablement status and related logging level.

        /// <summary>
        /// This boolean tracks whether or not there are any
        /// sessions listening to the app's channel. This is
        /// adjusted as the channel's LoggingEnabled event is 
        /// raised. Search for OnChannelLoggingEnabled for 
        /// more information.
        /// </summary>
        private bool isChannelEnabled = false;

        /// <summary>
        /// This is the current maximum level of listeners of
        /// the application's channel. It is adjusted as the 
        /// channel's LoggingEnabled event is raised. Search
        /// for OnChannelLoggingEnabled for more information.
        /// </summary>
        private LoggingLevel channelLoggingLevel = LoggingLevel.Verbose;

        void OnChannelLoggingEnabled(ILoggingChannel sender, object args)
        {
            // This method is called when the channel is informing us of channel-related state changes.
            // Save new channel state. These values can be used for advanced logging scenarios where, 
            // for example, it's desired to skip blocks of logging code if the channel is not being
            // consumed by any sessions. 
            isChannelEnabled = sender.Enabled;
            channelLoggingLevel = sender.Level;
        }

        #endregion

        #region Scenario code for suspend/resume.

        private const string LOGGING_ENABLED_SETTING_KEY_NAME = Prefix + "LoggingEnabled";
        private const string LOGFILEGEN_BEFORE_SUSPEND_SETTING_KEY_NAME = Prefix + "LogFileGeneratedBeforeSuspend";

        public bool IsPreparingForSuspend { get; private set; }

        public bool IsLoggingEnabled
        {
            get
            {
                return session != null;
            }
        }

        /// <summary>
        /// Prepare this scenario for suspend. 
        /// </summary>
        /// <returns></returns>
        public async Task PrepareToSuspendAsync()
        {
            CheckDisposed();

            if (session != null)
            {
                IsPreparingForSuspend = true;

                try
                {
                    // Before suspend, save any final log file.
                    string finalFileBeforeSuspend = await CloseSessionSaveFinalLogFile();
                    session.Dispose();
                    session = null;
                    // Save values used when the app is resumed or started later.
                    // Logging is enabled.
                    ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = true;
                    // Save the log file name saved at suspend so the sample UI can be 
                    // updated on resume with that information. 
                    ApplicationData.Current.LocalSettings.Values[LOGFILEGEN_BEFORE_SUSPEND_SETTING_KEY_NAME] = finalFileBeforeSuspend;
                }
                finally
                {
                    IsPreparingForSuspend = false;
                }
            }
            else
            {
                // Save values used when the app is resumed or started later.
                // Logging is not enabled and no log file was saved.
                ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = false;
                ApplicationData.Current.LocalSettings.Values[LOGFILEGEN_BEFORE_SUSPEND_SETTING_KEY_NAME] = null;
            }
        }

        /// <summary>
        /// This is called when the app is either resuming or starting. 
        /// It will enable logging if the app has never been started before
        /// or if logging had been enabled the last time the app was running.
        /// </summary>
        public void ResumeLoggingIfApplicable()
        {
            CheckDisposed();

            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LOGGING_ENABLED_SETTING_KEY_NAME, out object loggingEnabled) == false)
            {
                ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME] = true;
                loggingEnabled = ApplicationData.Current.LocalSettings.Values[LOGGING_ENABLED_SETTING_KEY_NAME];
            }

            if (loggingEnabled is bool && (bool)loggingEnabled == true)
            {
                StartLogging();
            }

            // When the sample suspends, it retains state as to whether or not it had
            // generated a new log file at the last suspension. This allows any
            // UI to be updated on resume to reflect that fact. 
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LOGFILEGEN_BEFORE_SUSPEND_SETTING_KEY_NAME, out object LogFileGeneratedBeforeSuspendObject) &&
                LogFileGeneratedBeforeSuspendObject != null &&
                LogFileGeneratedBeforeSuspendObject is string)
            {
                StatusChanged?.Invoke(this, new LoggingScenarioEventArgs(LoggingScenarioEventType.LogFileGeneratedAtSuspend, (string)LogFileGeneratedBeforeSuspendObject));
                ApplicationData.Current.LocalSettings.Values[LOGFILEGEN_BEFORE_SUSPEND_SETTING_KEY_NAME] = null;
            }
        }

        #endregion

        #region Helper functions/properties/events to support sample UI feedback.

        public event EventHandler<LoggingScenarioEventArgs> StatusChanged;

        private bool isBusy;
        /// <summary>
        /// True if the scenario is busy, or false if not busy.
        /// The UI can use this to affect UI controls.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }

            private set
            {
                isBusy = value;
                StatusChanged?.Invoke(this, new LoggingScenarioEventArgs(LoggingScenarioEventType.BusyStatusChanged));
            }
        }

        /// <summary>
        /// The number of times LogFileGeneratedHandler has been called.
        /// </summary>

        public int LogFileGeneratedCount { get; private set; }

        #endregion

        #region LoggingScenario constants and privates.

        private const string Prefix = "Aurora_Shared_";
        public const string DEFAULT_SESSION_NAME = Prefix + "Session";
        public const string DEFAULT_CHANNEL_NAME = Prefix + "Channel";

        /// <summary>
        /// LoggingScenario moves generated logs files into the 
        /// this folder under the LocalState folder.
        /// </summary>
        public const string LOG_FILE_FOLDER_NAME = Prefix + "LogFiles";

        /// <summary>
        /// <summary>
        /// The sample's one session.
        /// </summary>
        private FileLoggingSession session;

        /// <summary>
        /// The sample's one channel.
        /// </summary>
        private LoggingChannel channel;

        #endregion

        #region LoggingScenario constructor and singleton accessor.

        /// <summary>
        /// Disallow creation of instances beyond the one instance for the process.
        /// The one instance is accessible via the Instance property (see below).
        /// </summary>
        private LoggingDispatcher()
        {
            LogFileGeneratedCount = 0;

            channel = new LoggingChannel(DEFAULT_CHANNEL_NAME, null);

            channel.LoggingEnabled += OnChannelLoggingEnabled;

            // If the app is being launched (not resumed), the 
            // following call will activate logging if it had been
            // activated during the last suspend. 
            ResumeLoggingIfApplicable();
        }

        ~LoggingDispatcher()
        {
            Dispose(false);
        }

        public async Task Suspend()
        {
            // Prepare logging for suspension.
            await PrepareToSuspendAsync();
        }

        public void Resume()
        {
            // If logging was active at the last suspend,
            // ResumeLoggingIfApplicable will re-activate 
            // logging.
            ResumeLoggingIfApplicable();
        }

        /// The app's one and only LoggingScenario instance.
        /// </summary>
        static private LoggingDispatcher loggingScenario;

        /// <summary>
        /// A method to allowing callers to access the app's one and only LoggingScenario instance.
        /// </summary>
        /// <returns>The logging helper.</returns>
        static public LoggingDispatcher Current
        {
            get
            {
                if (loggingScenario == null)
                {
                    loggingScenario = new LoggingDispatcher();
                }
                return loggingScenario;
            }
        }

        #endregion

        #region IDisposable handling

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed == false)
            {
                isDisposed = true;

                if (disposing)
                {
                    if (channel != null)
                    {
                        channel.Dispose();
                        channel = null;
                    }

                    if (session != null)
                    {
                        session.Dispose();
                        session = null;
                    }
                }
            }
        }

        /// <summary>
        /// Set to 'true' if Dispose() has been called.
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// Helper function for other methods to call to check Dispose() state.
        /// </summary>
        private void CheckDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException("LoggingScenario");
            }
        }

        #endregion
    }
}
