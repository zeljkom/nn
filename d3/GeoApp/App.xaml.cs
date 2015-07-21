﻿using GeoBackgroundService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace GeoApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        
        private TransitionCollection transitions;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;            
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;                
            }

            if (rootFrame.Content == null)
            {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            
            Window.Current.Activate();
            StartBackgroundTask();

        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private  void OnSuspending(object sender, SuspendingEventArgs e)
        {

            
            
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity            
            deferral.Complete();
        }
        
        public static BackgroundTaskRegistration RegisterBackgroundTask(string taskEntryPoint,
                                                                              string taskName,
                                                                              IBackgroundTrigger trigger,
                                                                              IBackgroundCondition condition) {
            //
            // Check for existing registrations of this background task.
            //

            foreach (var cur in BackgroundTaskRegistration.AllTasks) {
                System.Diagnostics.Debug.WriteLine(cur.Value.Name);

                if (cur.Value.Name == taskName) {
                    System.Diagnostics.Debug.WriteLine("Task already registered " + taskName);
                    return (BackgroundTaskRegistration)(cur.Value);
                }
            }

            System.Diagnostics.Debug.WriteLine("Registering new task " + taskName);

            //
            // Register the background task.
            //

            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null) {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = null;

            try {
                task = builder.Register();
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return task;
        }

        private void UnregisterBackgroundTask() {

            var taskRegistration = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault();
            if (taskRegistration != null) { 
                taskRegistration.Unregister(true); 
            }
            
        }

        private async void StartBackgroundTask() {


            var promise = await BackgroundExecutionManager.RequestAccessAsync();

            if (promise == BackgroundAccessStatus.Denied) {
                MessageDialog warningDialog = new MessageDialog("Background execution is disabled. Please re-enable in the Battery Saver app to allow this app to function", "Background GPS");
                await warningDialog.ShowAsync();
            } else {
                var defaultSensor = Windows.Devices.Sensors.Accelerometer.GetDefault();
                if (defaultSensor != null) {
                    var deviceUseTrigger = new DeviceUseTrigger();
                    var deviceUseTask = RegisterBackgroundTask("GeoBackgroundService.GeoTask", "GeoTask", deviceUseTrigger, null);
                    try {
                        DeviceTriggerResult r = await deviceUseTrigger.RequestAsync(defaultSensor.DeviceId);
                        System.Diagnostics.Debug.WriteLine(r);
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
            }            
        }
    }
}