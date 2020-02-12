using AudioConversion.RESTApi.AudioConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.Models
{
   public class CallModel_Validation
    {
        [Theory]
        [InlineData(1)]
        [InlineData(34)]
        public void CallModel_AccountID_Valid(int Id)
        {
            //arange
            var call = new CallModel() {id=Id};
            //act
            var results = TestModelHelper.Validate(call);
            //assert
            Assert.Equal(0, results.Count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-34)]
        public void CallModel_AccountID_ID_InValid_NegativeAndZero(int Id)
        {
            //arange
            var call = new CallModel() { id=Id};
            //act
            var results = TestModelHelper.Validate(call);
            //assert
            Assert.NotEqual(0, results.Count);
        }
    }
}
