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
	/// Implements the GpsMetadata profile message.
	/// </summary>
	public class GpsMetadataMesg : Mesg
    {
        #region Fields
        #endregion

        /// <summary>
        /// Field Numbers for <see cref="GpsMetadataMesg"/>
        /// </summary>
        public sealed class FieldDefNum
        {
            public const byte Timestamp = 253;
            public const byte TimestampMs = 0;
            public const byte PositionLat = 1;
            public const byte PositionLong = 2;
            public const byte EnhancedAltitude = 3;
            public const byte EnhancedSpeed = 4;
            public const byte Heading = 5;
            public const byte UtcTimestamp = 6;
            public const byte Velocity = 7;
            public const byte Invalid = Fit.FieldNumInvalid;
        }

        #region Constructors
        public GpsMetadataMesg() : base(Profile.GetMesg(MesgNum.GpsMetadata))
        {
        }

        public GpsMetadataMesg(Mesg mesg) : base(mesg)
        {
        }
        #endregion // Constructors

        #region Methods
        ///<summary>
        /// Retrieves the Timestamp field
        /// Units: s
        /// Comment: Whole second part of the timestamp.</summary>
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
        /// Units: s
        /// Comment: Whole second part of the timestamp.</summary>
        /// <param name="timestamp_">Nullable field value to be set</param>
        public void SetTimestamp(DateTime timestamp_)
        {
            SetFieldValue(253, 0, timestamp_.GetTimeStamp(), Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the TimestampMs field
        /// Units: ms
        /// Comment: Millisecond part of the timestamp.</summary>
        /// <returns>Returns nullable ushort representing the TimestampMs field</returns>
        public ushort? GetTimestampMs()
        {
            Object val = GetFieldValue(0, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt16(val));
            
        }

        /// <summary>
        /// Set TimestampMs field
        /// Units: ms
        /// Comment: Millisecond part of the timestamp.</summary>
        /// <param name="timestampMs_">Nullable field value to be set</param>
        public void SetTimestampMs(ushort? timestampMs_)
        {
            SetFieldValue(0, 0, timestampMs_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the PositionLat field
        /// Units: semicircles</summary>
        /// <returns>Returns nullable int representing the PositionLat field</returns>
        public int? GetPositionLat()
        {
            Object val = GetFieldValue(1, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToInt32(val));
            
        }

        /// <summary>
        /// Set PositionLat field
        /// Units: semicircles</summary>
        /// <param name="positionLat_">Nullable field value to be set</param>
        public void SetPositionLat(int? positionLat_)
        {
            SetFieldValue(1, 0, positionLat_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the PositionLong field
        /// Units: semicircles</summary>
        /// <returns>Returns nullable int representing the PositionLong field</returns>
        public int? GetPositionLong()
        {
            Object val = GetFieldValue(2, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToInt32(val));
            
        }

        /// <summary>
        /// Set PositionLong field
        /// Units: semicircles</summary>
        /// <param name="positionLong_">Nullable field value to be set</param>
        public void SetPositionLong(int? positionLong_)
        {
            SetFieldValue(2, 0, positionLong_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the EnhancedAltitude field
        /// Units: m</summary>
        /// <returns>Returns nullable float representing the EnhancedAltitude field</returns>
        public float? GetEnhancedAltitude()
        {
            Object val = GetFieldValue(3, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToSingle(val));
            
        }

        /// <summary>
        /// Set EnhancedAltitude field
        /// Units: m</summary>
        /// <param name="enhancedAltitude_">Nullable field value to be set</param>
        public void SetEnhancedAltitude(float? enhancedAltitude_)
        {
            SetFieldValue(3, 0, enhancedAltitude_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the EnhancedSpeed field
        /// Units: m/s</summary>
        /// <returns>Returns nullable float representing the EnhancedSpeed field</returns>
        public float? GetEnhancedSpeed()
        {
            Object val = GetFieldValue(4, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToSingle(val));
            
        }

        /// <summary>
        /// Set EnhancedSpeed field
        /// Units: m/s</summary>
        /// <param name="enhancedSpeed_">Nullable field value to be set</param>
        public void SetEnhancedSpeed(float? enhancedSpeed_)
        {
            SetFieldValue(4, 0, enhancedSpeed_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the Heading field
        /// Units: degrees</summary>
        /// <returns>Returns nullable float representing the Heading field</returns>
        public float? GetHeading()
        {
            Object val = GetFieldValue(5, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToSingle(val));
            
        }

        /// <summary>
        /// Set Heading field
        /// Units: degrees</summary>
        /// <param name="heading_">Nullable field value to be set</param>
        public void SetHeading(float? heading_)
        {
            SetFieldValue(5, 0, heading_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the UtcTimestamp field
        /// Units: s
        /// Comment: Used to correlate UTC to system time if the timestamp of the message is in system time. This UTC time is derived from the GPS data.</summary>
        /// <returns>Returns DateTime representing the UtcTimestamp field</returns>
        public DateTime GetUtcTimestamp()
        {
            Object val = GetFieldValue(6, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return TimestampToDateTime(Convert.ToUInt32(val));
            
        }

        /// <summary>
        /// Set UtcTimestamp field
        /// Units: s
        /// Comment: Used to correlate UTC to system time if the timestamp of the message is in system time. This UTC time is derived from the GPS data.</summary>
        /// <param name="utcTimestamp_">Nullable field value to be set</param>
        public void SetUtcTimestamp(DateTime utcTimestamp_)
        {
            SetFieldValue(6, 0, utcTimestamp_.GetTimeStamp(), Fit.SubfieldIndexMainField);
        }
        
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>returns number of elements in field Velocity</returns>
        public int GetNumVelocity()
        {
            return GetNumFieldValues(7, Fit.SubfieldIndexMainField);
        }

        ///<summary>
        /// Retrieves the Velocity field
        /// Units: m/s
        /// Comment: velocity[0] is lon velocity. Velocity[1] is lat velocity. Velocity[2] is altitude velocity.</summary>
        /// <param name="index">0 based index of Velocity element to retrieve</param>
        /// <returns>Returns nullable float representing the Velocity field</returns>
        public float? GetVelocity(int index)
        {
            Object val = GetFieldValue(7, index, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToSingle(val));
            
        }

        /// <summary>
        /// Set Velocity field
        /// Units: m/s
        /// Comment: velocity[0] is lon velocity. Velocity[1] is lat velocity. Velocity[2] is altitude velocity.</summary>
        /// <param name="index">0 based index of velocity</param>
        /// <param name="velocity_">Nullable field value to be set</param>
        public void SetVelocity(int index, float? velocity_)
        {
            SetFieldValue(7, index, velocity_, Fit.SubfieldIndexMainField);
        }
        
        #endregion // Methods
    } // Class
} // namespace
