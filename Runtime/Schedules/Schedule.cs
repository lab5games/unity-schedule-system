using System.Collections.Generic;
using UnityEngine;

namespace Lab5Games.Schedules
{
    public abstract class Schedule
    {
        public enum States
        {
            NotStarted,
            Running,
            Completed,
            Canceled
        }

        public States State { get; internal set; } = States.NotStarted;

        public bool Paused { get; protected set; } = false;

        public event ScheduleCallback onComplete;
        public event ScheduleCallback onCancel;
        

        public void Start()
        {
            if(State != States.NotStarted)
            {
                throw new System.Exception("Failed to start the schedule, state must be 'NotStarted'");
            }

            if(ScheduleSystem.AddSchedule(this))
            {
                State = States.Running;
                
                OnStart();
            }
        }

        public void Cancel()
        {
            if (State == States.Running)
            {
                State = States.Canceled;
                
                OnCancel();
            }
        }

        public virtual void Pause()
        {
            Paused = true;
        }

        public virtual void Unpause()
        {
            Paused = false;
        }

        public void Update(float deltaTime)
        {
            OnUpdate(deltaTime);
        }

        protected virtual void OnStart() { }
        protected virtual void OnCancel() { }
        protected virtual void OnUpdate(float deltaTime) { }

        internal void InvokeCompleteCallback_Internal() => onComplete?.Invoke(this);
        internal void InvokeCancelCallback_Internal() => onCancel?.Invoke(this);
    }


    public sealed class ScheduleSystem : MonoBehaviour
    {
        private static ScheduleSystem _instance = null;

        internal static ScheduleSystem system
        {
            get
            {
                if(_instance == null)
                {
                    GameObject go = new GameObject("[ScheduleSystem]");
                    _instance = go.AddComponent<ScheduleSystem>();

                    Debug.LogWarning("[ScheduleSystem] The system has been created automatically");
                }

                return _instance;
            }
        }

        internal static bool AddSchedule(Schedule schedule) => system.AddScheduel_Internal(schedule);
        public static void CancelAll() => system.CancelAll_Internal();
        


        List<Schedule> _scheduleList = new List<Schedule>();

        private void CancelAll_Internal()
        {
            foreach(var schedule in _scheduleList)
            {
                schedule.Cancel();
                schedule.InvokeCancelCallback_Internal();
            }

            _scheduleList.Clear();

            Debug.Log("[ScheduleSystem] Cancel all");
        }

        private bool AddScheduel_Internal(Schedule schedule)
        {
            _scheduleList.Add(schedule);

            return true;
        }

        #region Unity Calls
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void FixedUpdate()
        {
            float dt = Time.deltaTime;

            for(int i=_scheduleList.Count-1; i>=0; i--)
            {
                Schedule schedule = _scheduleList[i];

                if (schedule.Paused)
                    continue;

                switch(schedule.State)
                {
                    case Schedule.States.Running:
                        schedule.Update(dt);
                        break;

                    case Schedule.States.Completed:
                        _scheduleList.RemoveAt(i);
                        schedule.InvokeCompleteCallback_Internal();
                        break;

                    case Schedule.States.Canceled:
                        _scheduleList.RemoveAt(i);
                        schedule.InvokeCancelCallback_Internal();
                        break;
                }
            }
        }
        #endregion
    }
}