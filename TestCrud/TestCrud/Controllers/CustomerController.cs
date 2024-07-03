using CrudApp.DB;
using CrudApp.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CrudApp.Controllers
{
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
    }
    [Route("/api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMongoCollection<Customer> _customerCollection;
        public CustomerController(MongoDbService mongoDbService)
        {
            _customerCollection = mongoDbService.Database?.GetCollection<Customer>("customer");

        }
        // done
        [HttpPost("/api/[controller]/add")]
        public async Task<ActionResult> AddCustomer([FromBody] Customer customer)
        {
            try
            {
                customer.CreatedAt = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;

                await _customerCollection.InsertOneAsync(customer);
                var response = new ApiResponse<Customer>
                {
                    Message = "Customer added successfully",
                    Data = customer
                };
                return CreatedAtAction(nameof(GetById), new { id = customer.Id.ToString() }, response);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error inserting data into the database.");
            }
        }
        // done
        [HttpGet("/api/[controller]/get_all")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
        {
            try
            {
                return await _customerCollection.Find(FilterDefinition<Customer>.Empty).ToListAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data from the database.");
            }
        }
        // done
        [HttpGet("/api/[controller]/get_by_id/{id}")]
        public async Task<ActionResult<Customer>> GetById(string id)
        {
            try
            {
                var filter = Builders<Customer>.Filter.Eq("_id", new ObjectId(id));
                var customer = await _customerCollection.Find(filter).FirstOrDefaultAsync();

                if (customer == null)
                {
                    return NotFound("Customer Not Found.");
                }

                return Ok(customer);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid ID format.");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer data from the database.");

            }
        }
        // done
        [HttpPut("/api/[controller]/update/{id}")]
        public async Task<ActionResult<IEnumerable<Customer>>> UpdateById(string id, [FromBody] Customer updatedCustomer)
        {
            try
            {
                var filter = Builders<Customer>.Filter.Eq("_id", new ObjectId(id));
                var update = Builders<Customer>.Update.Set(c => c.Name, updatedCustomer.Name);
                var customer = await _customerCollection.Find(filter).FirstOrDefaultAsync();
                if (customer == null)
                {
                    return NotFound("Customer Not Found.");
                }

                var result = await _customerCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    var response = new ApiResponse<Customer>
                    {
                        Message = "Customer updated successfully",
                        Data = customer
                    };
                    return CreatedAtAction(nameof(GetById), new { id = customer.Id.ToString() }, response);
                }
                else
                {
                    return NotFound("No Moficitation Applied");
                }
            }
            catch (FormatException)
            {
                return BadRequest("Invalid ID format.");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating customer data from the database.");



            }
        }
        // done
        [HttpDelete("/api/[controller]/delete/{id}")]
        public async Task<ActionResult<IEnumerable<Customer>>> DeleteById(string id)
        {
            try
            {
                var filter = Builders<Customer>.Filter.Eq("_id", new ObjectId(id));
                var customer = await _customerCollection.Find(filter).FirstOrDefaultAsync();
                if (customer == null)
                {
                    return NotFound("Customer Not Found.");
                }

                var result = await _customerCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    var response = new ApiResponse<Customer>
                    {
                        Message = "Customer deleted successfully",
                        Data = customer
                    };
                    return CreatedAtAction(nameof(GetById), new { id = customer.Id.ToString() }, response);
                }
                else
                {
                    return NotFound("No Deletion Applied");
                }
            }
            catch (FormatException)
            {
                return BadRequest("Invalid ID format.");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using a logging framework)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting customer data from the database.");

            }
        }
    }
}

