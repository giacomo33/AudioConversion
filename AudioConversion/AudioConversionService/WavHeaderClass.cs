using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic;

namespace AudioConversion.AudioConversionService
{
    public partial class WavHeaderClass
    {
        // WAV File Specification
        // FROM http://ccrma.stanford.edu/courses/422/projects/WaveFormat/
        // The canonical WAVE format starts with the RIFF header:
        // 0         4   ChunkID          Contains the letters "RIFF" in ASCII form
        // (0x52494646 big-endian form).
        // 4         4   ChunkSize        36 + SubChunk2Size, or more precisely:
        // 4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
        // This is the size of the rest of the chunk 
        // following this number.  This is the size of the 
        // entire file in bytes minus 8 bytes for the
        // two fields not included in this count:
        // ChunkID and ChunkSize.
        // 8         4   Format           Contains the letters "WAVE"
        // (0x57415645 big-endian form).

        // The "WAVE" format consists of two subchunks: "fmt " and "data":
        // The "fmt " subchunk describes the sound data's format:
        // 12        4   Subchunk1ID      Contains the letters "fmt "
        // (0x666d7420 big-endian form).
        // 16        4   Subchunk1Size    16 for PCM.  This is the size of the
        // rest of the Subchunk which follows this number.
        // 20        2   AudioFormat      PCM = 1 (i.e. Linear quantization)
        // Values other than 1 indicate some 
        // form of compression.
        // 22        2   NumChannels      Mono = 1, Stereo = 2, etc.
        // 24        4   SampleRate       8000, 44100, etc.
        // 28        4   ByteRate         == SampleRate * NumChannels * BitsPerSample/8
        // 32        2   BlockAlign       == NumChannels * BitsPerSample/8
        // The number of bytes for one sample including
        // all channels. I wonder what happens when
        // this number isn't an integer?
        // 34        2   BitsPerSample    8 bits = 8, 16 bits = 16, etc.

        // The "data" subchunk contains the size of the data and the actual sound:
        // 36        4   Subchunk2ID      Contains the letters "data"
        // (0x64617461 big-endian form).
        // 40        4   Subchunk2Size    == NumSamples * NumChannels * BitsPerSample/8
        // This is the number of bytes in the data.
        // You can also think of this as the size
        // of the read of the subchunk following this 
        // number.
        // 44        *   Data             The actual sound data.

        private short myFormat = Conversions.ToShort(0);
        private short myChannels = Conversions.ToShort(0);
        private int mySampleRate = 0;
        private int myByteRate = 0;
        private short myBlockAlign = Conversions.ToShort(0);
        private short myBitsPerSample = Conversions.ToShort(0);
        private int myDataSize = 0;
        private byte[] myHeader = null;
        private byte[] myData = null;

        public short Format
        {
            get
            {
                return myFormat;
            }
            set
            {
                myFormat = value;
            }
        }

        public short Channels
        {
            get
            {
                return myChannels;
            }
            set
            {
                myChannels = value;
            }
        }

        public int SampleRate
        {
            get
            {
                return mySampleRate;
            }
            set
            {
                mySampleRate = value;
            }
        }

        public int ByteRate
        {
            get
            {
                return myByteRate;
            }
            set
            {
                myByteRate = value;
            }
        }

        public short BlockAlign
        {
            get
            {
                return myBlockAlign;
            }
            set
            {
                myBlockAlign = value;
            }
        }

        public short BitsPerSample
        {
            get
            {
                return myBitsPerSample;
            }
            set
            {
                myBitsPerSample = value;
            }
        }

        public int DataSize
        {
            get
            {
                return myDataSize;
            }
        }

        public byte[] Data
        {
            get
            {
                return myData;
            }
            set
            {
                myData = value;
                myDataSize = myData.Length;
            }
        }

        public WavHeaderClass()
        {
            return;
        }

        public WavHeaderClass(string sFullFilename)
        {
            Load(sFullFilename);
            return;
        }

        // ------------------------------------------------------------------------------
        // FUNCTION : Load
        // 
        // Load a wav file into this class.
        // 
        // PARAMETERS:
        // sFilename   [input]  -  The wav file. e.g. 'c:\input.alaw'.
        // 
        // RETURN VALUES:
        // Nothing.
        // ------------------------------------------------------------------------------
        public void Load(string sFilename)
        {
            try
            {
                // Make sure we have a path.
                if (sFilename.Length == 0)
                    throw new Exception("filename must be provided");

                // Make sure that path is valid.
                if (System.IO.File.Exists(sFilename) == false)
                    throw new Exception("file doesnt exist");

                // Open the file.
                Read(System.IO.File.ReadAllBytes(sFilename));

                // Success.
                return;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // ------------------------------------------------------------------------------
        // FUNCTION : Read
        // 
        // Read a wav file into this class.
        // 
        // PARAMETERS:
        // sFilename   [input]  -  The wav file. e.g. 'c:\input.alaw'.
        // 
        // RETURN VALUES:
        // Nothing.
        // ------------------------------------------------------------------------------
        public void Read(byte[] bData)
        {
            try
            {
                // Make sure the file is big enough to at least have the appropriate header.
                long fileSize = bData.Length;
                if (fileSize < 44)
                    throw new Exception("header is not 44 bytes");

                // First, read the header.
                var header = new byte[44]; // the header holds 44 bytes, so dim the array from 0 to 43
                Array.Copy(bData, 0, header, 0, 44);
                myHeader = header;
                myFormat = BitConverter.ToInt16(header, 20);
                myChannels = BitConverter.ToInt16(header, 22);
                mySampleRate = BitConverter.ToInt32(header, 24);
                myByteRate = BitConverter.ToInt32(header, 28);
                myBlockAlign = BitConverter.ToInt16(header, 32);
                myBitsPerSample = BitConverter.ToInt16(header, 34);
                myDataSize = BitConverter.ToInt32(header, 40);

                // Success.
                return;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // ------------------------------------------------------------------------------
        // FUNCTION : ToString
        // 
        // Generate a description of the last read wav header.
        // 
        // PARAMETERS:
        // Nothing.
        // 
        // RETURN VALUES:
        // A description.
        // ------------------------------------------------------------------------------
        public new string ToString()
        {
            string output = string.Empty;
            var enc = new UTF8Encoding();

            output = output + "0 ChunkID:" + enc.GetString(myHeader, 0, 4) + Constants.vbCrLf;
            output = output + "4 ChunkSize:" + Conversions.ToString(BitConverter.ToInt32(myHeader, 4)) + Constants.vbCrLf;
            output = output + "8 Format:" + enc.GetString(myHeader, 8, 4) + Constants.vbCrLf;
            output = output + "12 SubChunk1ID:" + enc.GetString(myHeader, 12, 4) + Constants.vbCrLf;
            output = output + "16 SubChunk1Size:" + Conversions.ToString(BitConverter.ToInt32(myHeader, 16)) + Constants.vbCrLf;
            output = output + "20 Format: " + Conversions.ToString(myFormat) + Constants.vbCrLf;
            output = output + "22 Channels: " + Conversions.ToString(myChannels) + Constants.vbCrLf;
            output = output + "24 SampleRate: " + Conversions.ToString(mySampleRate) + Constants.vbCrLf;
            output = output + "28 ByteRate: " + Conversions.ToString(myByteRate) + Constants.vbCrLf;
            output = output + "32 BlockAlign: " + Conversions.ToString(myBlockAlign) + Constants.vbCrLf;
            output = output + "34 BitsPerSample: " + Conversions.ToString(myBitsPerSample) + Constants.vbCrLf;
            output = output + "36 SubChunk2ID:" + enc.GetString(myHeader, 36, 4) + Constants.vbCrLf;
            output = output + "40 Data Size: " + Conversions.ToString(myDataSize) + Constants.vbCrLf;

            return output;
        }

        // -------------------------------------------------------------------------------------------------------
        // GetDuration() as Integer : Returns the duration of the current file in seconds.
        // -------------------------------------------------------------------------------------------------------
        public int GetDurationInSeconds()
        {

            // Body Length / Channels / Single Sample Bytes / SampleRate
            int Duration = Conversions.ToInteger(Math.Round(myDataSize / (double)Channels / (myBitsPerSample / (double)8) / mySampleRate, 0));

            return Duration;
        }


        // -------------------------------------------------------------------------------------------------------
        // GetDuration() as Integer : Returns the duration of the current file in seconds.
        // -------------------------------------------------------------------------------------------------------
        public int GetDurationInMilliseconds()
        {

            // Body Length / Channels / Single Sample Bytes / SampleRate
            int Duration = Conversions.ToInteger(Math.Round(myDataSize / (double)Channels / (myBitsPerSample / (double)8) / mySampleRate, 3)) * 1000;

            return Duration;
        }
    }
}

