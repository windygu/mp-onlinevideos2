﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Text;

namespace OnlineVideos.MPUrlSourceFilter.UserSettings
{
    /// <summary>
    /// Represents class for RTSP url settings.
    /// </summary>
    [TypeConverter(typeof(ExpandableUserSettingObjectConverter<RtspUrlSettings>))]
    [Serializable]
    public class RtspUrlSettings : SimpleUrlSettings
    {
        #region Private fields

        private int openConnectionTimeout = OnlineVideoSettings.Instance.RtspOpenConnectionTimeout;
        private int openConnectionSleepTime = OnlineVideoSettings.Instance.RtspOpenConnectionSleepTime;
        private int totalReopenConnectionTimeout = OnlineVideoSettings.Instance.RtspTotalReopenConnectionTimeout;
        private int clientPortMin = OnlineVideoSettings.Instance.RtspClientPortMin;
        private int clientPortMax = OnlineVideoSettings.Instance.RtspClientPortMax;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the timeout to open RTSP url in milliseconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="OpenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        [Category("OnlineVideosUserConfiguration"), Description("The timeout to open RTSP url in milliseconds. It is applied to first opening of url.")]
        [NotifyParentProperty(true)]
        public int OpenConnectionTimeout
        {
            get { return this.openConnectionTimeout; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("OpenConnectionTimeout", value, "Cannot be less than zero.");
                }

                this.openConnectionTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the time in milliseconds to sleep before opening connection.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="OpenConnectionSleepTime"/> is lower than zero.</para>
        /// </exception>
        [Category("OnlineVideosUserConfiguration"), Description("The time in milliseconds to sleep before opening connection.")]
        [NotifyParentProperty(true)]
        public int OpenConnectionSleepTime
        {
            get { return this.openConnectionSleepTime; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("OpenConnectionSleepTime", value, "Cannot be less than zero.");
                }

                this.openConnectionSleepTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the total timeout to open RTSP url in milliseconds.
        /// </summary>
        /// <remarks>
        /// <para>It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="TotalReopenConnectionTimeout"/> is lower than zero.</para>
        /// </exception>
        [Category("OnlineVideosUserConfiguration"), Description("The total timeout to open RTSP url in milliseconds. It is applied when lost connection and trying to open new one. Filter will be trying to open connection until this timeout occurs. This parameter is ignored in case of live stream.")]
        [NotifyParentProperty(true)]
        public int TotalReopenConnectionTimeout
        {
            get { return this.totalReopenConnectionTimeout; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("TotalReopenConnectionTimeout", value, "Cannot be less than zero.");
                }

                this.totalReopenConnectionTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum client port for UDP transport.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ClientPortMin"/> is lower than zero.</para>
        /// <para>- or -</para>
        /// <para>The <see cref="ClientPortMin"/> is greater than 65535.</para>
        /// </exception>
        [Category("OnlineVideosUserConfiguration"), Description("The minimum client port for UDP transport.")]
        [NotifyParentProperty(true)]
        public int ClientPortMin
        {
            get { return this.clientPortMin; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMin", value, "Cannot be less than zero.");
                }

                if (value > 65535)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMin", value, "Cannot be greater than 65535.");
                }

                this.clientPortMin = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum client port for UDP transport.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>The <see cref="ClientPortMax"/> is lower than zero.</para>
        /// <para>- or -</para>
        /// <para>The <see cref="ClientPortMax"/> is greater than 65535.</para>
        /// </exception>
        [Category("OnlineVideosUserConfiguration"), Description("The maximum client port for UDP transport.")]
        [NotifyParentProperty(true)]
        public int ClientPortMax
        {
            get { return this.clientPortMax; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMax", value, "Cannot be less than zero.");
                }

                if (value > 65535)
                {
                    throw new ArgumentOutOfRangeException("ClientPortMax", value, "Cannot be greater than 65535.");
                }

                this.clientPortMax = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RtspUrlSettings" /> class.
        /// </summary>
        public RtspUrlSettings()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RtspUrlSettings" /> class with specified RTSP url parameters.
        /// </summary>
        /// <param name="value">RTSP url parameters.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <paramref name="value"/> is <see langword="null"/>.</para>
        /// </exception>
        public RtspUrlSettings(String value)
            : base(value)
        {
            var parameters = GetParameters(value);

            var ovSettings = OnlineVideoSettings.Instance;

            this.OpenConnectionTimeout = GetValue(parameters, "OpenConnectionTimeout", ovSettings.RtspOpenConnectionTimeout);
            this.OpenConnectionSleepTime = GetValue(parameters, "OpenConnectionSleepTime", ovSettings.RtspOpenConnectionSleepTime);
            this.TotalReopenConnectionTimeout = GetValue(parameters, "TotalReopenConnectionTimeout", ovSettings.RtspTotalReopenConnectionTimeout);
            this.ClientPortMin = GetValue(parameters, "ClientPortMin", ovSettings.RtspClientPortMin);
            this.ClientPortMax = GetValue(parameters, "ClientPortMax", ovSettings.RtspClientPortMax);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets canonical string representation for the specified instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> instance that contains the unescaped canonical representation of the this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(base.ToString());
            builder.Append((this.OpenConnectionTimeout != OnlineVideoSettings.Instance.RtspOpenConnectionTimeout) ? String.Format("OpenConnectionTimeout={0};", this.OpenConnectionTimeout) : String.Empty);
            builder.Append((this.OpenConnectionSleepTime != OnlineVideoSettings.Instance.RtspOpenConnectionSleepTime) ? String.Format("OpenConnectionSleepTime={0};", this.OpenConnectionSleepTime) : String.Empty);
            builder.Append((this.TotalReopenConnectionTimeout != OnlineVideoSettings.Instance.RtspTotalReopenConnectionTimeout) ? String.Format("TotalReopenConnectionTimeout={0};", this.TotalReopenConnectionTimeout) : String.Empty);
            builder.Append((this.ClientPortMin != OnlineVideoSettings.Instance.RtspClientPortMin) ? String.Format("ClientPortMin={0};", this.ClientPortMin) : String.Empty);
            builder.Append((this.ClientPortMax != OnlineVideoSettings.Instance.RtspClientPortMax) ? String.Format("ClientPortMax={0};", this.ClientPortMax) : String.Empty);

            return builder.ToString();
        }

        internal void Apply(RtspUrl rtspUrl)
        {
            base.Apply(rtspUrl);

            rtspUrl.OpenConnectionSleepTime = OpenConnectionSleepTime;
            rtspUrl.OpenConnectionTimeout = OpenConnectionTimeout;
            rtspUrl.TotalReopenConnectionTimeout = TotalReopenConnectionTimeout;
            rtspUrl.ClientPortMax = ClientPortMin;
            rtspUrl.ClientPortMin = ClientPortMax;
        }

        #endregion
    }

}
