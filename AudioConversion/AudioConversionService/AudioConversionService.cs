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
using AudioConversion.RESTApi.Client.Users;
using System.IO;
using AudioConversion.AudioConversionService;
using AudioConversions;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Threading;
using System.Text;

namespace AudioConversion.RESTApi.AudioConversion
{
    public interface IAudioConversionService
    {
        void WavConverter(string InputFullFilename, string OutputFullFilename, string inputFormat, int sampleRate, int bitDepth, int channels);
        void ConvertMP3ToWav(string InputFullFilename, string OutputFullFilename, int sampleRate);
        void SwapAudioChannel(string InputFullFilename, string OutputFullFilename, int sampleRate);
        void CreateMP3FromWav(string InputFullFilename, string OutputFullFilename);
    }

    public class AudioConversionService : IAudioConversionService
    {
        private readonly ILogger<Program> _logger = null;
        public readonly string CURRENTAPPLICATIONPATH = AppDomain.CurrentDomain.BaseDirectory;
        private bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

    public AudioConversionService(ILogger<Program> logger,  IConfiguration configuration, IReSTApiUsersService restapiusersservice)
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
                    ShellHelper.ExecBashProcess("mpg123", arguments);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\mpg123.exe", arguments);
                }
                

                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"AudioConversionService:ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

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
                arguments=" \"" + InputFullFilename + "\" \"" + OutputFullFilename + "\" swap";

                // Check if running in Docker Container
                if (InDocker)
                {
                    ShellHelper.ExecBashProcess("sox", arguments);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"AudioConversionService:ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

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
                        ShellHelper.ExecBashProcess("lame " ,arguments);                
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\lame.exe ", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"AudioConversionService:ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

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
                    ShellHelper.ExecBashProcess("sox" , arguments);
                }
                else
                {
                    ShellHelper.ExecWindowsProcess(CURRENTAPPLICATIONPATH + @"\Executables\sox.exe", arguments);
                }


                // Throw an exception, if no output file was created.
                if (System.IO.File.Exists(OutputFullFilename) == false)
                    throw new Exception("output file '" + OutputFullFilename + "' was not created successfully");

                // The audio file was converted successfully.
                _logger.LogInformation($"AudioConversionService:ConvertMP3ToWav .Converted file '" + InputFullFilename + "' to '" + OutputFullFilename + "' successfully. (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms)");

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
