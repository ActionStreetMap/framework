using System;
using System.IO;

namespace ActionStreetMap.Core.Positioning.Nmea
{
    /// <summary>
    ///     Provides the way to mock real position changes using NMEA files.
    ///     Uses slightly refactored code which is found here: https://code.google.com/p/nmeasharp/.
    ///     TODO refactor classes of this namespace to support used code style.
    /// </summary>
    public sealed class NmeaPositionMocker : IDisposable
    {
        private readonly NmeaParser _parser;
        private readonly IMessageBus _messageBus;

        private GeoPosition _position;
        private DateTime? _positionDateTime;
        private bool _isStarted;

        private const NmeaMessageType CycleStartMsgType = NmeaMessageType.GGA;

        /// <summary> Fired once mocking is done or stopped. </summary>
        public event EventHandler OnDone;

        private void FireDone()
        {
            var tmp = OnDone;
            if (tmp != null)
                tmp(this, new EventArgs());
        }

        /// <summary> Creates NmeaPositionMocker. </summary>
        /// <param name="stream">Nmea stream.</param>
        /// <param name="messageBus">MessageBus.</param>
        public NmeaPositionMocker(Stream stream, IMessageBus messageBus)
        {
            _parser = new NmeaParser(stream);
            _messageBus = messageBus;
        }

        /// <summary>
        ///     Starts processing of nmea file using passed delay action.
        ///     Send GeoPosition message to message bus.
        /// </summary>
        public void Start(Action<TimeSpan> delayAction)
        {
            _positionDateTime = null;
            _position = new GeoPosition();
            string line;
            _isStarted = true;
            while ((line = _parser.ReadLine()) != null)
            {
                if (!_isStarted) break;

                if (line.Length <= 0) continue;

                var message = _parser.ParseLine(line);
                if (message == null || !CanSetPositionFromMessage(message) || _position.Date.Year < 2001)
                    continue;

                DateTime currentDateTime = _position.DateTime;
                if (_positionDateTime != null)
                {
                    var sleepTime = currentDateTime - _positionDateTime.Value;
                    delayAction(sleepTime);
                    _messageBus.Send(_position);
                }
                _positionDateTime = currentDateTime;
            }
            _isStarted = false;
            FireDone();
        }

        /// <summary> Gets mocker state. </summary>
        public bool IsStarted { get { return _isStarted; }
        }

        /// <summary> Stops mocker.</summary>
        public void Stop()
        {
            _isStarted = false;
        }

        private bool CanSetPositionFromMessage(NmeaMessage message)
        {
            switch (message.Id)
            {
                case NmeaMessageType.GGA:
                    ParseCGA(message);
                    break;
                case NmeaMessageType.RMC:
                    ParseRMC(message);
                    break;
            }

            return message.Id == CycleStartMsgType;
        }

        private void ParseRMC(NmeaMessage message)
        {
            var f = (NmeaField) message.Fields[RMC.FieldIds.Status];
            double longitude = 0, latitude = 0;
            if ((f != null) && f.HasValue)
            {
                char status = f.GetChar((char) 0);
                if (status != 'A')
                    return;

                f = (NmeaField) message.Fields[RMC.FieldIds.X];
                if ((f != null) && f.HasValue)
                {
                    longitude = f.GetDouble(_position.Coordinate.Longitude);
                }
                f = (NmeaField) message.Fields[RMC.FieldIds.Y];
                if ((f != null) && f.HasValue)
                {
                    latitude = f.GetDouble(_position.Coordinate.Latitude);
                }
                f = (NmeaField) message.Fields[RMC.FieldIds.Utc];
                if ((f != null) && f.HasValue)
                {
                    _position.Time = f.GetTime(_position.Time);
                }
                f = (NmeaField) message.Fields[RMC.FieldIds.Date];
                if ((f != null) && f.HasValue)
                {
                    _position.Date = f.GetDate(_position.Date);
                }
                f = (NmeaField) message.Fields[RMC.FieldIds.Speed];
                if ((f != null) && f.HasValue)
                {
                    _position.Speed = f.GetDouble(_position.Speed);
                }
                f = (NmeaField) message.Fields[RMC.FieldIds.Course];
                if ((f != null) && f.HasValue)
                {
                    _position.Course = f.GetDouble(_position.Course);
                }
            }
            _position.Coordinate = new GeoCoordinate((float)latitude, (float)longitude);
        }

        private void ParseCGA(NmeaMessage message)
        {
            GeoPosition position = new GeoPosition();
            double longitude = 0, latitude = 0;
            var f = (NmeaField) message.Fields[GGA.FieldIds.PositionFixIndicator];
            if ((f != null) && f.HasValue)
            {
                position.PositionFixIndicator = f.GetInt(position.PositionFixIndicator);
            }
            f = (NmeaField) message.Fields[GGA.FieldIds.X];
            if ((f != null) && f.HasValue)
            {
                longitude = f.GetDouble(_position.Coordinate.Longitude);
            }
            f = (NmeaField) message.Fields[GGA.FieldIds.Y];
            if ((f != null) && f.HasValue)
            {
                latitude = f.GetDouble(_position.Coordinate.Latitude);
            }
            f = (NmeaField) message.Fields[GGA.FieldIds.Utc];
            if ((f != null) && f.HasValue)
            {
                position.Time = f.GetTime(position.Time);
            }
            f = (NmeaField) message.Fields[GGA.FieldIds.Satelites];
            if ((f != null) && f.HasValue)
            {
                position.Satelites = f.GetInt(position.Satelites);
            }
            f = (NmeaField) message.Fields[GGA.FieldIds.Hdop];
            if ((f != null) && f.HasValue)
            {
                position.Hdop = f.GetInt(position.Hdop);
            }
            _position.Coordinate = new GeoCoordinate((float)latitude, (float)longitude);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _parser.Dispose();
        }
    }
}