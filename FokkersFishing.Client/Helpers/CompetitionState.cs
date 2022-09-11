﻿using System;

namespace FokkersFishing.Client.Helpers
{
    public class CompetitionState
    {
        private string m_CompetitionName;
        private bool m_CompetitionActive;
        private DateTime m_StartDate;
        private DateTime m_EndDate;
        private Guid m_CompetitionId;

        public string CompetitionName
        {
            get
            {
                return m_CompetitionName;
            }
            set
            {
                m_CompetitionName = value;
                NotifyStateChanged();
            }
        }

        public Guid CompetitionId
        {
            get
            {
                return m_CompetitionId;
            }
            set
            {
                m_CompetitionId = value;
                NotifyStateChanged();
            }
        }

        public bool Active
        {
            get
            {
                return m_CompetitionActive;
            }
            set
            {
                m_CompetitionActive = value;
                NotifyStateChanged();
            }
        }


        public DateTime StartDate
        {
            get
            {
                return m_StartDate;
            }
            set
            {
                m_StartDate = value;
                NotifyStateChanged();
            }
        }

        public DateTime EndDate
        {
            get
            {
                return m_EndDate;
            }
            set
            {
                m_EndDate = value;
                NotifyStateChanged();
            }
        }
        public event Action OnChange;

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    } // end c
} // end ns
