using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using AudioConversions;
using Microsoft.Extensions.Logging;

namespace AudioConversion.AudioConversionService
{
    public static class ShellHelper
    {
        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
        }

        public static void ExecBashProcess(string cmd, string arguments, ILogger<Program> _logger)
        {
            System.IO.StreamReader sOut = null;
            System.IO.StreamReader sError = null;
            cmd = cmd + arguments;
            var escapedArgs = cmd.Replace("\"", "\\\"");

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\""
                    }
                };

                process.Start();

                // Attach the output for reading.
                sOut = process.StandardOutput;

                // Attach the error for reading.
                sError = process.StandardError;

                process.WaitForExit();

                // Close the process.
                process.Close();

                // Write the output stream to the log.
                while (sOut.EndOfStream == false)
                {
                    string sNextLine = string.Empty;

                    sNextLine = sOut.ReadLine();
                }

                // Write the error stream to the log.
                while (sError.EndOfStream == false)
                {
                    string sNextLine = string.Empty;

                    sNextLine = sError.ReadLine();

                    _logger.LogError($"ExecBashProcess Audio Conversion Executable {cmd} error: {sNextLine}");

                    if (sNextLine.Contains("RIFF header not found") == true)
                        throw new Exception("invalid input file, RIFF header not found");
                }
            }
            catch (Exception Ex)
            {
                throw new Exception("ExecBashProcess: unable to convert audio file with '" + cmd + ", " + Ex.Message + "");
            }
        }

        public static void ExecWindowsProcess(string processFileName, string arguments)
        {
            System.IO.StreamReader sOut = null;
            System.IO.StreamReader sError = null;

            // Make sure the decoder utility is available.
            if (System.IO.File.Exists(processFileName) == false)
                throw new Exception("unable to find the '" + processFileName + "' utility");

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = processFileName,
                        WorkingDirectory = System.IO.Path.GetDirectoryName(processFileName),
                        UseShellExecute = false,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        RedirectStandardInput = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Arguments = arguments
                    }
                };

                process.Start();


                // Attach the output for reading.
                sOut = process.StandardOutput;

                // Attach the error for reading.
                sError = process.StandardError;

                process.WaitForExit();

                // Close the process.
                process.Close();

                // Write the output stream to the log.
                while (sOut.EndOfStream == false)
                {
                    string sNextLine = string.Empty;

                    sNextLine = sOut.ReadLine();
                }

                // Write the error stream to the log.
                while (sError.EndOfStream == false)
                {
                    string sNextLine = string.Empty;

                    sNextLine = sError.ReadLine();

                    if (sNextLine.Contains("RIFF header not found") == true)
                        throw new Exception("invalid input file, RIFF header not found");
                }

                // Close the Io Streams.
                sOut.Close();
                sError.Close();


            }
            catch (Exception Ex)
            {
                throw new Exception("ExecWindowsProcess: unable to convert audio file with '" + processFileName + " " + arguments + "', " + Ex.Message + "");
            }
        }


    }
}
