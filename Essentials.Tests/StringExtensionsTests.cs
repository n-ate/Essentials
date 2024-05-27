namespace n_ate.Essentials.Tests
{
    public class StringExtensionsTests
    {
        [Test]
        public void CamelCaseToFriendly_A_Center()
        {
            //Assume//

            //Arrange//
            var input = "WordAWord";
            var expected = "Word A Word";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \"{expected}\"\n  Actual: \"{actual}\"");
        }

        [Test]
        public void CamelCaseToFriendly_A_Center_Spaced()
        {
            //Assume//

            //Arrange//
            var input = "Word A Word";
            var expected = "Word A Word";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \"{expected}\"\n    Actual: \"{actual}\"");
        }

        [Test]
        public void CamelCaseToFriendly_A_End()
        {
            //Assume//

            //Arrange//
            var input = "WordyWordA";
            var expected = "Wordy Word A";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \" {expected} \"\n    Actual: \" {actual} \"");
        }

        [Test]
        public void CamelCaseToFriendly_A_End_Spaced()
        {
            //Assume//

            //Arrange//
            var input = "Wordy Word A";
            var expected = "Wordy Word A";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \" {expected} \"\n    Actual: \" {actual} \"");
        }

        [Test]
        public void CamelCaseToFriendly_A_Start()
        {
            //Assume//

            //Arrange//
            var input = "AWordyWord";
            var expected = "A Wordy Word";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \" {expected} \"\n    Actual: \" {actual} \"");
        }

        [Test]
        public void CamelCaseToFriendly_A_Start_Spaced()
        {
            //Assume//

            //Arrange//
            var input = "A Wordy Word";
            var expected = "A Wordy Word";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \" {expected} \"\n    Actual: \" {actual} \"");
        }

        [Test]
        public void CamelCaseToFriendly_Acronym_Center()
        {
            //Assume//

            //Arrange//
            var input = "WordACRONYMWord";
            var expected = "Word ACRONYM Word";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \"{expected}\"\n  Actual: \"{actual}\"");
        }

        [Test]
        public void CamelCaseToFriendly_Acronym_Center_Spaced()
        {
            //Assume//

            //Arrange//
            var input = "Word ACRONYM Word";
            var expected = "Word ACRONYM Word";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \"{expected}\"\n    Actual: \"{actual}\"");
        }

        [Test]
        public void CamelCaseToFriendly_Acronym_End()
        {
            //Assume//

            //Arrange//
            var input = "WordyWordACRONYM";
            var expected = "Wordy Word ACRONYM";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \" {expected} \"\n    Actual: \" {actual} \"");
        }

        [Test]
        public void CamelCaseToFriendly_Acronym_End_Spaced()
        {
            //Assume//

            //Arrange//
            var input = "Wordy Word ACRONYM";
            var expected = "Wordy Word ACRONYM";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \" {expected} \"\n    Actual: \" {actual} \"");
        }

        [Test]
        public void CamelCaseToFriendly_Acronym_Start()
        {
            //Assume//

            //Arrange//
            var input = "ACRONYMWordyWord";
            var expected = "ACRONYM Wordy Word";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \" {expected} \"\n    Actual: \" {actual} \"");
        }

        [Test]
        public void CamelCaseToFriendly_Acronym_Start_Spaced()
        {
            //Assume//

            //Arrange//
            var input = "ACRONYM Wordy Word";
            var expected = "ACRONYM Wordy Word";

            //Act//
            var actual = input.CamelCaseToFriendly();

            //Assert//
            Assert.That(Equals(expected, actual), () => $"Expected: \" {expected} \"\n    Actual: \" {actual} \"");
        }
    }
}