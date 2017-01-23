using System.Collections.Generic;

namespace VxEventAgent
{
    /// <summary>
    /// Used to specify a resource type when linking resources to a Situation
    /// </summary>
    public enum ResourceType
    {
        DataSource,
        Device,
        Drawing,
        User
    }

    public interface INewSituation
    {
        /// <summary>
        /// If true, generated events shall have an initial ack_state of ack_needed.
        /// If false, generated events shall have an initial ack_state of no_ack_needed. Required.
        /// </summary>
        bool AckNeeded { get; }

        /// <summary>
        /// True specifies that a notification sound is to play on supporting clients
        /// when receiving a notification for Events corresponding to the Situation. Required.
        /// </summary>
        bool Audible { get; }

        /// <summary>
        /// Number of seconds after which a generated event ack_state will be set to
        /// auto_acked. A value of 0 does not auto acknowledge the event. Required.
        /// </summary>
        uint AutoAcknowledgeTimeout { get; }

        /// <summary>
        /// If true, events generated from this Situation shall be persisted as long
        /// as possible. If false, generated events shall immediately be discarded;
        /// unlogged events are hidden from clients (this supersedes all other situation
        /// configuration). Required.
        /// </summary>
        bool Log { get; }

        /// <summary>
        /// If true, an Event generated from the Situation shall generate notifications
        /// that are sent to authorized clients. Additionally, these notifications will
        /// be sent out whenever generated events have a change of ack_state. Required.
        /// </summary>
        bool Notify { get; }

        /// <summary>
        /// Severity of the generated Event, from 1 (highest) to 10 (lowest). Required.
        /// </summary>
        uint Severity { get; }

        /// <summary>
        /// List of default snooze intervals, in minutes, for a generated Event. Note
        /// that these are default options and that they do not limit the amount of time
        /// a generated Event may be snoozed for. Optional.
        /// </summary>
        List<uint> SnoozeIntervals { get; }

        /// <summary>
        /// MUST be of the form external/<company>/<event> where <company> and <event> are UTF-8
        /// strings no greater than 64 characters each; forward slashes are not allowed.
        /// These strings describe the <company> that manufactured the device that was the
        /// source of the <event>. Required.
        /// e.g. external/onguard/swipe
        /// </summary>
        string Type { get; }

        /// <summary>
        /// This is the unique source device id that is associated with the event generating device. Optional.
        /// e.g. bldg_3_reader_2
        /// </summary>
        string SourceDeviceId { get; }

        /// <summary>
        /// This returns a list of VideoXpert resources that will be associated with this situation.
        /// A resource specifies the ResourceType, and the VideoXpert id assocaited with that device. Optional.
        /// e.g. ResourceType::DataSource / 0f4f412e-b1c8-44f1-9593-142fb4cc310d
        /// </summary>
        List<KeyValuePair<ResourceType, string>> Resources { get; }
    }
}
