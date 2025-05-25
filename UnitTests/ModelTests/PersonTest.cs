using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using tamb.Models;
using Xunit;

namespace UnitTests.ModelTests
{
    public class PersonTest
    {
        [Fact]
        public void Person_Defaults_AreSet()
        {
            var person = new Person
            {
                ImePrezime = "Ivan Ivić",
                Email = "ivan@tamb.com"
            };

            Assert.Equal("Ivan Ivić", person.ImePrezime);
            Assert.Empty(person.PlaysInstruments);
            Assert.Empty(person.Rezervacije);
        }

        [Fact]
        public void Person_Requires_ImePrezime()
        {
            var person = new Person
            {
                ImePrezime = "" 
            };

            var context = new ValidationContext(person);
            var results = new List<ValidationResult>();

            bool valid = Validator.TryValidateObject(person, context, results, true);

            Assert.Contains(results, r => r.MemberNames.Contains("ImePrezime"));
        }
    }
}
