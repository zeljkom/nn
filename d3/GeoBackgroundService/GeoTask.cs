using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.UI.Notifications;

namespace GeoBackgroundService {
    public sealed class GeoTask: IBackgroundTask {
        BackgroundTaskDeferral _deferral = null;
        Accelerometer _accelerometer = null;
        Geolocator _locator = new Geolocator();
        private string longi="";
        private string lati = "";
        

        public void Run(IBackgroundTaskInstance taskInstance) {
            _deferral = taskInstance.GetDeferral();
            try {
                // force gps quality readings
                _locator.DesiredAccuracy = PositionAccuracy.High;
                _locator.ReportInterval = 200;

                taskInstance.Canceled += taskInstance_Canceled;

                _accelerometer = Windows.Devices.Sensors.Accelerometer.GetDefault();
                _accelerometer.ReportInterval = _accelerometer.MinimumReportInterval > 5000 ? _accelerometer.MinimumReportInterval : 5000;
                _accelerometer.ReadingChanged += accelerometer_ReadingChanged;
                _locator.PositionChanged += _locator_PositionChanged;

            } catch (Exception ex) {
                // Add your chosen analytics here
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        void _locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args) {
            string longiNew = args.Position.Coordinate.Point.Position.Longitude.ToString("f5");
            string latiNew = args.Position.Coordinate.Point.Position.Latitude.ToString("f5");
            if (longi != longiNew || lati != latiNew ) {
                ShowToastNotification(longiNew, latiNew);
                UpdateTile(longiNew, latiNew);
            }
            if (longi != longiNew) longi = longiNew;
            if (lati != latiNew) lati = latiNew;            
        }

        private void UpdateTile(string longiNew, string latiNew) {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);
            String position = latiNew + ";" + longiNew;

            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = position;
                        XmlDocument wideTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text01);
            XmlNodeList wideNode = wideTile.GetElementsByTagName("text");
            wideNode[0].InnerText = position;

            IXmlNode node = tileXml.ImportNode(wideTile.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);
            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(10);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);     
        }

        void taskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason) {
            _deferral.Complete();
        }

        async void accelerometer_ReadingChanged(Windows.Devices.Sensors.Accelerometer sender, Windows.Devices.Sensors.AccelerometerReadingChangedEventArgs args) {
            try {
                if (_locator.LocationStatus != PositionStatus.Disabled) {
                    try {
                        Geoposition pos = await _locator.GetGeopositionAsync();

                        XmlDocument xml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text03);
                        var tileElements = xml.GetElementsByTagName("text");
                        tileElements[0].AppendChild(xml.CreateTextNode(pos.Coordinate.Point.Position.Latitude.ToString("f5")));
                        tileElements[1].AppendChild(xml.CreateTextNode(pos.Coordinate.Point.Position.Longitude.ToString("f5")));
                        
                        TileNotification tile = new TileNotification(xml);
                        TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
                        updater.Update(tile);
                    } catch (Exception ex) {
                        if (ex.HResult != unchecked((int)0x800705b4)) {
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                    }
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public void Dispose() {
            if (_accelerometer != null) {
                _accelerometer.ReadingChanged -= accelerometer_ReadingChanged;
                _accelerometer.ReportInterval = 0;
            }
        }

        private void ShowToastNotification(string lati, string longi) {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var toastText = toastXml.GetElementsByTagName("text");
            (toastText[0] as XmlElement).InnerText = lati + ";" + longi;
            var toast = new ToastNotification(toastXml);
            toastNotifier.Show(toast);
        }
    }
}
