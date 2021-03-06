﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;
using System.Threading.Tasks;
using System;
using System.IO;
using AudioConversion.AudioConversionService;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AudioConversions;

namespace AudioConversion.RESTApi.AudioConversion
{
    [Route("api/v1/audioconversion")]
    [ApiController]
    public class AudioConversionController : ControllerBase
    {
        private readonly ILogger<Program> _logger = null;
        private readonly IAudioConversionService _audioconversionservice = null;

        // Set by dependency injection.
        public AudioConversionController(ILogger<Program> logger, IAudioConversionService audioconversionservice)
        {
            _logger = logger;
            _audioconversionservice = audioconversionservice;
        }

        /// <summary>
        /// Converts an audio file to wav format. Can change the sample rate, bit depth and channels
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="inputFormat">The format of the audio. Values include wav, ogg, raw </param>
        /// <param name="sampleRate">The sample rate of the audio. Values include 8000, 16000...</param>
        /// <param name="bitDepth"> The precision or bit depth e.g. 16 bit</param>
        /// <param name="channels">The number of channels in the file</param>
        /// <returns>A stream of the audio</returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>            
        /// <response code="500">Internal Server Error</response>            
        [HttpPost("wav-converter")]
        [DisableFormValueModelBinding]
        [Consumes("multipart/form-data")] // You can specify consumes here and it gets automatically added also to swagger
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(Stream))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Stream))]
        public async Task<ActionResult> WavConverter(string inputFormat, int sampleRate, int bitDepth, int channels)
        {
            // Specify paths.
            string sTempLocalFileName = Utilities.GetTempFilenameSafe("." + inputFormat);
            string sTempFinalLocalFileName = Utilities.GetTempFilenameSafe(".wav");

            try
            {
                FormValueProvider formModel;
                using (var stream = System.IO.File.Create(sTempLocalFileName))
                {
                    formModel = await Request.StreamFile(stream);
                }
                // Return the record.
                _audioconversionservice.WavConverter(sTempLocalFileName, sTempFinalLocalFileName, inputFormat, sampleRate, bitDepth, channels);

                return new TempPhysicalFileResult(sTempFinalLocalFileName, "audio/wav") { FileDownloadName = Path.GetFileName(sTempFinalLocalFileName) };
            }
            finally
            {
                // Delete the initial streamed file
                if (System.IO.File.Exists(sTempLocalFileName))
                    System.IO.File.Delete(sTempLocalFileName);
            }

        }

        /// <summary>
        /// Converts an audio file from mp3 to wav
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sampleRate">The sample rate of the audio</param>
        /// <returns>A stream of the audio</returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>            
        /// <response code="500">Internal Server Error</response>            
        [HttpPost("convert-mp3-wav")]
        [DisableFormValueModelBinding]
        [Consumes("multipart/form-data")] // You can specify consumes here and it gets automatically added also to swagger
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(Stream))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Stream))]
        public async Task<ActionResult> ConvertMP3ToWav(int sampleRate)
        {
            // Specify paths.
            string sTempLocalFileName = Utilities.GetTempFilenameSafe(".mp3");
            string sTempFinalLocalFileName = Utilities.GetTempFilenameSafe(".wav");

            try
            {
                FormValueProvider formModel;
                using (var stream = System.IO.File.Create(sTempLocalFileName))
                {
                    formModel = await Request.StreamFile(stream);
                }
                // Return the record.
                _audioconversionservice.ConvertMP3ToWav(sTempLocalFileName, sTempFinalLocalFileName, sampleRate);

                return new TempPhysicalFileResult(sTempFinalLocalFileName, "audio/wav") { FileDownloadName = Path.GetFileName(sTempFinalLocalFileName) };
            }
            finally
            {
                // Delete the initial streamed file
                if (System.IO.File.Exists(sTempLocalFileName))
                    System.IO.File.Delete(sTempLocalFileName);
            }

        }

        /// <summary>
        /// Swaps the channels of a wav file
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sampleRate">The sample rate of the audio</param>
        /// <param name="contentType">The content type of the download. Values can be audio/mpeg, audio/mpeg3, audio/x-mpeg-3</param>
        /// <returns>A stream of the audio</returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>            
        /// <response code="500">Internal Server Error</response>            
        [HttpPost("swap-audio-channel")]
        [DisableFormValueModelBinding]
        [Consumes("multipart/form-data")] // You can specify consumes here and it gets automatically added also to swagger
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(Stream))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Stream))]
        public async Task<ActionResult> SwapAudioChannel(int sampleRate, string contentType)
        {
            // Specify paths.
            string sTempLocalFileName = Utilities.GetTempFilenameSafe(".wav");
            string sTempFinalLocalFileName = Utilities.GetTempFilenameSafe(".wav");

            try
            {
                FormValueProvider formModel;
                using (var stream = System.IO.File.Create(sTempLocalFileName))
                {
                    formModel = await Request.StreamFile(stream);
                }
                // Return the record.
                _audioconversionservice.SwapAudioChannel(sTempLocalFileName, sTempFinalLocalFileName, sampleRate);

                return new TempPhysicalFileResult(sTempFinalLocalFileName, contentType) { FileDownloadName = Path.GetFileName(sTempFinalLocalFileName) };
            }
            finally
            {
                // Delete the initial streamed file
                if (System.IO.File.Exists(sTempLocalFileName))
                    System.IO.File.Delete(sTempLocalFileName);
            }

        }

        /// <summary>
        /// Creates an mp3 file from a wav file
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sampleRate">The sample rate of the audio</param>
        /// <param name="contentType">The content type of the download. Values can be audio/mpeg, audio/mpeg3, audio/x-mpeg-3</param>
        /// <returns>A stream of the audio</returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>            
        /// <response code="500">Internal Server Error</response>            
        [HttpPost("create-mp3-from-wav")]
        [DisableFormValueModelBinding]
        [Consumes("multipart/form-data")] // You can specify consumes here and it gets automatically added also to swagger
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(Stream))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Stream))]
        public async Task<ActionResult> CreateMP3FromWav(int sampleRate, string contentType)
        {
            // Specify paths.
            string sTempLocalFileName = Utilities.GetTempFilenameSafe(".wav");
            string sTempFinalLocalFileName = Utilities.GetTempFilenameSafe(".mp3");

            try
            {
                FormValueProvider formModel;
                using (var stream = System.IO.File.Create(sTempLocalFileName))
                {
                    formModel = await Request.StreamFile(stream);
                }
                // Return the record.
                _audioconversionservice.CreateMP3FromWav(sTempLocalFileName, sTempFinalLocalFileName);

                return new TempPhysicalFileResult(sTempFinalLocalFileName, contentType) { FileDownloadName = Path.GetFileName(sTempFinalLocalFileName) };
            }
            finally
            {
                // Delete the initial streamed file
                if (System.IO.File.Exists(sTempLocalFileName))
                    System.IO.File.Delete(sTempLocalFileName);
            }

        }

        /// <summary>
        /// Redacts a wav file based on starttimeoffset and duration. If duration > call length, call length is selected.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="lStartTimeOffsetMilliseconds"></param>
        /// <param name="lRedactionLengthMilliseconds"></param>
        /// <returns>A stream of the audio</returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>            
        /// <response code="500">Internal Server Error</response>            
        [HttpPost("redact-wav")]
        [DisableFormValueModelBinding]
        [Consumes("multipart/form-data")] // You can specify consumes here and it gets automatically added also to swagger
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(Stream))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Stream))]
        public async Task<ActionResult> RedactWavAudio(long lStartTimeOffsetMilliseconds, long lRedactionLengthMilliseconds)
        {
            // Specify paths.
            string sTempLocalFileName = Utilities.GetTempFilenameSafe(".wav");
            string sTempFinalLocalFileName = Utilities.GetTempFilenameSafe(".wav");

            try
            {
                FormValueProvider formModel;
                using (var stream = System.IO.File.Create(sTempLocalFileName))
                {
                    formModel = await Request.StreamFile(stream);
                }
                // Return the record.
                _audioconversionservice.RedactWavAudio(sTempLocalFileName, sTempFinalLocalFileName, lStartTimeOffsetMilliseconds, lRedactionLengthMilliseconds);

                return new TempPhysicalFileResult(sTempFinalLocalFileName, "audio/wav") { FileDownloadName = Path.GetFileName(sTempFinalLocalFileName) };
            }
            finally
            {
                // Delete the initial streamed file
                if (System.IO.File.Exists(sTempLocalFileName))
                    System.IO.File.Delete(sTempLocalFileName);
            }

        }

        /// <summary>
        /// Redact 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="leftChannel">What channel to extract. Values can be 1 </param>
        /// <returns>A stream of the audio</returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>            
        /// <response code="500">Internal Server Error</response>            
        [HttpPost("extract-one-channel-wav")]
        [DisableFormValueModelBinding]
        [Consumes("multipart/form-data")] // You can specify consumes here and it gets automatically added also to swagger
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(Stream))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Stream))]
        public async Task<ActionResult> ExtractOneChannelToWav(bool leftChannel)
        {
            // Specify paths.
            string sTempLocalFileName = Utilities.GetTempFilenameSafe(".mp3");
            string sTempFinalLocalFileName = Utilities.GetTempFilenameSafe(".wav");

            try
            {
                FormValueProvider formModel;
                using (var stream = System.IO.File.Create(sTempLocalFileName))
                {
                    formModel = await Request.StreamFile(stream);
                }
                // Return the record.
                _audioconversionservice.ExtractOneChannelToWav(sTempLocalFileName, sTempFinalLocalFileName, leftChannel);

                return new TempPhysicalFileResult(sTempFinalLocalFileName, "audio/wav") { FileDownloadName = Path.GetFileName(sTempFinalLocalFileName) };
            }
            finally
            {
                // Delete the initial streamed file
                if (System.IO.File.Exists(sTempLocalFileName))
                    System.IO.File.Delete(sTempLocalFileName);
            }

        }

        /// <summary>
        /// Converts an audio file from opus to wav
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sampleRate">The sample rate of the audio</param>
        /// <returns>A stream of the audio</returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>            
        /// <response code="500">Internal Server Error</response>            
        [HttpPost("convert-opus-wav")]
        [DisableFormValueModelBinding]
        [Consumes("multipart/form-data")] // You can specify consumes here and it gets automatically added also to swagger
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(Stream))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Stream))]
        public async Task<ActionResult> ConvertOpusToWav(int sampleRate)
        {
            // Specify paths.
            string sTempLocalFileName = Utilities.GetTempFilenameSafe(".opus");
            string sTempFinalLocalFileName = Utilities.GetTempFilenameSafe(".wav");

            try
            {
                FormValueProvider formModel;
                using (var stream = System.IO.File.Create(sTempLocalFileName))
                {
                    formModel = await Request.StreamFile(stream);
                }
                // Return the record.
                _audioconversionservice.ConvertOpusToWav(sTempLocalFileName, sTempFinalLocalFileName, sampleRate);

                return new TempPhysicalFileResult(sTempFinalLocalFileName, "audio/wav") { FileDownloadName = Path.GetFileName(sTempFinalLocalFileName) };
            }
            finally
            {
                // Delete the initial streamed file
                if (System.IO.File.Exists(sTempLocalFileName))
                    System.IO.File.Delete(sTempLocalFileName);
            }

        }
    }

}