using System;

namespace Lab5Games.Schedules
{
    public class TimeSchedule : Schedule, IAwaiter<ScheduleReport>, IAwaitable<TimeSchedule, ScheduleReport>
    {
        public static TimeSchedule Create(float secondsTimeout, bool autoStart = true)
        {
            TimeSchedule schedule = new TimeSchedule(secondsTimeout);

            if (autoStart)
                schedule.Start();

            return schedule;
        }

        float _secondsTimeout;
        public float RemainingTime => _secondsTimeout;

        public TimeSchedule(float secondsTimeout)
        {
            _secondsTimeout = secondsTimeout;
        }

        protected override void OnUpdate(float deltaTime)
        {
            _secondsTimeout -= deltaTime;

            if(_secondsTimeout <= 0)
            {
                State = States.Completed;
            }
        }

        public bool IsCompleted => State == States.Completed || State == States.Canceled;

        public TimeSchedule GetAwaiter() => this;

        public ScheduleReport GetResult() => new ScheduleReport() { state = this.State };

        public void OnCompleted(Action continuation)
        {
            onCancel += x => continuation();
            onComplete += x => continuation();
        }
    }
}