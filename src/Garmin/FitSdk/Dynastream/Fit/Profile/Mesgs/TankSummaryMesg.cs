#region Copyright
/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2023 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////
// ****WARNING****  This file is auto-generated!  Do NOT edit this file.
// Profile Version = 21.105Release
// Tag = production/release/21.105.00-0-gdc65d24
/////////////////////////////////////////////////////////////////////////////////////////////

#endregion

using System;

namespace Dynastream.Fit
{
	/// <summary>
	/// Implements the TankSummary profile message.
	/// </summary>
	public class TankSummaryMesg : Mesg
    {
        #region Fields
        #endregion

        /// <summary>
        /// Field Numbers for <see cref="TankSummaryMesg"/>
        /// </summary>
        public sealed class FieldDefNum
        {
            public const byte Timestamp = 253;
            public const byte Sensor = 0;
            public const byte StartPressure = 1;
            public const byte EndPressure = 2;
            public const byte VolumeUsed = 3;
            public const byte Invalid = Fit.FieldNumInvalid;
        }

        #region Constructors
        public TankSummaryMesg() : base(Profile.GetMesg(MesgNum.TankSummary))
        {
        }

        public TankSummaryMesg(Mesg mesg) : base(mesg)
        {
        }
        #endregion // Constructors

        #region Methods
        ///<summary>
        /// Retrieves the Timestamp field
        /// Units: s</summary>
        /// <returns>Returns DateTime representing the Timestamp field</returns>
        public DateTime GetTimestamp()
        {
            Object val = GetFieldValue(253, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return TimestampToDateTime(Convert.ToUInt32(val));
            
        }

        /// <summary>
        /// Set Timestamp field
        /// Units: s</summary>
        /// <param name="timestamp_">Nullable field value to be set</param>
        public void SetTimestamp(DateTime timestamp_)
        {
            SetFieldValue(253, 0, timestamp_.GetTimeStamp(), Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the Sensor field</summary>
        /// <returns>Returns nullable uint representing the Sensor field</returns>
        public uint? GetSensor()
        {
            Object val = GetFieldValue(0, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt32(val));
            
        }

        /// <summary>
        /// Set Sensor field</summary>
        /// <param name="sensor_">Nullable field value to be set</param>
        public void SetSensor(uint? sensor_)
        {
            SetFieldValue(0, 0, sensor_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the StartPressure field
        /// Units: bar</summary>
        /// <returns>Returns nullable float representing the StartPressure field</returns>
        public float? GetStartPressure()
        {
            Object val = GetFieldValue(1, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToSingle(val));
            
        }

        /// <summary>
        /// Set StartPressure field
        /// Units: bar</summary>
        /// <param name="startPressure_">Nullable field value to be set</param>
        public void SetStartPressure(float? startPressure_)
        {
            SetFieldValue(1, 0, startPressure_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the EndPressure field
        /// Units: bar</summary>
        /// <returns>Returns nullable float representing the EndPressure field</returns>
        public float? GetEndPressure()
        {
            Object val = GetFieldValue(2, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToSingle(val));
            
        }

        /// <summary>
        /// Set EndPressure field
        /// Units: bar</summary>
        /// <param name="endPressure_">Nullable field value to be set</param>
        public void SetEndPressure(float? endPressure_)
        {
            SetFieldValue(2, 0, endPressure_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the VolumeUsed field
        /// Units: L</summary>
        /// <returns>Returns nullable float representing the VolumeUsed field</returns>
        public float? GetVolumeUsed()
        {
            Object val = GetFieldValue(3, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToSingle(val));
            
        }

        /// <summary>
        /// Set VolumeUsed field
        /// Units: L</summary>
        /// <param name="volumeUsed_">Nullable field value to be set</param>
        public void SetVolumeUsed(float? volumeUsed_)
        {
            SetFieldValue(3, 0, volumeUsed_, Fit.SubfieldIndexMainField);
        }
        
        #endregion // Methods
    } // Class
} // namespace
