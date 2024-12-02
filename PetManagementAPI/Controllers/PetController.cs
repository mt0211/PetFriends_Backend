using Microsoft.AspNetCore.Mvc;
using PetManagementAPI.DTOs.PetDTOs;
using PetManagementAPI.Services;

namespace PetManagementAPI.Controllers
{
    [ApiController]
    [Route("api/pet")]
    public class PetController : ControllerBase
    {
        private readonly IPetService _petService;
        public PetController(IPetService petService)
        {
            _petService = petService;
        }
        [HttpGet("pet-list")]
        public async Task<IActionResult> GetPetList(int page)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _petService.GetAllPet(token,page);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("vaccine-list")]
        public async Task<IActionResult> GetVaccineList()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _petService.GetAllVaccine(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("add-pet")]
        public async Task<IActionResult> AddPet([FromBody] AddPetReqModel petDto)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _petService.AddNewPet(token, petDto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("pet-detail")]
        public async Task<IActionResult> GetPetDetail(Guid id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _petService.GetPetDetail(token,id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("delete-pet/{id}")]
        public async Task<IActionResult> DeletePet(Guid id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _petService.DeletePet(token, id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update-pet")]
        public async Task<IActionResult> UpdatePet([FromBody] UpdatePetReqModel petDto)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _petService.UpdatePet(token, petDto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
