using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using tamb.Models;
using Xunit;

namespace UnitTests.ModelTests
{
    public class SheetMusicTests
    {
        [Fact]
        public void SheetMusic_ValidatesRequiredFields()
        {
            var sheetMusic = new SheetMusic
            {
                Title = null,
                FileName = null
            };

            var context = new ValidationContext(sheetMusic);
            var results = new List<ValidationResult>();

            bool valid = Validator.TryValidateObject(sheetMusic, context, results, true);

            Assert.False(valid);
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));
            Assert.Contains(results, r => r.MemberNames.Contains("FileName"));
        }

        [Fact]
        public void SheetMusic_CanBeCreated()
        {
            var sheet = new SheetMusic
            {
                Title = "Mamica su štrukle pekli",
                FileName = "mamica_su_strukle_pekli.pdf",
                DateAdded = DateTime.Now
            };

            Assert.Equal("Mamica su štrukle pekli", sheet.Title);
            Assert.Equal("mamica_su_strukle_pekli.pdf", sheet.FileName);
        }
    }
}
