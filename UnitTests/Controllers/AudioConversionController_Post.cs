using AudioConversion.AudioConversionService;
using AudioConversion.BackgroundServices;
using AudioConversion.EventBusPublishers;
using AudioConversion.RESTApi;
using AudioConversion.RESTApi.AudioConversion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnitTests;
using Xunit;

namespace UnitTests.Controllers
{
    public class AudioConversionController_Post : ControllerTesting<AudioConversionController>
    {

        public AudioConversionController_Post()
        {
            // Don't want to save changes to the database and therefore will get instanced contexts rather than static contexts.
            //this.StaticContext = false;
        }

        protected override AudioConversionController CreateController()
        {
            //Create mock for ILogger.
            var loggerMock = new Mock<ILogger<AudioConversions.Program>>();   

            // Create mock for IEventBusPublisher.
            var eventbuspublisherMock = new Mock<IEventBusPublisher>();

            // Create mock for IEventBusPublisher.
            var eventpublisherervice = new Mock<IEventBusPublisher>();

            var hostEnvironment = new Mock<IHostEnvironment>();
            hostEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");

            // Create mock for IConfiguration
            var configurationMock = new Mock<IConfiguration>();

            // Create mock for IProcess.
            var processMock = new Mock<AudioConversionService>(loggerMock.Object, configurationMock.Object);

            return new AudioConversionController(loggerMock.Object, processMock.Object);
         
        }

        [Fact]
        public async void  AudioConversion_Post_Wav_ReturnsMP3()
        {
            //arange
            var AudioConversionController = this.Controller;
            string audioFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"TestAudio\1.wav");
            var controllerContext =  ControllerContextHelper.Create(audioFilePath);
            AudioConversionController.ControllerContext = controllerContext;

            //act
            TempPhysicalFileResult response =   (TempPhysicalFileResult)await AudioConversionController.CreateMP3FromWav(8000, "audio/wav");

            //assert
            Assert.IsType<TempPhysicalFileResult>(response);
            Assert.True(response.FileName.Contains(".mp3") == true);
        }

        [Fact]
        public async void AudioConversion_Post_Ogg_ReturnsWav()
        {
            //arange
            var AudioConversionController = this.Controller;
            string audioFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"TestAudio\2.ogg");
            var controllerContext = ControllerContextHelper.Create(audioFilePath);
            AudioConversionController.ControllerContext = controllerContext;

            //act
            TempPhysicalFileResult response = (TempPhysicalFileResult)await AudioConversionController.WavConverter("ogg",8000, 16, 2);

            //assert
            Assert.IsType<TempPhysicalFileResult>(response);
            Assert.True(response.FileName.Contains(".wav") == true);
        }

        [Fact]
        public async void AudioConversion_Post_Wav8000_ReturnsWav16000()
        {
            //arange
            var AudioConversionController = this.Controller;
            string audioFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"TestAudio\1.wav");
            var controllerContext = ControllerContextHelper.Create(audioFilePath);
            AudioConversionController.ControllerContext = controllerContext;

            //act
            TempPhysicalFileResult response = (TempPhysicalFileResult)await AudioConversionController.WavConverter("wav", 16000, 16, 2);

            //assert
            Assert.IsType<TempPhysicalFileResult>(response);
            Assert.True(response.FileName.Contains(".wav") == true);
        }

        [Fact]
        public async void AudioConversion_Post_MP3_ReturnsWav()
        {
            //arange
            var AudioConversionController = this.Controller;
            string audioFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"TestAudio\3.mp3");
            var controllerContext = ControllerContextHelper.Create(audioFilePath);
            AudioConversionController.ControllerContext = controllerContext;

            //act
            TempPhysicalFileResult response = (TempPhysicalFileResult)await AudioConversionController.ConvertMP3ToWav(8000);

            //assert
            Assert.IsType<TempPhysicalFileResult>(response);
            Assert.True(response.FileName.Contains(".wav") == true);
        }

        [Fact]
        public async void AudioConversion_Post_Wav_ReturnsRedactedWav()
        {
            //arange
            var AudioConversionController = this.Controller;
            string audioFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"TestAudio\1.wav");
            var controllerContext = ControllerContextHelper.Create(audioFilePath);
            AudioConversionController.ControllerContext = controllerContext;

            //// Read the header of the input file. So we can check duration
            //var oHeader = new WavHeaderClass();
            //oHeader.Load(audioFilePath);

            //// Call Duration
            //int iCallDurationMilliseconds = oHeader.GetDurationInMilliseconds();

            //act
            TempPhysicalFileResult response = (TempPhysicalFileResult)await AudioConversionController.RedactWavAudio(5000, 7000);

            //assert
            Assert.IsType<TempPhysicalFileResult>(response);
            Assert.True(response.FileName.Contains(".wav") == true);
        }

       

    }      
}


