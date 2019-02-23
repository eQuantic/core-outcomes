using eQuantic.Core.Outcomes.Sample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eQuantic.Core.Outcomes.Sample.Repositories
{
    public class ExampleRepository
    {
        private List<ExampleModel> _examples = new List<ExampleModel>
        {
            new ExampleModel { Name = "example1", Description = "The example 1" },
            new ExampleModel { Name = "example2", Description = "The example 2" },
            new ExampleModel { Name = "example3", Description = "The example 3" }
        };

        public ExampleModel Get(string name)
        {
            return _examples.FirstOrDefault(e => e.Name == name);
        }

        public IEnumerable<ExampleModel> GetAll()
        {
            return _examples;
        }
    }
}
