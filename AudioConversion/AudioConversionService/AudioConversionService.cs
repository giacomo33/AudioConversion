using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.IO;
using AudioConversion.AudioConversionService;
using AudioConversions;
using System.Threading;
using System.Text;

namespace AudioConversion.RESTApi.AudioConversion
{
    public interface IAudioConversionService
    {
        void WavConverter(string InputFullFilename, string OutputFullFilename, string inputFormat, int sampleRate, int bitDepth, int channels);
        void ConvertMP3ToWav(string InputFullFilename, string OutputFullFilename, int sampleRate);
        void ConvertOpusToWav(string InputFullFilename, string OutputFullFilename, int sampleRate);
        void SwapAudioChannel(string InputFullFilename, string OutputFullFilename, int sampleRate);
        void ExtractOneChannelToWav(string InputFullFilename, string OutputFullFilename, bool leftChannel);
        void CreateMP3FromWav(string InputFullFilename, string OutputFullFilename);
        void RedactWavAudio(string InputFullFilename, string OutputFullFilename, long lStartTimeOffsetMilliseconds, long lRedactionLengthMilliseconds);
        void WavFileExtract(string InputFullFilename, string OutputFullFilename, TimeSpan tStartTimeOffset, TimeSpan tDuration);
        void CreateStereoWavFile(string LeftChannelFilename, string RightChannelFilename, string OutputFullFilename, bool AlsoTrimSilence);
        void ConvertWavToFlac(string InputFullFilename, string OutputFullFilename, int channelCount);
    }

    public class AudioConversionService : IAudioConversionService
    {
        private readonly ILogger<Program> _logger = null;
        public readonly string CURRENTAPPLICATIONPATH = AppDomain.CurrentDomain.BaseDirectory;
        private bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        public AudioConversionService(ILogger<Program> logger, IConfiguration configuration)
        {
            _logger = logger;
        }

        /// <summary>Converts the m p3 to wav.</summary>
        /// <param name="InputFullFilename">The input full filename.</param>
        /// <param name="OutputFullFilename">The output full filename.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <exception cref="Exception">output file '" + OutputFullFilename + "' was not created successfully</exception>
        public void ConvertMP3ToWav(string InputFullFilename, string OutputFullFilename, int sampleRate)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");


                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                // Setup Arguments
                arguments = " -r " + sampleRate + " -q -w \"" + OutputFullFilename + "\" \"" + InputFullFilename + "\"";

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("mpg123", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\mpg123.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"ConvertMP3ToWav Exception {Ex.Message}");
                throw;
            }
        }

        public void SwapAudioChannel(string InputFullFilename, string OutputFullFilename, int sampleRate)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");

                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                // Setup Arguments
                arguments = " \"" + InputFullFilename + "\" \"" + OutputFullFilename + "\" swap";

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"ConvertMP3ToWav Exception {Ex.Message}");
                throw;
            }
        }

        public void CreateMP3FromWav(string InputFullFilename, string OutputFullFilename)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");

                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                // Setup Arguments
                arguments = "--cbr -b 24 -h --quiet --strictly-enforce-ISO  --tt CallAudio --ta www.calln.com --tg 101 --ty " + DateTime.UtcNow.Year.ToString() + " \"" + InputFullFilename + "\" \"" + OutputFullFilename + "\"";

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("lame ", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\lame.exe ", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"ConvertMP3ToWav Exception {Ex.Message}");
                throw;
            }
        }

        public void WavConverter(string InputFullFilename, string OutputFullFilename, string inputFormat, int sampleRate, int bitDepth, int channels)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");

                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                // Setup arguments
                arguments = " -t " + inputFormat + "  " + InputFullFilename + " -r " + sampleRate + " -b " + bitDepth + " -c " + channels + " -t wav " + OutputFullFilename;

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"ConvertMP3ToWav Exception {Ex.Message}");
                throw;
            }
        }

        /// <summary>Extracts the one channel to wav.</summary>
        /// <param name="InputFullFilename">The input mp3 full filename.</param>
        /// <param name="OutputFullFilename">The output wav full filename.</param>
        /// <param name="leftChannel">if set to <c>true</c> [left channel].</param>
        /// <exception cref="Exception">input file '" + InputFullFilename + "' was not created successfully
        /// or
        /// output file '" + OutputFullFilename + "' was not created successfully</exception>
        public void ExtractOneChannelToWav(string InputFullFilename, string OutputFullFilename, bool leftChannel)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");


                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                // Setup Arguments
                if (leftChannel)
                {
                    arguments = " -q -0 -w \"" + OutputFullFilename + "\" \"" + InputFullFilename + "\"";
                }
                else
                {
                    arguments = " -q -1 -w \"" + OutputFullFilename + "\" \"" + InputFullFilename + "\"";
                }

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("mpg123", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\mpg123.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"ExtractOneChannelToWav Exception {Ex.Message}");
                throw;
            }
        }

        /// <summary>Redacts the audio.</summary>
        /// <param name="InputFullFilename">The input full filename.</param>
        /// <param name="OutputFullFilename">The output full filename.</param>
        /// <param name="lStartTimeOffsetMilliseconds">The l start time offset milliseconds.</param>
        /// <param name="lRedactionLengthMilliseconds">The l redaction length milliseconds.</param>
        /// <exception cref="Exception">
        /// input file '" + InputFullFilename + "' was not created successfully
        /// or
        /// Start Time Offset is greater than the Call Duration
        /// or
        /// output file '" + OutputFullFilename + "' was not created successfully
        /// </exception>
        public void RedactWavAudio(string InputFullFilename, string OutputFullFilename, long lStartTimeOffsetMilliseconds, long lRedactionLengthMilliseconds)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;
            string sTempStartFile = Utilities.GetTempFilenameSafe(".wav");
            string sTempEndFile = Utilities.GetTempFilenameSafe(".wav");
            string sBeepFile = Utilities.GetTempFilenameSafe(".wav");
            string sTempFinalLocalFileName = Utilities.GetTempFilenameSafe(".wav");

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");

                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                //Convert incoming file to sample rate
                this.WavConverter(InputFullFilename, sTempFinalLocalFileName, "wav", 8000, 16, 2);

                // Check sTempFinalLocalFileName file exists
                if (System.IO.File.Exists(sTempFinalLocalFileName) == false)
                    throw new Exception("sTempFinalLocalFileName file '" + sTempFinalLocalFileName + "' was not created successfully");

                var oHeader = new WavHeaderClass();
                // Read the header of the input file. So we can check if it is in stereo format.
                oHeader.Load(sTempFinalLocalFileName);

                // Check Call Duration
                int iCallDurationMilliseconds = oHeader.GetDurationInMilliseconds();

                // Add extra 10 milliseconds to redaction start offset to bring offset to the start of the next word
                lStartTimeOffsetMilliseconds += 10;

                // Check StartTimeOffset
                if (lStartTimeOffsetMilliseconds > iCallDurationMilliseconds)
                {
                    throw new Exception("Start Time Offset is greater than the Call Duration");
                }

                if (lStartTimeOffsetMilliseconds + lRedactionLengthMilliseconds > iCallDurationMilliseconds)
                {
                    lRedactionLengthMilliseconds = iCallDurationMilliseconds - lStartTimeOffsetMilliseconds;
                }

                // Setup arguments to create beep file
                arguments = " -r 8000 -c 2 -b 16 -n " + sBeepFile + " synth " + Convert.ToDecimal(lRedactionLengthMilliseconds / 1000.00).ToString("######.000") + " sine 270.0";

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }

                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(sBeepFile) == false)
                    throw new Exception("BeepFile file '" + sBeepFile + "' was not created successfully");

                // Setup arguments to create start file
                arguments = "\"" + sTempFinalLocalFileName + "\" \"" + sTempStartFile + "\" trim 0 " + Convert.ToDecimal(lStartTimeOffsetMilliseconds / 1000.00).ToString("######.000");

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }

                // Setup arguments to create end file
                arguments = "\"" + sTempFinalLocalFileName + "\" \"" + sTempEndFile + "\" trim " + Convert.ToDecimal(lStartTimeOffsetMilliseconds / (double)1000 + lRedactionLengthMilliseconds / (double)1000).ToString("######.000") + " =" + Convert.ToDecimal(iCallDurationMilliseconds / (double)1000).ToString("######.000");

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }

                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(sTempEndFile) == false)
                    throw new Exception("TempEndFile file '" + sTempEndFile + "' was not created successfully");

                // Setup arguments to combine the 3 files
                arguments = "\"" + sTempStartFile + "\" \"" + sBeepFile + "\" \"" + sTempEndFile + "\"  \"" + OutputFullFilename + "\"";

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"RedactAudio .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"RedactAudio Exception {Ex.Message}");
                throw;
            }
            finally
            {
                // Delete any temporary file.
                if (sTempStartFile.Length > 0 && System.IO.File.Exists(sTempStartFile) == true)
                {
                    try
                    {
                        System.IO.File.Delete(sTempStartFile);
                    }
                    catch (Exception ex)
                    {
                        // Ignore any error.
                    }
                }

                if (sTempEndFile.Length > 0 && System.IO.File.Exists(sTempEndFile) == true)
                {
                    try
                    {
                        System.IO.File.Delete(sTempEndFile);
                    }
                    catch (Exception ex)
                    {
                        // Ignore any error.
                    }
                }

                if (sBeepFile.Length > 0 && System.IO.File.Exists(sBeepFile) == true)
                {
                    try
                    {
                        System.IO.File.Delete(sBeepFile);
                    }
                    catch (Exception ex)
                    {
                        // Ignore any error.
                    }
                }

                if (sTempFinalLocalFileName.Length > 0 && System.IO.File.Exists(sTempFinalLocalFileName) == true)
                {
                    try
                    {
                        System.IO.File.Delete(sTempFinalLocalFileName);
                    }
                    catch (Exception ex)
                    {
                        // Ignore any error.
                    }
                }
            }
        }

        /// <summary>Wavs the file extract.</summary>
        /// <param name="InputFullFilename">The input full filename.</param>
        /// <param name="OutputFullFilename">The output full filename.</param>
        /// <param name="tStartTimeOffset">The t start time offset.</param>
        /// <param name="tDuration">Duration of the t.</param>
        /// <exception cref="Exception">
        /// input file '" + InputFullFilename + "' was not created successfully
        /// or
        /// Start Time is greater than the Call Duration
        /// or
        /// output file '" + OutputFullFilename + "' was not created successfully
        /// </exception>
        public void WavFileExtract(string InputFullFilename, string OutputFullFilename, TimeSpan tStartTimeOffset, TimeSpan tDuration)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");

                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                var oHeader = new WavHeaderClass();
                // Read the header of the input file. So we can check if it is in stereo format.
                oHeader.Load(InputFullFilename);

                // Check Call Duration
                int iCallDurationMilliseconds = oHeader.GetDurationInMilliseconds();

                // Check StartTime
                if (tStartTimeOffset.TotalMilliseconds > iCallDurationMilliseconds)
                {
                    throw new Exception("Start Time is greater than the Call Duration");
                }

                // Check Duration
                if (tDuration.TotalMilliseconds > iCallDurationMilliseconds - tStartTimeOffset.TotalMilliseconds)
                {
                    tDuration = TimeSpan.FromMilliseconds(iCallDurationMilliseconds - tStartTimeOffset.TotalMilliseconds);
                }


                // Setup arguments to create beep file
                arguments = "\"" + InputFullFilename + "\" \"" + OutputFullFilename + "\" trim " + Convert.ToDecimal(tStartTimeOffset.TotalMilliseconds / 1000).ToString("######.000") + " " + Convert.ToDecimal(tDuration.TotalMilliseconds / 1000).ToString("######.000");

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }

                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"WavFileExtract .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"WavFileExtract Exception {Ex.Message}");
                throw;
            }
        }

        /// <summary>Creates the stereo wav file.</summary>
        /// <param name="LeftChannelFilename">The left channel filename.</param>
        /// <param name="RightChannelFilename">The right channel filename.</param>
        /// <param name="OutputFullFilename">The output full filename.</param>
        /// <param name="AlsoTrimSilence">if set to <c>true</c> [also trim silence].</param>
        /// <exception cref="Exception">
        /// left input file '" + LeftChannelFilename + "' was not created successfully
        /// or
        /// right input file '" + RightChannelFilename + "' was not created successfully
        /// or
        /// output file '" + OutputFullFilename + "' was not created successfully
        /// </exception>
        public void CreateStereoWavFile(string LeftChannelFilename, string RightChannelFilename, string OutputFullFilename, bool AlsoTrimSilence)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(LeftChannelFilename) == false)
                    throw new Exception("left input file '" + LeftChannelFilename + "' was not created successfully");

                if (System.IO.File.Exists(RightChannelFilename) == false)
                    throw new Exception("right input file '" + RightChannelFilename + "' was not created successfully");

                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                // Build up the command line arguments.
                arguments = " -M \"" + LeftChannelFilename + "\" \"" + RightChannelFilename + "\" -c 2 \"" + OutputFullFilename + "\"";
                if (AlsoTrimSilence == true)
                    arguments += " silence 1 0 -40db";

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }

                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"CreateStereoWavFile .Converted files successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"CreateStereoWavFile Exception {Ex.Message}");
                throw;
            }
        }

        public void ConvertOpusToWav(string InputFullFilename, string OutputFullFilename, int sampleRate)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {
                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");


                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                // Setup Arguments
                arguments = " --rate " + sampleRate + " --quiet \"" + InputFullFilename + "\" \"" + OutputFullFilename + "\"";

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("opusdec", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\opusdec.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"ConvertOpusToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"ConvertOpusToWav Exception {Ex.Message}");
                throw;
            }
        }

        public void ConvertWavToFlac(string InputFullFilename, string OutputFullFilename, int channelCount)
        {

            DateTime BeginTimeUTC = DateTime.UtcNow;
            string arguments = string.Empty;

            try
            {

                var oHeader = new WavHeaderClass();
                // Read the header of the input file. So we can check if it is in stereo format.
                oHeader.Load(InputFullFilename);

                // Check Input file exists
                if (System.IO.File.Exists(InputFullFilename) == false)
                    throw new Exception("input file '" + InputFullFilename + "' was not created successfully");

                // Delete any pre-existing output file.
                if (System.IO.File.Exists(OutputFullFilename) == true)
                {
                    System.IO.File.SetAttributes(OutputFullFilename, System.IO.FileAttributes.Normal);
                    System.IO.File.Delete(OutputFullFilename);
                }

                // Setup Arguments
                if (channelCount == 2)
                {
                    arguments = " -t wav \"" + InputFullFilename + "\" -t flac -r 8000 -b 16 -c 2  \"" + OutputFullFilename + "\"";
                }
                else if (channelCount == 1)
                {
                    arguments = " -t wav \"" + InputFullFilename + "\" -t flac -r 8000 -b 16 -c 1  \"" + OutputFullFilename + "\"";
                }
                else
                    throw new Exception("channel count can only be 1 or 2");

                //arguments = " -i \"" + InputFullFilename + "\"  \"" + OutputFullFilename + "\"";


                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments, _logger);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

                return;
            }
            catch (Exception Ex)
            {
                _logger.LogInformation($"ConvertMP3ToWav Exception {Ex.Message}");
                throw;
            }
        }
    }


}
