using System;
using System.Collections;
using NodaTime;

namespace HealthCenter
{
    public delegate bool ScheduleHourChanged(ScheduleHour instance, bool newValue);

    public class ScheduleHour
    {
        private bool _changed;
        private OffsetTime _Hour;
        private BitArray _Days;

        public LocalTime Hour => _Hour.TimeOfDay;
        public bool Monday { get => _Days[0]; set => ChangeDay(0, value); }
        public bool Tuesday { get => _Days[1]; set => ChangeDay(1, value); }
        public bool Wednesday { get => _Days[2]; set => ChangeDay(2, value); }
        public bool Thursday { get => _Days[3]; set => ChangeDay(3, value); }
        public bool Friday { get => _Days[4]; set => ChangeDay(4, value); }

        public ScheduleHourChanged? Changed;

        public ScheduleHour(OffsetTime hour, BitArray days)
        {
            _Hour = hour;
            _Days = days;
        }

        public OffsetTime GetOffsetHour()
        {
            return _Hour;
        }

        public BitArray GetDays()
        {
            return _Days;
        }

        public bool Save()
        {
            bool changed = _changed;
            _changed = false;
            return changed;
        }

        private void ChangeDay(int index, bool value)
        {
            if (_Days[index] != value)
            {
                if (Changed == null || Changed.Invoke(this, value))
                {
                    _Days[index] = value;
                    _changed = true;
                }
            }
        }
    }
}
