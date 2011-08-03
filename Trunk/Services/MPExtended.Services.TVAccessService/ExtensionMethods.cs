﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvControl;
using TvDatabase;
using TvLibrary.Streaming;
using MPExtended.Services.TVAccessService.Interfaces;

namespace MPExtended.Services.TVAccessService
{
    public static class WebCardExtensionMethods
    {
        public static WebCard ToWebCard(this Card card)
        {
            if (card == null)
                return null;

            return new WebCard
            {
                CAM = card.CAM,
                CamType = card.CamType,
                DecryptLimit = card.DecryptLimit,
                DevicePath = card.DevicePath,
                Enabled = card.Enabled,
                GrabEPG = card.GrabEPG,
                IdCard = card.IdCard,
                IdServer = card.IdServer,
                IsChanged = card.IsChanged,
                LastEpgGrab = card.LastEpgGrab != DateTime.MinValue ? card.LastEpgGrab : new DateTime(2000, 1, 1),
                Name = card.Name,
                netProvider = card.netProvider,
                PreloadCard = card.PreloadCard,
                Priority = card.Priority,
                RecordingFolder = card.RecordingFolder,
                RecordingFormat = card.RecordingFormat,
                supportSubChannels = card.supportSubChannels,
                TimeShiftFolder = card.TimeShiftFolder
            };
        }

        public static Card ToCard(this WebCard card)
        {
            if (card == null)
                return null;

            return Card.Retrieve(card.IdCard);
        }
    }

    public static class WebVirtualCardExtensionMethods
    {
        public static WebVirtualCard ToWebVirtualCard(this VirtualCard card)
        {
            if (card == null)
                return null;

            return new WebVirtualCard
            {
                BitRateMode = (int)card.BitRateMode,
                ChannelName = card.ChannelName,
                Device = card.Device,
                Enabled = card.Enabled,
                GetTimeshiftStoppedReason = (int)card.GetTimeshiftStoppedReason,
                GrabTeletext = card.GrabTeletext,
                HasTeletext = card.HasTeletext,
                Id = card.Id,
                IdChannel = card.IdChannel,
                IsGrabbingEpg = card.IsGrabbingEpg,
                IsRecording = card.IsRecording,
                IsScanning = card.IsScanning,
                IsScrambled = card.IsScrambled,
                IsTimeShifting = card.IsTimeShifting,
                IsTunerLocked = card.IsTunerLocked,
                MaxChannel = card.MaxChannel,
                MinChannel = card.MinChannel,
                Name = card.Name,
                QualityType = (int)card.QualityType,
                RecordingFileName = card.RecordingFileName,
                RecordingFolder = card.RecordingFolder,
                RecordingFormat = card.RecordingFormat,
                RecordingScheduleId = card.RecordingScheduleId,
                RecordingStarted = card.RecordingStarted != DateTime.MinValue ? card.RecordingStarted : new DateTime(2000, 1, 1),
                RemoteServer = card.RemoteServer,
                RTSPUrl = card.RTSPUrl,
                SignalLevel = card.SignalLevel,
                SignalQuality = card.SignalQuality,
                TimeShiftFileName = card.TimeShiftFileName,
                TimeshiftFolder = card.TimeshiftFolder,
                TimeShiftStarted = card.TimeShiftStarted != DateTime.MinValue ? card.TimeShiftStarted : new DateTime(2000, 1, 1),
                Type = (int)card.Type,
                User = card.User != null ? card.User.ToWebUser() : null
            };
        }
    }

    public static class WebUserExtensionMethods
    {
        public static WebUser ToWebUser(this User user)
        {
            if (user == null)
                return null;

            return new WebUser
            {
                CardId = user.CardId,
                HeartBeat = user.HeartBeat != DateTime.MinValue ? user.HeartBeat : new DateTime(2000, 1, 1),
                IdChannel = user.IdChannel,
                IsAdmin = user.IsAdmin,
                Name = user.Name,
                SubChannel = user.SubChannel,
                TvStoppedReason = (int)user.TvStoppedReason
            };
        }
    }

    public static class WebChannelExtensionMethods
    {
        public static WebChannelDetailed ToWebChannelDetailed(this Channel ch)
        {
            if (ch == null)
                return null;

            return new WebChannelDetailed
            {
                CurrentProgram = ch.CurrentProgram.ToWebProgramDetailed(),
                DisplayName = ch.DisplayName,
                EpgHasGaps = ch.EpgHasGaps,
                ExternalId = ch.ExternalId,
                FreeToAir = GetFreeToAirInformation(ch),
                GrabEpg = ch.GrabEpg,
                GroupNames = ch.GroupNames,
                IdChannel = ch.IdChannel,
                IsChanged = ch.IsChanged,
                IsRadio = ch.IsRadio,
                IsTv = ch.IsTv,
                LastGrabTime = ch.LastGrabTime != DateTime.MinValue ? ch.LastGrabTime : new DateTime(2000, 1, 1),
                NextProgram = ch.NextProgram.ToWebProgramDetailed(),
                SortOrder = ch.SortOrder,
                TimesWatched = ch.TimesWatched,
                TotalTimeWatched = ch.TotalTimeWatched != DateTime.MinValue ? ch.TotalTimeWatched : new DateTime(2000, 1, 1),
                VisibleInGuide = ch.VisibleInGuide
            };
        }

        /// <summary>
        /// Checks if the channel is available on free to air or scrambled
        /// </summary>
        /// <param name="ch">The channel to get the free to air information for.</param>
        /// <returns>0 if the channel is available only scrambled, 1 if it is only available free to air, 2 if it is available scrambled and free to air.</returns>
        private static int GetFreeToAirInformation(Channel ch)
        {
            IList<TuningDetail> tuningDetails = ch.ReferringTuningDetail();

            // channel is available on only one frequency, use the FreeToAir information from this frequency
            if (tuningDetails.Count == 1)
                return tuningDetails.First().FreeToAir ? 1 : 0;


            // if the channel is available on more than one frequency, check if it is available only free to air, only scrambled or both
            bool hasFta = tuningDetails.Any(i => i.FreeToAir);
            bool hasScrambled = tuningDetails.Any(i => !i.FreeToAir);

            // has both scrambled and free to air frequencies
            if (hasFta && hasScrambled)
                return 2;

            // has only scrambled frequency
            if (hasScrambled)
                return 0;

            // has only free to air frequency
            return 1;
        }

        public static WebChannelBasic ToWebChannelBasic(this Channel ch)
        {
            if (ch == null)
                return null;

            return new WebChannelBasic
            {
                DisplayName = ch.DisplayName,
                IdChannel = ch.IdChannel
            };
        }
        public static List<WebProgramBasic> ToListWebProgramBasicNowNext(this Channel ch)
        {
            if (ch == null)
                return null;

            List<WebProgramBasic> tmp = new List<WebProgramBasic>();
            tmp.Add(ch.CurrentProgram.ToWebProgramBasic());
            tmp.Add(ch.NextProgram.ToWebProgramBasic());
            return tmp;

        }
        public static List<WebProgramDetailed> ToListWebProgramDetailedNowNext(this Channel ch)
        {
            if (ch == null)
                return null;

            List<WebProgramDetailed> tmp = new List<WebProgramDetailed>();
            tmp.Add(ch.CurrentProgram.ToWebProgramDetailed());
            tmp.Add(ch.NextProgram.ToWebProgramDetailed());
            return tmp;

        }

        public static Channel ToChannel(this WebChannelBasic ch)
        {
            if (ch == null)
                return null;

            return Channel.Retrieve(ch.IdChannel);
        }
    }

    public static class WebChannelGroupExtensionMethods
    {
        public static WebChannelGroup ToWebChannelGroup(this ChannelGroup group)
        {
            if (group == null)
                return null;

            return new WebChannelGroup
            {
                GroupName = group.GroupName,
                IdGroup = group.IdGroup,
                IsChanged = group.IsChanged,
                SortOrder = group.SortOrder
            };
        }

        public static ChannelGroup ToChannelGroup(this WebChannelGroup group)
        {
            if (group == null)
                return null;

            return ChannelGroup.Retrieve(group.IdGroup);
        }
    }

    public static class WebProgramExtensionMethods
    {
        public static WebProgramDetailed ToWebProgramDetailed(this Program p)
        {
            if (p == null)
                return null;

            return new WebProgramDetailed
            {
                Classification = p.Classification,
                Description = p.Description,
                EndTime = p.EndTime != DateTime.MinValue ? p.EndTime : new DateTime(2000, 1, 1),
                EpisodeName = p.EpisodeName,
                EpisodeNum = p.EpisodeNum,
                EpisodeNumber = p.EpisodeNumber,
                EpisodePart = p.EpisodePart,
                Genre = p.Genre,
                HasConflict = p.HasConflict,
                IdChannel = p.IdChannel,
                IdProgram = p.IdProgram,
                IsChanged = p.IsChanged,
                IsPartialRecordingSeriesPending = p.IsPartialRecordingSeriesPending,
                IsRecording = p.IsRecording,
                IsRecordingManual = p.IsRecordingManual,
                IsRecordingOnce = p.IsRecordingOnce,
                IsRecordingOncePending = p.IsRecordingOncePending,
                IsRecordingSeries = p.IsRecordingSeries,
                IsRecordingSeriesPending = p.IsRecordingSeriesPending,
                Notify = p.Notify,
                OriginalAirDate = p.OriginalAirDate != DateTime.MinValue ? p.OriginalAirDate : new DateTime(2000, 1, 1),
                ParentalRating = p.ParentalRating,
                SeriesNum = p.SeriesNum,
                StarRating = p.StarRating,
                StartTime = p.StartTime != DateTime.MinValue ? p.StartTime : new DateTime(2000, 1, 1),
                Title = p.Title,
                DurationInMinutes = (p.EndTime - p.StartTime).Minutes,
                IsScheduled = Schedule.ListAll().Where(schedule => schedule.IsRecordingProgram(p, true)).Count() > 0
            };
        }

        public static WebProgramBasic ToWebProgramBasic(this Program p)
        {
            if (p == null)
                return null;

            return new WebProgramBasic
            {
                Description = p.Description,
                EndTime = p.EndTime != DateTime.MinValue ? p.EndTime : new DateTime(2000, 1, 1),
                IdChannel = p.IdChannel,
                IdProgram = p.IdProgram,
                StartTime = p.StartTime != DateTime.MinValue ? p.StartTime : new DateTime(2000, 1, 1),
                Title = p.Title,
                DurationInMinutes = (p.EndTime - p.StartTime).Minutes,
                IsScheduled = Schedule.ListAll().Where(schedule => schedule.IdChannel == p.IdChannel && schedule.IsRecordingProgram(p, true)).Count() > 0
            };
        }

        public static Program ToProgram(this WebProgramBasic p)
        {
            if (p == null)
                return null;

            return Program.Retrieve(p.IdProgram);
        }
    }

    public static class WebRtspClientExtensionMethods
    {
        public static WebRtspClient ToWebRtspClient(this RtspClient rtsp)
        {
            if (rtsp == null)
                return null;

            return new WebRtspClient
            {
                DateTimeStarted = rtsp.DateTimeStarted != DateTime.MinValue ? rtsp.DateTimeStarted : new DateTime(2000, 1, 1),
                Description = rtsp.Description,
                IpAdress = rtsp.IpAdress,
                IsActive = rtsp.IsActive,
                StreamName = rtsp.StreamName
            };
        }
    }

    public static class WebScheduleExtensionMethods
    {
        public static WebSchedule ToWebSchedule(this Schedule sch)
        {
            if (sch == null)
                return null;

            return new WebSchedule
            {
                BitRateMode = (int)sch.BitRateMode,
                Canceled = sch.Canceled != DateTime.MinValue ? sch.Canceled : new DateTime(2000, 1, 1),
                Directory = sch.Directory,
                DoesUseEpisodeManagement = sch.DoesUseEpisodeManagement,
                EndTime = sch.EndTime != DateTime.MinValue ? sch.EndTime : new DateTime(2000, 1, 1),
                IdChannel = sch.IdChannel,
                IdParentSchedule = sch.IdParentSchedule,
                IdSchedule = sch.IdSchedule,
                IsChanged = sch.IsChanged,
                IsManual = sch.IsManual,
                KeepDate = sch.KeepDate != DateTime.MinValue ? sch.KeepDate : new DateTime(2000, 1, 1),
                KeepMethod = sch.KeepMethod,
                MaxAirings = sch.MaxAirings,
                PostRecordInterval = sch.PostRecordInterval,
                PreRecordInterval = sch.PreRecordInterval,
                Priority = sch.Priority,
                ProgramName = sch.ProgramName,
                Quality = sch.Quality,
                QualityType = (int)sch.QualityType,
                RecommendedCard = sch.RecommendedCard,
                ScheduleType = sch.ScheduleType,
                Series = sch.Series,
                StartTime = sch.StartTime != DateTime.MinValue ? sch.StartTime : new DateTime(2000, 1, 1)
            };
        }

        public static Schedule ToSchedule(this WebSchedule sch)
        {
            if (sch == null)
                return null;

            return Schedule.Retrieve(sch.IdSchedule);
        }
    }

    public static class WebRecordingExtensionMethods
    {
        public static WebRecording ToWebRecording(this Recording rec)
        {
            if (rec == null)
                return null;

            return new WebRecording
            {
                Description = rec.Description,
                EndTime = rec.EndTime != DateTime.MinValue ? rec.EndTime : new DateTime(2000, 1, 1),
                EpisodeName = rec.EpisodeName,
                EpisodeNum = rec.EpisodeNum,
                EpisodeNumber = rec.EpisodeNumber,
                EpisodePart = rec.EpisodePart,
                FileName = rec.FileName,
                Genre = rec.Genre,
                IdChannel = rec.IdChannel,
                IdRecording = rec.IdRecording,
                Idschedule = rec.Idschedule,
                IdServer = rec.IdServer,
                IsChanged = rec.IsChanged,
                IsManual = rec.IsManual,
                IsRecording = rec.IsRecording,
                KeepUntil = rec.KeepUntil,
                KeepUntilDate = rec.KeepUntilDate != DateTime.MinValue ? rec.KeepUntilDate : new DateTime(2000, 1, 1),
                SeriesNum = rec.SeriesNum,
                ShouldBeDeleted = rec.ShouldBeDeleted,
                StartTime = rec.StartTime != DateTime.MinValue ? rec.StartTime : new DateTime(2000, 1, 1),
                StopTime = rec.StopTime,
                TimesWatched = rec.TimesWatched,
                Title = rec.Title
            };
        }

        public static Recording ToRecording(this WebRecording rec)
        {
            if (rec == null)
                return null;

            return Recording.Retrieve(rec.IdRecording);
        }
    }
   
}