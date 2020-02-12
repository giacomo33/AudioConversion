using AudioConversion.RESTApi.AudioConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.Models
{
   public class CallCreateModel_Validation
    {
        [Theory]
        [InlineData(2562)]
        [InlineData(1)]
        public void CallCreateModel_AccountID_Valid(int AccountId)
        {
            //arange
            var callcreate = new CallCreateModel() { accountid = AccountId };
            //act
            var results = TestModelHelper.Validate(callcreate);
            //assert
            Assert.Equal(0, results.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CallCreateModel_AccountID_InValid_NegativeAndZero(int AccountId)
        {
            //arange
            var callcreate = new CallCreateModel() { accountid = AccountId };
            //act
            var results = TestModelHelper.Validate(callcreate);
            //assert
            Assert.Equal(1, results.Count);
        }

        [Theory]
        [InlineData(1, 56)]
        [InlineData(1, 1)]
        public void CallCreateModel_DurationSeconds_Valid(int AccountId, int DurationSeconds)
        {
            //arange
            var callcreate = new CallCreateModel() {accountid=AccountId, durationseconds = DurationSeconds };
            //act
            var results = TestModelHelper.Validate(callcreate);
            //assert
            Assert.Equal(0, results.Count);
        }

        [Theory]
        [InlineData(0, -23)]
        [InlineData(23, -1)]
        public void CallCreateModel_DurationSeconds_InValid_NegativeAndZero(int AccountId, int DurationSeconds)
        {
            //arange
            var callcreate = new CallCreateModel() { accountid = AccountId, durationseconds = DurationSeconds };
            //act
            var results = TestModelHelper.Validate(callcreate);
            //assert
            Assert.NotEqual(0, results.Count);
        }
    }
}
