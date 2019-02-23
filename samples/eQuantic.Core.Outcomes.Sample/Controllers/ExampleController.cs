using eQuantic.Core.Outcomes.Sample.Models;
using eQuantic.Core.Outcomes.Sample.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eQuantic.Core.Outcomes.Sample.Controllers
{
    public class ExampleController : Controller
    {
        protected ExampleRepository ExampleRepository { get; private set; }
        public ExampleController()
        {
            ExampleRepository = new ExampleRepository();
        }
        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {
            var resultBuilder = Outcome.FromItemResult<ExampleModel>();

            try
            {
                var example = ExampleRepository.Get(name);
                if (example == null)
                {
                    resultBuilder = resultBuilder
                        .WithError()
                        .WithStatus(ResultStatus.NotFound)
                        .WithMessage("The example cannot be found.");

                    return NotFound(resultBuilder.Result());
                }

                resultBuilder = resultBuilder
                    .WithSuccess()
                    .WithItem(example);

                return Ok(resultBuilder.Result());
            }
            catch (Exception ex)
            {
                resultBuilder = resultBuilder
                    .WithError()
                    .WithStatus(ResultStatus.Error)
                    .WithException(ex);

                return StatusCode(StatusCodes.Status500InternalServerError, resultBuilder.Result());
            }
            
        }
    }
}
