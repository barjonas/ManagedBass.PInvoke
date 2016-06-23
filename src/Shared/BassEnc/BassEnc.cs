using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedBass.Enc
{
    /// <summary>
    /// Wraps BassEnc: bassenc.dll
    /// </summary>
    public static partial class BassEnc
    {
        static IntPtr _castProxy;
        
        #region Version
        [DllImport(DllName)]
        static extern int BASS_Encode_GetVersion();

        /// <summary>
        /// Gets the Version of BassEnc that is loaded.
        /// </summary>
        public static Version Version => Extensions.GetVersion(BASS_Encode_GetVersion());
        #endregion

        #region Configure
        /// <summary>
        /// Encoder DSP priority (default -1000) which determines where in the DSP chain the encoding is performed. 
        /// </summary>
        /// <remarks>
        /// All DSP with a higher priority will be present in the encoding.
        /// Changes only affect subsequent encodings, not those that have already been started.
        /// </remarks>
        public static int DSPPriority
        {
            get { return Bass.GetConfig(Configuration.EncodePriority); }
            set { Bass.Configure(Configuration.EncodePriority, value); }
        }

        /// <summary>
        /// The maximum queue Length of the async encoder (default 10000, 0 = Unlimited) in milliseconds.
        /// </summary>
        /// <remarks>
        /// When queued encoding is enabled, the queue's Buffer will grow as needed to hold the queued data, up to a limit specified by this config option.
        /// Changes only apply to new encoders, not any already existing encoders.
        /// </remarks>
        public static int Queue
        {
            get { return Bass.GetConfig(Configuration.EncodeQueue); }
            set { Bass.Configure(Configuration.EncodeQueue, value); }
        }

        /// <summary>
        /// The time to wait (in milliseconds) to send data to a cast server (default 5000ms)
        /// </summary>
        /// <remarks>
        /// When an attempt to send data is timed-out, the data is discarded. 
        /// <see cref="EncodeSetNotify"/> can be used to receive a notification of when this happens.
        /// Changes take immediate effect.
        /// </remarks>
        public static int CastTimeout
        {
            get { return Bass.GetConfig(Configuration.EncodeCastTimeout); }
            set { Bass.Configure(Configuration.EncodeCastTimeout, value); }
        }

        /// <summary>
        /// Proxy server settings when connecting to Icecast and Shoutcast (in the form of "[User:Password@]server:port"... <see langword="null"/> (default) = don't use a proxy but a direct connection).
        /// </summary>
        /// <remarks>
        /// If only the "server:port" part is specified, then that proxy server is used without any authorization credentials.
        /// This setting affects how the following functions connect to servers: <see cref="CastInit"/>, <see cref="CastGetStats"/>, <see cref="CastSetTitle(int, string, string)"/>.
        /// When a proxy server is used, it needs to support the HTTP 'CONNECT' method.
        /// The default setting is <see langword="null"/> (do not use a proxy).
        /// Changes take effect from the next internet stream creation call. 
        /// By default, BassEnc will not use any proxy settings when connecting to Icecast and Shoutcast.
        /// </remarks>
        public static string CastProxy
        {
            // BassEnc does not make a copy of the config string, so it must reside in the heap (not the stack), eg. a global variable. 
            // This also means that the proxy settings can subsequently be changed at that location without having to call this function again.

            get { return Marshal.PtrToStringAnsi(Bass.GetConfigPtr(Configuration.EncodeCastProxy)); }
            set
            {
                if (_castProxy != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_castProxy);

                    _castProxy = IntPtr.Zero;
                }

                _castProxy = Marshal.StringToHGlobalAnsi(value);

                Bass.Configure(Configuration.EncodeCastProxy, _castProxy);
            }
        }
        #endregion

        #region Encoding
        /// <summary>
        /// Sends a RIFF chunk to an encoder.
        /// </summary>
        /// <param name="Handle">The encoder Handle... a HENCODE.</param>
        /// <param name="ID">The 4 character chunk id (e.g. 'bext').</param>
        /// <param name="Buffer">The buffer containing the chunk data (without the id).</param>
        /// <param name="Length">The number of bytes in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// BassEnc writes the minimum chunks required of a WAV file: "fmt" and "data", and "ds64" and "fact" when appropriate.
        /// This function can be used to add other chunks. 
        /// For example, a BWF "bext" chunk or "INFO" tags.
        /// <para>
        /// Chunks can only be added prior to sample data being sent to the encoder.
        /// The <see cref="EncodeFlags.Pause"/> flag can be used when starting the encoder to ensure that no sample data is sent before additional chunks have been set.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No RIFF headers/chunks are being sent to the encoder (due to the <see cref="EncodeFlags.NoHeader"/> flag being in effect), or sample data encoding has started.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_AddChunk")]
        public static extern bool EncodeAddChunk(int Handle, string ID, IntPtr Buffer, int Length);

        /// <summary>
        /// Sends a RIFF chunk to an encoder.
        /// </summary>
        /// <param name="Handle">The encoder Handle... a HENCODE.</param>
        /// <param name="ID">The 4 character chunk id (e.g. 'bext').</param>
        /// <param name="Buffer">The buffer containing the chunk data (without the id).</param>
        /// <param name="Length">The number of bytes in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// BassEnc writes the minimum chunks required of a WAV file: "fmt" and "data", and "ds64" and "fact" when appropriate.
        /// This function can be used to add other chunks. 
        /// For example, a BWF "bext" chunk or "INFO" tags.
        /// <para>
        /// Chunks can only be added prior to sample data being sent to the encoder.
        /// The <see cref="EncodeFlags.Pause"/> flag can be used when starting the encoder to ensure that no sample data is sent before additional chunks have been set.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No RIFF headers/chunks are being sent to the encoder (due to the <see cref="EncodeFlags.NoHeader"/> flag being in effect), or sample data encoding has started.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_AddChunk")]
        public static extern bool EncodeAddChunk(int Handle, string ID, byte[] Buffer, int Length);
        
        /// <summary>
        /// Retrieves the channel that an encoder is set on.
        /// </summary>
        /// <param name="Handle">The encoder to get the channel from.</param>
        /// <returns>If successful, the encoder's channel Handle is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_GetChannel")]
        public static extern int EncodeGetChannel(int Handle);

        /// <summary>
        /// Retrieves the amount of data queued, sent to or received from an encoder, or sent to a cast server.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Count">The count to retrieve (see <see cref="EncodeCount"/>).</param>
        /// <returns>If successful, the requested count (in bytes) is returned, else -1 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// The queue counts are based on the channel's sample format (floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled),
        /// while the <see cref="EncodeCount.In"/> count is based on the sample format used by the encoder,
        /// which could be different if one of the Floating-point conversion flags is active or the encoder is using an ACM codec (which take 16-bit data).
        /// </para>
        /// <para>
        /// When the encoder output is being sent to a cast server, the <see cref="EncodeCount.Cast"/> count will match the <see cref="EncodeCount.Out"/> count,
        /// unless there have been problems (eg. network timeout) that have caused data to be dropped.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">The encoder does not have a queue.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Count" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_GetCount")]
        public static extern long EncodeGetCount(int Handle, EncodeCount Count);

		/// <summary>
		/// Checks if an encoder is running on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>The return value is one of <see cref="PlaybackState"/> values.</returns>
		/// <remarks>
		/// <para>When checking if there's an encoder running on a channel, and there are multiple encoders on the channel, <see cref="PlaybackState.Playing"/> will be returned if any of them are active.</para>
		/// <para>If an encoder stops running prematurely, <see cref="EncodeStop(int)" /> should still be called to release resources that were allocated for the encoding.</para>
		/// </remarks>
        [DllImport(DllName, EntryPoint = "BASS_Encode_IsActive")]
        public static extern PlaybackState EncodeIsActive(int Handle);
        
		/// <summary>
		/// Moves an encoder (or all encoders on a channel) to another channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Channel">The channel to move the encoder(s) to... a HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
        /// The new channel must have the same sample format (rate, channels, resolution) as the old channel, as that is what the encoder is expecting. 
		/// A channel's sample format is available via <see cref="Bass.ChannelGetInfo(int, out ChannelInfo)" />.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> or <paramref name="Channel" /> is not valid.</exception>
        /// <exception cref="Errors.SampleFormat">The new channel's sample format is not the same as the old channel's.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetChannel")]
        public static extern bool EncodeSetChannel(int Handle, int Channel);

        /// <summary>
        /// Sets a callback function on an encoder (or all encoders on a channel) to receive notifications about its status.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Procedure">Callback function to receive the notifications... <see langword="null" /> = no callback.</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// When setting a notification callback on a channel, it only applies to the encoders that are currently set on the channel.
        /// Subsequent encoders will not automatically have the notification callback set on them, this function will have to be called again to set them up.
        /// </para>
        /// <para>
        /// An encoder can only have one notification callback set.
        /// Subsequent calls of this function can be used to change the callback function, or disable notifications (<paramref name="Procedure"/> = <see langword="null" />).
        /// </para>
        /// <para>
        /// The status of an encoder and its cast connection (if it has one) is checked when data is sent to the encoder or server, and by <see cref="EncodeIsActive" />.
        /// That means an encoder's death will not be detected automatically, and so no notification given, while no data is being encoded.
        /// </para>
        /// <para>If the encoder is already dead when setting up a notification callback, the callback will be triggered immediately.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetNotify")]
        public static extern bool EncodeSetNotify(int Handle, EncodeNotifyProcedure Procedure, IntPtr User = default(IntPtr));
        
		/// <summary>
		/// Pauses or resumes encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Paused">Paused?</param>
		/// <returns>If no encoder has been started on the channel, <see langword="false" /> is returned, otherwise <see langword="true" /> is returned.</returns>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// When an encoder is paused, no sample data will be sent to the encoder "automatically".
        /// Data can still be sent to the encoder "manually" though, via the <see cref="EncodeWrite(int, IntPtr, int)" /> function.
        /// </para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.s</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_SetPaused")]
        public static extern bool EncodeSetPaused(int Handle, bool Paused = true);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_Start(int handle, string cmdline, EncodeFlags flags, EncodeProcedure proc, IntPtr user);

        /// <summary>
        /// Starts encoding on a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="CommandLine">The encoder command-line, including the executable filename and any options. Or the output filename if the <see cref="EncodeFlags.PCM"/> flag is specified.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Procedure">Optional callback function to receive the encoded data... <see langword="null" /> = no callback. To have the encoded data received by a callback function, the encoder needs to be told to output to STDOUT (instead of a file).</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <returns>The encoder process Handle is returned if the encoder is successfully started, else 0 is returned (use <see cref="Bass.LastError" /> to get the error code).</returns>
        /// <remarks>
        /// <para>
        /// The encoder must be told (via the command-line) to expect input from STDIN, rather than a file.
        /// The command-line should also tell the encoder what filename to write it's output to, unless you're using a callback function, in which case it should be told to write it's output to STDOUT.
        /// </para>
        /// <para>
        /// No user interaction with the encoder is possible, so anything that would cause the encoder to require the user to press any keys should be avoided.
        /// For example, if the encoder asks whether to overwrite files, the encoder should be instructed to always overwrite (via the command-line), or you should delete the existing file before starting the encoder.
        /// </para>
        /// <para>
        /// Standard RIFF files are limited to a little over 4GB in size.
        /// When writing a WAV file, BASSenc will automatically stop at that point, so that the file is valid.
        /// That does not apply when sending data to an encoder though, as the encoder may (possibly via a command-line option) ignore the size restriction, but if it does not, it could mean that the encoder stops after a few hours (depending on the sample format).
        /// If longer encodings are needed, the <see cref="EncodeFlags.NoHeader"/> flag can be used to omit the WAVE header, and the encoder informed of the sample format via the command-line instead.
        /// The 4GB size limit can also be overcome with the <see cref="EncodeFlags.RF64"/> flag, but most encoders are unlikely to support RF64.
        /// </para>
        /// <para>
        /// When writing an RF64 WAV file, a standard RIFF header will still be written initially, which will only be replaced by an RF64 header at the end if the file size has exceeded the standard limit.
        /// When an encoder is used, it is not possible to go back and change the header at the end, so the RF64 header is sent at the beginning in that case.
        /// </para>
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the encoder at the same time. 
        /// It also means that if you use the <see cref="Bass.FloatingPointDSP"/> option, then the sample data will be 32-bit floating-point, and you'll need to use one of the Floating-point flags if the encoder does not support floating-point sample data. 
        /// The <see cref="Bass.FloatingPointDSP"/> setting should not be changed while encoding is in progress.
        /// </para>
        /// <para>The encoder DSP has a priority setting of -1000, so if you want to set DSP/FX on the channel and have them present in the encoding, set their priority above that.</para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function.
        /// Both methods can be used together, but in general, the "automatic" system ought be paused when using the "manual" system, by use of the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost; <see cref="EncodeSetNotify(int,EncodeNotifyProcedure,IntPtr)" /> can be used to be notified of that occurrence.
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount(int,EncodeCount)" />.
        /// </para>
        /// <para>
        /// <see cref="EncodeIsActive" /> can be used to check that the encoder is still running.
        /// When done encoding, use <see cref="EncodeStop(int)" /> to close the encoder.
        /// </para>
        /// <para>The returned process Handle can be used to do things like change the encoder's priority and get it's exit code.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For simplicity, the encoder functions will accept either an encoder Handle or a channel Handle.
        /// When using a channel Handle, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>External encoders are not supported on iOS or Windows CE, so only plain PCM file writing with the <see cref="EncodeFlags.PCM"/> flag is possible on those platforms.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.FileOpen">Couldn't start the encoder. Check that the executable exists.</exception>
        /// <exception cref="Errors.Create">The PCM file couldn't be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeStart(int Handle, string CommandLine, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User = default(IntPtr))
        {
            return BASS_Encode_Start(Handle, CommandLine, Flags | EncodeFlags.Unicode, Procedure, User);
        }

#if __MAC__ || __IOS__
        [DllImport(DllName, EntryPoint = "BASS_Encode_StartCA")]
        public static extern int EncodeStartCA(int Handle, int ftype, int atype, EncodeFlags flags, int bitrate, EncodeProcedureEx Procedure, IntPtr user);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartCAFile(int Handle, int ftype, int atype, EncodeFlags flags, int bitrate, string filename);

        public static int EncodeStartCA(int Handle, int ftype, int atype, EncodeFlags flags, int bitrate, string filename)
        {
            return BASS_Encode_StartCAFile(Handle, ftype, atype, flags | EncodeFlags.Unicode, bitrate, filename);
        }
#endif

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartLimit(int handle, string cmdline, EncodeFlags flags, EncodeProcedure proc, IntPtr user, int limit);

        /// <summary>
        /// Starts encoding on a channel.
        /// </summary>
        /// <param name="Handle">The channel Handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="CommandLine">The encoder command-line, including the executable filename and any options. Or the output filename if the <see cref="EncodeFlags.PCM"/> flag is specified.</param>
        /// <param name="Flags">A combination of <see cref="BassFlags"/>.</param>
        /// <param name="Procedure">Optional callback function to receive the encoded data... <see langword="null" /> = no callback. To have the encoded data received by a callback function, the encoder needs to be told to output to STDOUT (instead of a file).</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <param name="Limit">The maximum number of bytes that will be encoded (0 = no limit).</param>
        /// <returns>The encoder process Handle is returned if the encoder is successfully started, else 0 is returned (use <see cref="Bass.LastError" /> to get the error code).</returns>
        /// <remarks>
        /// <para>
        /// This function works exactly like <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />, but with a <paramref name="Limit" /> parameter added, which is the maximum number of bytes that will be encoded (0=no limit).
        /// Once the limit is hit, the encoder will die.
        /// <see cref="EncodeSetNotify" /> can be used to be notified of that occurrence.
        /// One thing to note is that the limit is applied after any conversion due to the floating-point flags.
        /// </para>
        /// <para>This can be useful in situations where the encoder needs to know in advance how much data it will be receiving. For example, when using a callback function with a file format that stores the length in the header, as the header cannot then be updated at the end of encoding. The length is communicated to the encoder via the WAVE header, so it requires that the BASS_ENCODE_NOHEAD flag is not used.</para>
        /// <para>
        /// The encoder must be told (via the command-line) to expect input from STDIN, rather than a file.
        /// The command-line should also tell the encoder what filename to write it's output to, unless you're using a callback function, in which case it should be told to write it's output to STDOUT.
        /// </para>
        /// <para>
        /// No user interaction with the encoder is possible, so anything that would cause the encoder to require the user to press any keys should be avoided.
        /// For example, if the encoder asks whether to overwrite files, the encoder should be instructed to always overwrite (via the command-line), or you should delete the existing file before starting the encoder.
        /// </para>
        /// <para>
        /// Standard RIFF files are limited to a little over 4GB in size.
        /// When writing a WAV file, BASSenc will automatically stop at that point, so that the file is valid.
        /// That does not apply when sending data to an encoder though, as the encoder may (possibly via a command-line option) ignore the size restriction, but if it does not, it could mean that the encoder stops after a few hours (depending on the sample format).
        /// If longer encodings are needed, the <see cref="EncodeFlags.NoHeader"/> flag can be used to omit the WAVE header, and the encoder informed of the sample format via the command-line instead.
        /// The 4GB size limit can also be overcome with the <see cref="EncodeFlags.RF64"/> flag, but most encoders are unlikely to support RF64.
        /// </para>
        /// <para>
        /// When writing an RF64 WAV file, a standard RIFF header will still be written initially, which will only be replaced by an RF64 header at the end if the file size has exceeded the standard limit.
        /// When an encoder is used, it is not possible to go back and change the header at the end, so the RF64 header is sent at the beginning in that case.
        /// </para>
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when you play the channel (or call <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> if it's a decoding channel), the sample data will be sent to the encoder at the same time. 
        /// It also means that if you use the <see cref="Bass.FloatingPointDSP"/> option, then the sample data will be 32-bit floating-point, and you'll need to use one of the Floating-point flags if the encoder does not support floating-point sample data. 
        /// The <see cref="Bass.FloatingPointDSP"/> setting should not be changed while encoding is in progress.
        /// </para>
        /// <para>The encoder DSP has a priority setting of -1000, so if you want to set DSP/FX on the channel and have them present in the encoding, set their priority above that.</para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function.
        /// Both methods can be used together, but in general, the "automatic" system ought be paused when using the "manual" system, by use of the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost; <see cref="EncodeSetNotify(int,EncodeNotifyProcedure,IntPtr)" /> can be used to be notified of that occurrence.
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount(int,EncodeCount)" />.
        /// </para>
        /// <para>
        /// <see cref="EncodeIsActive" /> can be used to check that the encoder is still running.
        /// When done encoding, use <see cref="EncodeStop(int)" /> to close the encoder.
        /// </para>
        /// <para>The returned process Handle can be used to do things like change the encoder's priority and get it's exit code.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For simplicity, the encoder functions will accept either an encoder Handle or a channel Handle.
        /// When using a channel Handle, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>External encoders are not supported on iOS or Windows CE, so only plain PCM file writing with the <see cref="EncodeFlags.PCM"/> flag is possible on those platforms.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.FileOpen">Couldn't start the encoder. Check that the executable exists.</exception>
        /// <exception cref="Errors.Create">The PCM file couldn't be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeStart(int Handle, string CommandLine, EncodeFlags Flags, EncodeProcedure Procedure, IntPtr User, int Limit)
        {
            return BASS_Encode_StartLimit(Handle, CommandLine, Flags | EncodeFlags.Unicode, Procedure, User, Limit);
        }

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        static extern int BASS_Encode_StartUser(int handle, string filename, EncodeFlags flags, EncoderProcedure proc, IntPtr user);

        /// <summary>
        /// Sets up a user-provided encoder on a channel.
        /// </summary>
        /// <param name="Handle">The channel handle... a HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Filename">Output filename... <see langword="null" /> = no output file.</param>
        /// <param name="Flags">A combination of <see cref="EncodeFlags"/>.</param>
        /// <param name="Procedure">Callback function to receive the sample data and return the encoded data.</param>
        /// <param name="User">User instance data to Password to the callback function.</param>
        /// <returns>The encoder process handle is returned if the encoder is successfully started, else 0 is returned (use <see cref="Bass.LastError" /> to get the error code).</returns>
        /// <remarks>
        /// <para>
        /// This function allows user-provided encoders to be used, which is most useful for platforms where external encoders are unavailable 
        /// for use with <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />.
        /// For example, the LAME library could be used with this function instead of the standalone LAME executable with <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" />.
        /// </para>
        /// <para>
        /// Internally, the sending of sample data to the encoder is implemented via a DSP callback on the channel.
        /// That means when the channel is played (or <see cref="Bass.ChannelGetData(int,IntPtr,int)" /> is called if it is a decoding channel), the sample data will be sent to the encoder at the same time.
        /// It also means that if the <see cref="Bass.FloatingPointDSP"/> option is enabled, the sample data will be 32-bit floating-point, and one of the floating-point flags will be required if the encoder does not support floating-point sample data.
        /// The <see cref="Bass.FloatingPointDSP"/> setting should not be changed while encoding is in progress.
        /// </para>
        /// <para>
        /// By default, the encoder DSP has a priority setting of -1000, which determines where in the DSP chain the encoding is performed.
        /// That can be changed via the <see cref="DSPPriority"/> config option.
        /// </para>
        /// <para>
        /// Besides the automatic DSP system, data can also be manually fed to the encoder via the <see cref="EncodeWrite(int,IntPtr,int)" /> function. 
        /// Both methods can be used together, but in general, the 'automatic' system ought to be paused when using the 'manual' system, via the <see cref="EncodeFlags.Pause"/> flag or the <see cref="EncodeSetPaused" /> function.
        /// Data fed to the encoder manually does not go through the source channel's DSP chain, so any DSP/FX set on the channel will not be applied to the data.
        /// </para>
        /// <para>
        /// When queued encoding is enabled via the <see cref="EncodeFlags.Queue"/> flag, the DSP system or <see cref="EncodeWrite(int,IntPtr,int)" /> call will just buffer the data, and the data will then be fed to the encoder by another thread.
        /// The buffer will grow as needed to hold the queued data, up to a limit specified by the <see cref="Queue"/> config option.
        /// If the limit is exceeded (or there is no free memory), data will be lost;
        /// <see cref="EncodeSetNotify" /> can be used to be notified of that occurrence. 
        /// The amount of data that is currently queued, as well as the queue limit and how much data has been lost, is available from <see cref="EncodeGetCount" />.
        /// </para>
        /// <para>When done encoding, use <see cref="EncodeStop(int)" /> or <see cref="EncodeStop(int,bool)" /> to close the encoder.</para>
        /// <para>
        /// Multiple encoders can be set on a channel.
        /// For convenience, most of the encoder functions will accept either an encoder handle or a channel handle. 
        /// When a channel handle is used, the function is applied to all encoders that are set on that channel.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Create">The file could not be created.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static int EncodeStart(int Handle, string Filename, EncodeFlags Flags, EncoderProcedure Procedure, IntPtr User = default(IntPtr))
        {
            return BASS_Encode_StartUser(Handle, Filename, Flags | EncodeFlags.Unicode, Procedure, User);
        }
        
		/// <summary>
		/// Stops encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// This function will free an encoder immediately, without waiting for any data that may be remaining in the queue.
        /// <see cref="EncodeStop(int, bool)" /> can be used to have an encoder process the queue before it is freed.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Stop")]
        public static extern bool EncodeStop(int Handle);
        
		/// <summary>
		/// Stops async encoding on a channel.
		/// </summary>
		/// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
		/// <param name="Queue">Process the queue first? If so, the encoder will not be freed until after any data remaining in the queue has been processed, and it will not accept any new data in the meantime.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// When an encoder is told to wait for its queue to be processed, this function will return immediately and the encoder will be freed in the background after the queued data has been processed.
		/// <see cref="EncodeSetNotify" /> can be used to request notification of when the encoder has been freed.
        /// <see cref="EncodeStop(int)" /> (or this function with queue = <see langword="false" />) can be used to cancel to queue processing and free the encoder immediately.
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_StopEx")]
        public static extern bool EncodeStop(int Handle, bool Queue);

        #region Encode Write
        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">A pointer to the buffer containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, IntPtr Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">byte[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, byte[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">short[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, short[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">int[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, int[] Buffer, int Length);

        /// <summary>
        /// Sends sample data to the encoder.
        /// </summary>
        /// <param name="Handle">The encoder or channel Handle... a HENCODE, HSTREAM, HMUSIC, or HRECORD.</param>
        /// <param name="Buffer">float[] containing the sample data.</param>
        /// <param name="Length">The number of BYTES in the buffer.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// There's usually no need to use this function, as the channel's sample data will automatically be fed to the encoder.
        /// But in some situations, it could be useful to be able to manually feed the encoder instead.
        /// <para>The sample data is expected to be the same format as the channel's, or floating-point if the <see cref="Bass.FloatingPointDSP"/> option is enabled.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Ended">The encoder has died.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_Write")]
        public static extern bool EncodeWrite(int Handle, float[] Buffer, int Length);
        #endregion
        #endregion

        #region Casting
        [DllImport(DllName)]
        static extern IntPtr BASS_Encode_CastGetStats(int handle, EncodeStats type, [In] string pass);

        /// <summary>
        /// Retrieves stats from the Shoutcast or Icecast server.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Type">The type of stats to retrieve.</param>
        /// <param name="Password">Password when retrieving Icecast server stats... <see langword="null" /> = use the password provided in the <see cref="CastInit" /> call.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The stats are returned in XML format.
        /// <para>
        /// Each encoder has a single stats buffer, which is reused by each call of this function for the encoder. 
        /// So if the data needs to be retained across multiple calls, it should be copied to another buffer.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Type"><paramref name="Type" /> is invalid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast of the requested type set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static string CastGetStats(int Handle, EncodeStats Type, string Password)
        {
            return Marshal.PtrToStringAnsi(BASS_Encode_CastGetStats(Handle, Type, Password));
        }

        /// <summary>
        /// Initializes sending an encoder's output to a Shoutcast or Icecast server.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Server">The server to send to, in the form of "address:port" (Shoutcast v1) resp. "address:port,sid" (Shoutcast v2) or "address:port/mount" (Icecast).</param>
        /// <param name="Password">The server password. A username can be included in the form of "username:password" when connecting to an Icecast or Shoutcast 2 server.</param>
        /// <param name="Content">
        /// The MIME type of the encoder output.
        /// <para>'audio/mpeg' for Mp3, 'application/ogg' for Ogg and 'audio/aacp' for Aac.</para>
        /// </param>
        /// <param name="Name">The stream name... <see langword="null" /> = no name.</param>
        /// <param name="Url">The URL, for example, of the radio station's webpage... <see langword="null" /> = no URL.</param>
        /// <param name="Genre">The genre... <see langword="null" /> = no genre.</param>
        /// <param name="Description">Description... <see langword="null" /> = no description. This applies to Icecast only.</param>
        /// <param name="Headers">Other headers to send to the server... <see langword="null" /> = none. Each header should end with a carriage return and line feed ("\r\n").</param>
        /// <param name="Bitrate">The bitrate (in kbps) of the encoder output... 0 = undefined bitrate. In cases where the bitrate is a "quality" (rather than CBR) setting, the headers parameter can be used to communicate that instead, eg. "ice-bitrate: Quality 0\r\n".</param>
        /// <param name="Public">Public? If <see langword="true" />, the stream is added to the public directory of streams, at shoutcast.com or dir.xiph.org (or as defined in the server config).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// This function sets up a Shoutcast/Icecast source client, sending the encoder's output to a server, which listeners can then connect to and receive the data from. 
        /// The Shoutcast and Icecast server software is available from http://www.shoutcast.com/broadcast-tools and http://www.icecast.org/download.php, respectively.
        /// </para>
        /// <para>
        /// An encoder needs to be started (but with no data sent to it yet) before using this function to setup the sending of the encoder's output to a Shoutcast or Icecast server.
        /// If <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" /> is used, the encoder should be setup to write its output to STDOUT.
        /// Due to the length restrictions of WAVE headers/files, the encoder should also be started with the <see cref="EncodeFlags.NoHeader"/> flag, and the sample format details sent via the command-line.
        /// </para>
        /// <para>
        /// Unless the <see cref="EncodeFlags.UnlimitedCastDataRate"/> flag is set on the encoder, BASSenc automatically limits the rate that data is processed to real-time speed to avoid overflowing the server's buffer, which means that it is safe to simply try to process data as quickly as possible, eg. when the source is a decoding channel.
        /// Encoders set on recording channels are automatically exempt from the rate limiting, as they are inherently real-time.
        /// With BASS 2.4.6 or above, also exempt are encoders that are fed in a playback buffer update cycle (including <see cref="Bass.Update" /> and <see cref="Bass.ChannelUpdate" /> calls), eg. when the source is a playing channel;
        /// that is to avoid delaying the update thread, which could result in playback buffer underruns.
        /// </para>
        /// <para>
        /// Normally, BASSenc will produce the encoded data (with the help of an encoder) that is sent to a Shoutcast/Icecast server, but it is also possible to send already encoded data to a server (without first decoding and re-encoding it) via the PCM encoding option.
        /// The encoder can be set on any BASS channel, as rather than feeding on sample data from the channel, <see cref="EncodeWrite(int,IntPtr,int)" /> would be used to feed in the already encoded data.
        /// BASSenc does not know what the data's bitrate is in that case, so it is up to the user to process the data at the correct rate (real-time speed).
        /// </para>
        /// <para><see cref="ServerInit" /> can be used to setup a server that listeners can connect to directly, without a Shoutcast/Icecast server intermediary.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Already">There is already a cast set on the encoder.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Server" /> doesn't include a port number.</exception>
        /// <exception cref="Errors.FileOpen">Couldn't connect to the server.</exception>
        /// <exception cref="Errors.CastDenied"><paramref name="Password" /> is not valid.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_CastInit")]
        public static extern bool CastInit(int Handle,
            string Server,
            string Password,
            string Content,
            string Name,
            string Url,
            string Genre,
            string Description,
            string Headers,
            int Bitrate,
            bool Public);

        [DllImport(DllName)]
        static extern bool BASS_Encode_CastSendMeta(int handle, EncodeMetaDataType type, byte[] data, int length);
        
        /// <summary>
        /// Sends metadata to a Shoutcast 2 server.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Type">The type of metadata.</param>
        /// <param name="Buffer">The XML metadata as an UTF-8 encoded byte array.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool CastSendMeta(int Handle, EncodeMetaDataType Type, byte[] Buffer)
        {
            return BASS_Encode_CastSendMeta(Handle, Type, Buffer, Buffer.Length);
        }

        /// <summary>
        /// Sends metadata to a Shoutcast 2 server.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Type">The type of metadata.</param>
        /// <param name="Metadata">The XML metadata to send.</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        public static bool CastSendMeta(int Handle, EncodeMetaDataType Type, string Metadata)
        {
            if (string.IsNullOrEmpty(Metadata))
                return false;

            var bytes = Encoding.UTF8.GetBytes(Metadata);
            return BASS_Encode_CastSendMeta(Handle, Type, bytes, bytes.Length);
        }

        /// <summary>
        /// Sets the title (ANSI) of a cast stream.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Title">The title to set.</param>
        /// <param name="Url">URL to go with the title... <see langword="null" /> = no URL. This applies to Shoutcast only (not Shoutcast 2).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The ISO-8859-1 (Latin-1) character set should be used with Shoutcast servers, and UTF-8 with Icecast and Shoutcast 2 servers.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_CastSetTitle")]
        public static extern bool CastSetTitle(int Handle, string Title, string Url);

        /// <summary>
        /// Sets the title of a cast stream.
        /// </summary>
        /// <param name="Handle">The encoder Handle.</param>
        /// <param name="Title">encoded byte[] containing the title to set.</param>
        /// <param name="Url">encoded byte[] containing the URL to go with the title... <see langword="null" /> = no URL. This applies to Shoutcast only (not Shoutcast 2).</param>
        /// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// The ISO-8859-1 (Latin-1) character set should be used with Shoutcast servers, and UTF-8 with Icecast and Shoutcast 2 servers.
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">There isn't a cast set on the encoder.</exception>
        /// <exception cref="Errors.Timeout">The server did not respond to the request within the timeout period, as set with the <see cref="Bass.NetTimeOut"/> config option.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_CastSetTitle")]
        public static extern bool CastSetTitle(int Handle, byte[] Title, byte[] Url);
        #endregion

        #region Server
        /// <summary>
        /// Initializes a server to send an encoder's output to connecting clients.
        /// </summary>
        /// <param name="Handle">The encoder handle.</param>
        /// <param name="Port">
        /// The IP address and port number to accept client connections on... "xxx.xxx.xxx.xxx:port", <see langword="null" /> = an available port on all local addresses.
        /// The IP address should be local and the port number should be lower than 65536.
        /// If the address is "0.0.0.0" or omitted, then the server will accept connections on all local addresses.
        /// If the port is "0" or omitted, then an available port will be assigned.
        /// </param>
        /// <param name="Buffer">The server's buffer length in bytes.</param>
        /// <param name="Burst">The amount of buffered data to send to new clients. This will be capped at the size of the buffer.</param>
        /// <param name="Flags"><see cref="EncodeServer"/> flags.</param>
        /// <param name="Procedure">Callback function to receive notification of clients connecting and disconnecting... <see langword="null" /> = no callback.</param>
        /// <param name="User">User instance data to pass to the callback function.</param>
        /// <returns>If successful, the new server's port number is returned, else 0 is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
        /// <remarks>
        /// <para>
        /// This function allows remote (or local) clients to receive the encoder's output by setting up a TCP server for them to connect to, using <see cref="Bass.CreateStream(string,int,BassFlags,DownloadProcedure,IntPtr)" /> for example. 
        /// Connections can be refused by the <see cref="EncodeClientProcedure" /> callback function, and already connected clients can be kicked with the <see cref="ServerKick" /> function.
        /// </para>
        /// <para>
        /// The server buffers the data that it receives from the encoder, and the data is then sent from the buffer to the connected clients.
        /// The buffer should be at least big enough to account for the time that it takes for the clients to receive the data.
        /// If a client falls too far behind (beyond the buffer length), it will miss some data.
        /// When a client connects, buffered data can be "burst" to the client, allowing it to prebuffer and begin playback more quickly.
        /// </para>
        /// <para>
        /// An encoder needs to be started, but with no data yet sent to it, before using this function to setup the server.
        /// If <see cref="EncodeStart(int,string,EncodeFlags,EncodeProcedure,IntPtr)" /> is used, the encoder should be setup to write its output to STDOUT.
        /// Due to the length restrictions of WAVE headers/files, the encoder should also be started with the <see cref="EncodeFlags.NoHeader"/> flag, and the sample format details sent via the command-line.
        /// </para>
        /// <para>
        /// Normally, BASSenc will produce the encoded data (with the help of an encoder) that is sent to a clients, but it is also possible to send already encoded data (without first decoding and re-encoding it) via the PCM encoding option.
        /// The encoder can be set on any BASS channel, as rather than feeding on sample data from the channel, <see cref="EncodeWrite(int,IntPtr,int)" /> would be used to feed in the already encoded data.
        /// BASSenc does not know what the data's bitrate is in that case, so it is up to the user to process the data at the correct rate (real-time speed).
        /// </para>
        /// <para><b>Platform-specific</b></para>
        /// <para>This function is not available on Windows CE.</para>
        /// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.Already">There is already a server set on the encoder.</exception>
        /// <exception cref="Errors.Parameter"><paramref name="Port" /> is not valid.</exception>
        /// <exception cref="Errors.Busy">The port is in use.</exception>
        /// <exception cref="Errors.Memory">There is insufficient memory.</exception>
        /// <exception cref="Errors.Unknown">Some other mystery problem!</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_ServerInit")]
        public static extern int ServerInit(int Handle, string Port, int Buffer, int Burst, EncodeServer Flags, EncodeClientProcedure Procedure, IntPtr User);
        
		/// <summary>
		/// Kicks clients from a server.
		/// </summary>
		/// <param name="Handle">The encoder Handle.</param>
		/// <param name="Client">The client(s) to kick... "" (empty string) = all clients. Unless a port number is included, this string is compared with the start of the connected clients' IP address.</param>
		/// <returns>If successful, <see langword="true" /> is returned, else <see langword="false" /> is returned. Use <see cref="Bass.LastError" /> to get the error code.</returns>
		/// <remarks>
		/// <para>
        /// The clients may not be kicked immediately, but shortly after the call.
        /// If the server has been setup with an <see cref="EncodeClientProcedure" /> callback function, that will receive notification of the disconnections.
        /// </para>
		/// <para><b>Platform-specific</b></para>
		/// <para>This function is not available on Windows CE.</para>
		/// </remarks>
        /// <exception cref="Errors.Handle"><paramref name="Handle" /> is not valid.</exception>
        /// <exception cref="Errors.NotAvailable">No matching clients were found.</exception>
        [DllImport(DllName, EntryPoint = "BASS_Encode_ServerKick")]
        public static extern int ServerKick(int Handle, string Client = "");
        #endregion
    }
}