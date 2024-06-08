using n_ate.Essentials.Serialization;
using System.Text.Json;

namespace n_ate.Essentials.Tests
{
    public class InterfaceAutoResolveConverterTests
    {
        internal interface INeedResolving
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public List<decimal> List { get; set; }
        }

        internal class MainClass
        {
            public INeedResolving Interface1 { get; set; }
            public INeedResolving Interface2 { get; set; }
            public INeedResolving Interface3 { get; set; }

            public INeedResolving[] InterfaceArray { get; set; }
        }

        internal class Class1 : INeedResolving
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public List<decimal> List { get; set; }

            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
        }

        internal class Class2 : INeedResolving
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public List<decimal> List { get; set; }
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }

        internal class Class3 : INeedResolving
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public List<decimal> List { get; set; }
            public string Property2 { get; set; }
            public string Property3 { get; set; }
        }

        [Test]
        public void DeserializeInterfaces()
        {
            //Assume//
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new InterfaceAutoResolveConverter(this.GetType().Assembly));
            var expected = new MainClass
            {
                Interface1 = new Class1 { Id = 1, Key = nameof(Class1), List = [0.1m, 0.2m, 0.3m], Property1 = nameof(Class1.Property1), Property2 = nameof(Class1.Property2), Property3 = nameof(Class1.Property3) },
                Interface2 = new Class2 { Id = 1, Key = nameof(Class1), List = [0.1m, 0.2m, 0.3m], Property1 = nameof(Class1.Property1), Property2 = nameof(Class1.Property2) /* no property 3 * * * * * * * * * */ },
                Interface3 = new Class3 { Id = 1, Key = nameof(Class1), List = [0.1m, 0.2m, 0.3m], /* no property 1 * * * * * * * * * */ Property2 = nameof(Class1.Property2), Property3 = nameof(Class1.Property3) },
            };
            expected.InterfaceArray = [expected.Interface1, expected.Interface2, expected.Interface3];

            //Arrange//
            var json = JsonSerializer.Serialize(expected, options);

            //Act//
            var actual = JsonSerializer.Deserialize<MainClass>(json, options);

            //Assert//
            Assert.That(actual.IsComparable(expected, out var message), message);
        }

        [Test]
        public void SerializeInterfaces()
        {
            //Assume//
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new InterfaceAutoResolveConverter(this.GetType().Assembly));
            var expected = "{\"Interface1\":{\"Id\":1,\"Key\":\"Class1\",\"List\":[0.1,0.2,0.3],\"Property1\":\"Property1\",\"Property2\":\"Property2\",\"Property3\":\"Property3\"},\"Interface2\":{\"Id\":1,\"Key\":\"Class1\",\"List\":[0.1,0.2,0.3],\"Property1\":\"Property1\",\"Property2\":\"Property2\"},\"Interface3\":{\"Id\":1,\"Key\":\"Class1\",\"List\":[0.1,0.2,0.3],\"Property2\":\"Property2\",\"Property3\":\"Property3\"},\"InterfaceArray\":[{\"Id\":1,\"Key\":\"Class1\",\"List\":[0.1,0.2,0.3],\"Property1\":\"Property1\",\"Property2\":\"Property2\",\"Property3\":\"Property3\"},{\"Id\":1,\"Key\":\"Class1\",\"List\":[0.1,0.2,0.3],\"Property1\":\"Property1\",\"Property2\":\"Property2\"},{\"Id\":1,\"Key\":\"Class1\",\"List\":[0.1,0.2,0.3],\"Property2\":\"Property2\",\"Property3\":\"Property3\"}]}";

            //Arrange//
            var seed = new MainClass
            {
                Interface1 = new Class1 { Id = 1, Key = nameof(Class1), List = [0.1m, 0.2m, 0.3m], Property1 = nameof(Class1.Property1), Property2 = nameof(Class1.Property2), Property3 = nameof(Class1.Property3) },
                Interface2 = new Class2 { Id = 1, Key = nameof(Class1), List = [0.1m, 0.2m, 0.3m], Property1 = nameof(Class1.Property1), Property2 = nameof(Class1.Property2) /* no property 3 * * * * * * * * * */ },
                Interface3 = new Class3 { Id = 1, Key = nameof(Class1), List = [0.1m, 0.2m, 0.3m], /* no property 1 * * * * * * * * * */ Property2 = nameof(Class1.Property2), Property3 = nameof(Class1.Property3) },
            };
            seed.InterfaceArray = [seed.Interface1, seed.Interface2, seed.Interface3];

            //Act//
            var actual = JsonSerializer.Serialize(seed, options).Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);

            //Assert//
            Assert.That(actual == expected);
        }
    }
}