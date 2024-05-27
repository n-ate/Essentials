namespace n_ate.Essentials.Tests
{
    public class FilesTests
    {
        [Test]
        public void FindMatchingAbsoluteFilePaths_FileName()
        {
            //Assume//

            //Arrange//
            var path = "to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths1_Linux()
        {
            //Assume//

            //Arrange//
            var path = "FilesTestsData/Directory/to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths1_Windows()
        {
            //Assume//

            //Arrange//
            var path = "FilesTestsData\\Directory\\to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths2_Linux()
        {
            //Assume//

            //Arrange//
            var path = "/FilesTestsData/Directory/to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths2_Windows()
        {
            //Assume//

            //Arrange//
            var path = "\\FilesTestsData\\Directory\\to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths3_Linux()
        {
            //Assume//

            //Arrange//
            var path = "Directory/to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths3_Windows()
        {
            //Assume//

            //Arrange//
            var path = "Directory\\to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths4_Linux()
        {
            //Assume//

            //Arrange//
            var path = "/Directory/to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths4_Windows()
        {
            //Assume//

            //Arrange//
            var path = "\\Directory\\to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths5_Linux()
        {
            //Assume//

            //Arrange//
            var path = "/to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }

        [Test]
        public void FindMatchingAbsoluteFilePaths5_Windows()
        {
            //Assume//

            //Arrange//
            var path = "\\to-find.txt";
            var expected = "\\FilesTestsData\\Directory\\to-find.txt".ToSingleItemArray();

            //Act//
            var actual = Files.FindMatchingAbsoluteFilePaths(path);

            //Assert//
            Assert.That(Equals(expected.Length, actual.Length), () => $"Expected length: \"{expected.Length}\"\n  Actual length: \"{actual.Length}\"");
            Assert.That(actual[0].EndsWith(expected[0]), () => $"Actual must end with expected. Expected: \"{expected[0]}\"\n  Actual: \"{actual[0]}\"");
        }
    }
}