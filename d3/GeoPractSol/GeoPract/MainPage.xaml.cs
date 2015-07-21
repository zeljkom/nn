using GeoPract.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NotificationsExtensions;
using NotificationsExtensions.TileContent;
using Windows.UI.Notifications;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace GeoPract {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private Geolocator locator;
        Geopoint currentLocation;
        string longi;
        string lati;
        

        public MainPage() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.locator = new Geolocator();
            this.locator.ReportInterval = 200;
            locator.PositionChanged += locator_PositionChanged;
        }

        void locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args) {
            var position = args.Position;
            currentLocation = position.Coordinate.Point;
            string longiNew = position.Coordinate.Point.Position.Longitude.ToString();
            string latiNew = position.Coordinate.Point.Position.Latitude.ToString();

            //this.currentLocation = position.Coordinate.Point;
            //Zbog prčkanja po UI threadu 
            //Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync
            //(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {



                if (longi != longiNew || lati != latiNew) {
                    UpdateMap(); //Ažuriraj poziciju na mapi
                    //UpdateUICoords(); //ažuriraj prikaz na sučelju
                    //UpdateFile(); //Ažuriraj poziciju u fajl    
                }

                if (longi != longiNew) longi = longiNew;
                if (lati != latiNew) lati = latiNew;

            //});            
        }

        private async void UpdateMap() {
            //var pushpin = new Windows.UI.Xaml.Shapes.Ellipse();
            //pushpin.Fill = new SolidColorBrush(Colors.Red);
            //pushpin.Height = 20;
            //pushpin.Width = 20;
            //if (MyMap.Children.Count > 0) MyMap.Children.RemoveAt(0);
            //MyMap.Children.Add(pushpin);
            //MapControl.SetLocation(pushpin, currentLocation);
            //MapControl.SetNormalizedAnchorPoint(pushpin, new Point(0.5, 0.5));
            await MyMap.TrySetViewAsync(currentLocation, 15, 0, 0, MapAnimationKind.Linear);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
            this.UpdateTile();
        }

        private void UpdateTile() {
            ITileSquare150x150PeekImageAndText01 tile = TileContentFactory.CreateTileSquare150x150PeekImageAndText01();
            tile.TextBody1.Text = "Ja sam tajlić";
            tile.TextBody2.Text = "Ja sam tajlić 2";
            tile.TextBody3.Text = "Ja sam tajlić 3";
            tile.Image.Src = "ms-appx:///assets/Square71x71Logo.scale-240.png";

            TileNotification notification = new TileNotification(tile.GetXml());
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Update(notification);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void ZoomIn_Click(object sender, RoutedEventArgs e) {
            if (MyMap.ZoomLevel < MyMap.MaxZoomLevel) 
            MyMap.ZoomLevel += 1;
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e) {
            if (MyMap.ZoomLevel > MyMap.MinZoomLevel)
                MyMap.ZoomLevel -= 1;
        }
    }
}
