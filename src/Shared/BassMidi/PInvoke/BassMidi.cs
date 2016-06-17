using System;
using System.Runtime.InteropServices;

namespace ManagedBass.Midi
{
    /// <summary>
    /// Wraps BassMidi: bassmidi.dll
    /// 
    /// <para>Supports: .midi, .mid, .rmi, .kar</para>
    /// </summary>
    public static partial class BassMidi
    {
        const int BassMidiFontEx = 0x1000000;

        /// <summary>
        /// Chorus Mix Channel.
        /// </summary>
        public const int ChorusChannel = -1;

        /// <summary>
        /// Reverb Mix Channel.
        /// </summary>
        public const int ReverbChannel = -2;

        /// <summary>
        /// User FX Channel.
        /// </summary>
        public const int UserFXChannel = -3;

        /// <summary>
        /// Creates a sample stream to render real-time MIDI events.
        /// </summary>
        /// <param name="Channels">The number of MIDI channels: 1 (min) - 128 (max).</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Frequency">Sample rate (in Hz) to render/play the MIDI at (0 = the rate specified in the <see cref="Bass.Init" /> call; 1 = the device's current output rate or the <see cref="Bass.Init"/> rate if that is not available).</param>
        /// <returns>If successful, the new stream's handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// This function creates a stream solely for real-time MIDI events.
        /// As it's not based on any file, the stream has no predetermined length and is never-ending.
        /// Seeking isn't possible, but it is possible to reset everything, including playback buffer, by calling <see cref="Bass.ChannelPlay" /> (Restart = <see langword="true" />) or <see cref="Bass.ChannelSetPosition" /> (Position = 0).
        /// </para>
        /// <para>
        /// MIDI events are applied using the <see cref="StreamEvent(int,int,MidiEventType,int)" /> function.
        /// If the stream is being played (it's not a decoding channel), then there will be some delay in the effect of the events being heard. 
        /// This latency can be reduced by making use of the <see cref="Bass.PlaybackBufferLength" /> and <see cref="Bass.UpdatePeriod" /> options.
        /// </para>
        /// <para>
        /// If a stream has 16 MIDI channels, then channel 10 defaults to percussion/drums and the rest melodic, otherwise they are all melodic.
        /// That can be changed using <see cref="StreamEvent(int,int,MidiEventType,int)" /> and <see cref="MidiEventType.Drums"/>.
        /// </para>
        /// <para>
        /// Soundfonts provide the sounds that are used to render a MIDI stream.
        /// A default soundfont configuration is applied initially to the new MIDI stream, which can subsequently be overriden using <see cref="StreamSetFonts(int,MidiFont[],int)" />.
        /// </para>
        /// <para>To play a MIDI file, use <see cref="CreateStream(string,long,long,BassFlags,int)" />.</para>
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// On Android and iOS, sinc interpolation requires a NEON-supporting CPU; the <see cref="BassFlags.SincInterpolation"/> flag will otherwise be ignored.
        /// Sinc interpolation is not available on Windows CE.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Init"><see cref="Bass.Init" /> has not been successfully called.</exception>
        /// <exception cref="Errors.NotAvailable">Only decoding channels (<see cref="BassFlags.Decode"/>) are allowed when using the <see cref="Bass.NoSoundDevice"/>. The <see cref="BassFlags.AutoFree"/> flag is also unavailable to decoding channels.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Channels" /> is not valid.</exception>
        /// <exception cref="Errors.SampleFormat">The sample format is not supported by the device/drivers. If the stream is more than stereo or the <see cref="BassFlags.Float"/> flag is used, it could be that they are not supported.</exception>
        /// <exception cref="Errors.Speaker">The specified Speaker flags are invalid. The device/drivers do not support them, they are attempting to assign a stereo stream to a mono speaker or 3D functionality is enabled.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.No3D">Could not initialize 3D support.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamCreate")]
        public static extern int CreateStream(int Channels, BassFlags Flags = BassFlags.Default, int Frequency = 0);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamCreateEvents")]
        public static extern int CreateStream(MidiEvent[] events, int ppqn, BassFlags flags = BassFlags.Default, int freq = 0);

        /// <summary>
        /// Applies an event to a MIDI stream.
        /// </summary>
        /// <param name="Handle">The MIDI stream to apply the event to (as returned by <see cref="CreateStream(int,BassFlags,int)" />).</param>
        /// <param name="Channel">The MIDI channel to apply the event to... 0 = channel 1.</param>
        /// <param name="Event">The event to apply (see <see cref="MidiEventType" /> for details).</param>
        /// <param name="Parameter">The event parameter (see <see cref="MidiEventType" /> for details).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>Apart from the "global" events, all events apply only to the specified MIDI channel.</para>
        /// <para>
        /// Except for the "non-MIDI" events, events applied to a MIDI file stream can subsequently be overridden by events in the file itself, and will also be overridden when seeking or looping.
        /// That can be avoided by using additional channels, allocated via the <see cref="ChannelAttribute.MidiChannels"/> attribute.
        /// </para>
        /// <para>
        /// Event syncs (see <see cref="SyncFlags" />) are not triggered by this function.
        /// If sync triggering is wanted, <see cref="StreamEvents(int,MidiEventsMode,MidiEvent[],int)" /> can be used instead.
        /// </para>
        /// <para>
        /// If the MIDI stream is being played (it's not a decoding channel), then there will be some delay in the effect of the event being heard. 
        /// This latency can be reduced by making use of the <see cref="Bass.PlaybackBufferLength"/> and <see cref="Bass.UpdatePeriod"/> config options when creating the stream.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter">One of the other parameters is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamEvent")]
        public static extern bool StreamEvent(int Handle, int Channel, MidiEventType Event, int Parameter);

        /// <summary>
        /// Applies an event to a MIDI stream.
        /// </summary>
        /// <param name="Handle">The MIDI stream to apply the event to (as returned by <see cref="CreateStream(int,BassFlags,int)" />).</param>
        /// <param name="Channel">The MIDI channel to apply the event to... 0 = channel 1.</param>
        /// <param name="Event">The event to apply (see <see cref="MidiEventType" /> for details).</param>
        /// <param name="LowParameter">The event parameter (LOBYTE), (see <see cref="MidiEventType" /> for details).</param>
        /// <param name="HighParameter">The event parameter (HIBYTE), (see <see cref="MidiEventType" /> for details).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>Apart from the "global" events, all events apply only to the specified MIDI channel.</para>
        /// <para>
        /// Except for the "non-MIDI" events, events applied to a MIDI file stream can subsequently be overridden by events in the file itself, and will also be overridden when seeking or looping.
        /// That can be avoided by using additional channels, allocated via the <see cref="ChannelAttribute.MidiChannels"/> attribute.
        /// </para>
        /// <para>
        /// Event syncs (see <see cref="SyncFlags" />) are not triggered by this function.
        /// If sync triggering is wanted, <see cref="StreamEvents(int,MidiEventsMode,MidiEvent[],int)" /> can be used instead.
        /// </para>
        /// <para>
        /// If the MIDI stream is being played (it's not a decoding channel), then there will be some delay in the effect of the event being heard. 
        /// This latency can be reduced by making use of the <see cref="Bass.PlaybackBufferLength"/> and <see cref="Bass.UpdatePeriod"/> config options when creating the stream.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter">One of the other parameters is invalid.</exception>
        public static bool StreamEvent(int Handle, int Channel, MidiEventType Event, byte LowParameter, byte HighParameter)
        {
            return StreamEvent(Handle, Channel, Event, BitHelper.MakeLong(LowParameter, HighParameter));
        }

        #region StreamEvents
        [DllImport(DllName, EntryPoint= "BASS_MIDI_StreamEvents")]
        public static extern int StreamEvents(int Handle, MidiEventsMode Mode, IntPtr Events, int Length);

        [DllImport(DllName)]
        static extern int BASS_MIDI_StreamEvents(int Handle, MidiEventsMode Mode, MidiEvent[] Events, int Length);

        [DllImport(DllName)]
        static extern int BASS_MIDI_StreamEvents(int Handle, MidiEventsMode Mode, byte[] Events, int Length);

        public static int StreamEvents(int Handle, MidiEventsMode Mode, MidiEvent[] Events, int Length = 0)
        {
            return BASS_MIDI_StreamEvents(Handle, Mode & ~MidiEventsMode.Raw, Events, Length == 0 ? Events.Length : Length);
        }

        public static int StreamEvents(int Handle, MidiEventsMode Mode, byte[] Raw, int Length = 0)
        {
            return BASS_MIDI_StreamEvents(Handle, MidiEventsMode.Raw | Mode, Raw, Length == 0 ? Raw.Length : Length);
        }
        #endregion

        /// <summary>
        /// Gets a HSTREAM handle for a MIDI channel (e.g. to set DSP/FX on individual MIDI channels).
        /// </summary>
        /// <param name="Handle">The midi stream to get a channel from.</param>
        /// <param name="Channel">The MIDI channel... 0 = channel 1. Or one of the following special channels:
        /// <para><see cref="ChorusChannel"/> = Chorus mix channel. The default chorus processing is replaced by the stream's processing.</para>
        /// <para><see cref="ReverbChannel"/> = Reverb mix channel. The default reverb processing is replaced by the stream's processing.</para>
        /// <para><see cref="UserFXChannel"/> = User effect mix channel.</para>
        /// </param>
        /// <returns>If successful, the channel handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// By default, MIDI channels do not have streams assigned to them;
        /// a MIDI channel only gets a stream when this function is called, which it then keeps until the MIDI stream is freed. 
        /// MIDI channel streams can also be freed before then via <see cref="Bass.StreamFree" />.
        /// Each MIDI channel stream increases the CPU usage slightly, even if there are no DSP/FX set on them, so for optimal performance they should not be activated when unnecessary.
        /// </para>
        /// <para>
        /// The MIDI channel streams have a different path to the final mix than the BASSMIDI reverb/chorus processing, which means that the reverb/chorus will not be present in the data received by any DSP/FX set on the streams and nor will the reverb/chorus be applied to the DSP/FX output; 
        /// the reverb/chorus processing will use the channel's original data.
        /// </para>
        /// <para>
        /// The MIDI channel streams can only be used to set DSP/FX on the channels. 
        /// They cannot be used with <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> or <see cref="Bass.ChannelGetLevel(int)" /> to visualise the channels, for example, 
        /// but that could be achieved via a DSP function instead.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable"><paramref name="Channel" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetChannel")]
        public static extern int StreamGetChannel(int Handle, int Channel);

        /// <summary>
        /// Retrieves the current value of an event in a MIDI stream channel.
        /// </summary>
        /// <param name="Handle">The MIDI stream to retrieve the event from (as returned by <see cref="CreateStream(int,BassFlags,int)"/>.</param>
        /// <param name="Channel">The MIDI channel to get the event value from... 0 = channel 1.</param>
        /// <param name="Event">
        /// The event value to retrieve.
        /// With the drum key events (<see cref="MidiEventType.DrumCutOff"/>/etc) and the <see cref="MidiEventType.Note"/> and <see cref="MidiEventType.ScaleTuning"/> events, the HIWORD can be used to specify which key/note to get the value from.</param>
        /// <returns>The event parameter if successful - else -1 (use <see cref="Bass.LastError" /> to get the error code).</returns>
        /// <remarks>SYNCs can be used to be informed of when event values change.</remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Parameter">One of the other parameters is invalid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetEvent")]
        public static extern int StreamGetEvent(int Handle, int Channel, MidiEventType Event);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetEvents")]
        public static extern int StreamGetEvents(int handle, int track, int filter, [In, Out] MidiEvent[] events);

        public static MidiEvent[] StreamGetEvents(int handle, int track, int filter)
        {
            var count = StreamGetEvents(handle, track, filter, null);

            if (count <= 0)
                return null;

            var events = new MidiEvent[count];

            StreamGetEvents(handle, track, filter, events);

            return events;
        }

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetFonts")]
        public static extern int StreamGetFonts(int handle, IntPtr fonts, int count);
        
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetFonts")]
        public static extern int StreamGetFonts(int handle, [In][Out] MidiFont[] fonts, int count);

        [DllImport(DllName)]
        static extern int BASS_MIDI_StreamGetFonts(int handle, [In][Out] MidiFontEx[] fonts, int count);
        
        public static int StreamGetFonts(int handle, MidiFontEx[] fonts, int count)
        {
            return BASS_MIDI_StreamGetFonts(handle, fonts, count | BassMidiFontEx);
        }

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetMark")]
        public static extern bool StreamGetMark(int handle, MidiMarkerType type, int index, out MidiMarker mark);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamGetMarks")]
        public static extern int StreamGetMarks(int handle, int track, MidiMarkerType type, [In, Out] MidiMarker[] marks);

        public static MidiMarker[] StreamGetMarks(int handle)
        {
            var markCount = StreamGetMarks(handle, -1, MidiMarkerType.Marker, null);

            if (markCount <= 0)
                return null;

            var marks = new MidiMarker[markCount];
            StreamGetMarks(handle, -1, MidiMarkerType.Marker, marks);

            return marks;
        }
        
		/// <summary>
		/// Preloads the samples required by a MIDI file stream.
		/// </summary>
		/// <param name="Handle">The MIDI stream handle.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// Samples are normally loaded as they are needed while rendering a MIDI stream, which can result in CPU spikes, particularly with packed soundfonts.
        /// That generally won't cause any problems, but when smooth/constant performance is critical this function can be used to preload the samples before rendering, so avoiding the need to load them while rendering.
        /// </para>
		/// <para>Preloaded samples can be compacted/unloaded just like any other samples, so it is probably wise to disable the <see cref="Compact"/> option when preloading samples, to avoid any chance of the samples subsequently being automatically unloaded.</para>
		/// <para>This function should not be used while the MIDI stream is being rendered, as that could interrupt the rendering.</para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The stream is for real-time events only, so it's not possible to know what presets are going to be used. Use <see cref="FontLoad" /> instead.</exception>
        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamLoadSamples")]
        public static extern bool StreamLoadSamples(int Handle);

        [DllImport(DllName, EntryPoint = "BASS_MIDI_StreamSetFonts")]
        public static extern int StreamSetFonts(int handle, MidiFont[] fonts, int count);

        [DllImport(DllName)]
        static extern int BASS_MIDI_StreamSetFonts(int handle, MidiFontEx[] fonts, int count);

        public static int StreamSetFonts(int handle, MidiFontEx[] fonts, int count)
        {
            return BASS_MIDI_StreamSetFonts(handle, fonts, count | BassMidiFontEx);
        }
    }
}