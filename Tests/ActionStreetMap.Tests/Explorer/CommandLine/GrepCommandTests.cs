using System.Collections.Generic;
using ActionStreetMap.Explorer.CommandLine;
using NUnit.Framework;

namespace ActionStreetMap.Tests.Explorer.CommandLine
{
    [TestFixture]
    public class GrepCommandTests
    {
        private GrepCommand _command;

        [SetUp]
        public void SetUp()
        {
            // ARRANGE
            _command = new GrepCommand();
            _command.Content = new List<string> 
                                {@"Please remedy my confusion 
                                   And thrust me back to the day 
                                   The silence of your seclusion
                                   Brings night into all you say",

                                 @"Pull me down again
                                   And guide me into pain",

                                 @"I'm counting nocturnal hours
                                   Drowned visions in haunted sleep
                                   Faint flickering of your powers
                                   Leaks out to show what you keep"};
        }

        [Test]
        public void CanDoSimpleSearchSuccess()
        {
            // ACT
            var result = _command.Execute(new string[]
            {
                @"/i", "/e:me"
            });

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("And thrust me back to the day"));
            Assert.IsTrue(result.Contains("And guide me into pain"));
        }

        [Test]
        public void CanDoSimpleSearchFailure()
        {
            // ACT
            var result = _command.Execute(new string[]
            {
                @"/i", "/e:asd"
            });

            // ASSERT
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("No matches found!"));
        }
    }
}
