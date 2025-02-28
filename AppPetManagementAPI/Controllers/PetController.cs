using AppPetManagementAPI.DTOs.PetDTOs;
using AppPetManagementAPI.DTOs.ResultModel;
using AppPetManagementAPI.DTOs.VaccineDTOs;
using AppPetManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppPetManagementAPI.Controllers
{
    [ApiController]
    [Route("api/userpetmanagement")]
    public class PetController : ControllerBase
    {
        private readonly IPetService _service;
        public PetController(IPetService service)
        {
            _service = service;
        }
        [HttpGet("pet-list")]
        public async Task<IActionResult> GetListPetByUserID()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetListPetByUserID(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-pet-information")]
        public async Task<IActionResult> UpdatePetInformation(PetUpdateReqModel updatemodel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.UpdatePetInformation(token, updatemodel) ;
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("add-pet-information")]
        public async Task<IActionResult> AddPetInformation(PetAddReqModel addmodel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.AddPetInformation(token, addmodel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-pet/{PetID}")]
        public async Task<IActionResult> DeletePet(Guid PetID)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.DeletePet(token, PetID);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPost("add-pet-vaccine")]
        public async Task<IActionResult> AddPetVaccine(AddUserPetVaccineReqModel addmodel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.AddUserPetVaccine(token, addmodel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("update-pet-vaccine")]
        public async Task<IActionResult> UpdatePetVaccine(UpdateVaccineDoseReqModel updatemodel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.UpdateUserPetVaccine(token, updatemodel);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("vaccine-detail")]
        public async Task<IActionResult> GetVaccineDetailByID(Guid vaccineID)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetVaccineDetailByID(token, vaccineID);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpDelete("delete-pet-vaccine/{vaccineId}")]
        public async Task<IActionResult> DeletePetVaccine(Guid vaccineId)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var result = await _service.RemovePetVaccine(token, vaccineId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpGet("list-vaccines")]
        public async Task<IActionResult> GetListVaccines()
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            ResultModel result = await _service.GetListVaccines(token);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
