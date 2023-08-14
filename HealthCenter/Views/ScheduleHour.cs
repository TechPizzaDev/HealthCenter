using System;
using NodaTime;

namespace HealthCenter.Views
{
    public class ScheduleHour
    {
        private bool _changed;
        private OffsetTime _Hour;
        private bool _Monday;
        private bool _Tuesday;
        private bool _Wednesday;
        private bool _Thursday;
        private bool _Friday;

        public LocalTime Hour => _Hour.TimeOfDay;
        public bool Monday { get => _Monday; set => Change(ref _Monday, value); }
        public bool Tuesday { get => _Tuesday; set => Change(ref _Tuesday, value); }
        public bool Wednesday { get => _Wednesday; set => Change(ref _Wednesday, value); }
        public bool Thursday { get => _Thursday; set => Change(ref _Thursday, value); }
        public bool Friday { get => _Friday; set => Change(ref _Friday, value); }

        public event Action<ScheduleHour>? Changed;

        public ScheduleHour(OffsetTime hour, bool monday, bool tuesday, bool wednesday, bool thursday, bool friday)
        {
            _Hour = hour;
            _Monday = monday;
            _Tuesday = tuesday;
            _Wednesday = wednesday;
            _Thursday = thursday;
            _Friday = friday;
        }

        public OffsetTime GetOffsetHour()
        {
            return _Hour;
        }

        public bool Save()
        {
            bool changed = _changed;
            _changed = false;
            return changed;
        }

        private void Change(ref bool field, bool value)
        {
            if (field != value)
            {
                field = value;
                _changed = true;
                Changed?.Invoke(this);
            }
        }
    }
}
