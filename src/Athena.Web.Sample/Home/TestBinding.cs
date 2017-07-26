using System.Threading.Tasks;

namespace Athena.Web.Sample.Home
{
    public class TestBinding
    {
        public Task<TestBindingGetResult> Get(TestBindingGetInput input)
        {
            return Task.FromResult(new TestBindingGetResult(input.Slug));
        }

        public Task<TestBindingPostResult> Post(TestBindingPostInput input)
        {
            return Task.FromResult(new TestBindingPostResult(input.Name));
        }
    }

    public class TestBindingGetInput
    {
        public string Slug { get; set; }
    }

    public class TestBindingGetResult
    {
        public TestBindingGetResult(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class TestBindingPostInput
    {
        public string Name { get; set; }
    }

    public class TestBindingPostResult
    {
        public TestBindingPostResult(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}