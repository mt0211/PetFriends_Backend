using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PetManagementAPI.Repository.PetRepository
{
    public class PetRepository : Repository<Pet>, IPetRepository
    {
        private readonly PetFriendsContext _context;
        public PetRepository(PetFriendsContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetAllPets()
        {
            return await _context.Pets
                .GroupJoin(
                    _context.Users, // Bảng User
                    pet => pet.UserPhoneNumber, // Khóa trong bảng Pet
                    user => user.PhoneNumber,  // Khóa trong bảng User
                    (pet, users) => new { Pet = pet, Users = users.DefaultIfEmpty() } // LEFT JOIN
                )
                .Select(joinResult => new
                {
                    Id = joinResult.Pet.Id,
                    PetName = joinResult.Pet.Name,
                    Gender = joinResult.Pet.Gender,
                    Species = joinResult.Pet.Species,
                    Breed = joinResult.Pet.Breed,
                    DateOfBirth = joinResult.Pet.DateOfBirth,
                    OwnerName = joinResult.Users.FirstOrDefault() != null ? joinResult.Users.FirstOrDefault().FullName : "N/A", 
                    OwnerPhoneNumber = joinResult.Pet.UserPhoneNumber,
                    Vaccinated = joinResult.Pet.PetVaccines.Any() ? "Vaccinated" : "Not yet",
                    VaccineNames = joinResult.Pet.PetVaccines != null && joinResult.Pet.PetVaccines.Any()
    ? string.Join(", ", joinResult.Pet.PetVaccines.Select(pv => pv.Vaccine.Name))
    : "N/A"
                })
                //.OrderBy(p => p.PetName) 
                .ToListAsync();
        }

        public async Task<User> GetUserByPhoneNumber(string phoneNumber)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                return null;
            }
            return user;
        }
        public async Task<IEnumerable<Vaccine>> GetAllVaccine()
        {
            return await _context.Vaccines.ToListAsync();
        }
        public async Task AddPetVaccineAsync(PetVaccine petVaccine)
        {
            await _context.PetVaccines.AddAsync(petVaccine);
            await _context.SaveChangesAsync();
        }
        public async Task<dynamic> GetPetById(Guid petId)
        {
            return await _context.Pets
         .Include(p => p.User) // Bao gồm thông tin chủ sở hữu
         .Include(p => p.PetVaccines) // Bao gồm thông tin vaccine
         .ThenInclude(pv => pv.Vaccine) // Bao gồm chi tiết vaccine
         .Where(p => p.Id == petId)
         .Select(p => new
         {
             Id = p.Id,
             Name = p.Name,
             Gender = p.Gender,
             Species = p.Species,
             Breed = p.Breed,
             DateOfBirth = p.DateOfBirth,
             OwnerName = p.User != null ? p.User.FullName : "N/A",
             OwnerPhoneNumber = p.UserPhoneNumber,
             Vaccinated = p.PetVaccines.Any() ? "Vaccinated" : "Not yet",
             VaccineNames = p.PetVaccines != null && p.PetVaccines.Any()
    ? string.Join(", ", p.PetVaccines.Select(pv => pv.Vaccine.Name))
    : "N/A"
         })
         .FirstOrDefaultAsync();
        }

        public async Task DeletePetWithVaccinesAsync(Guid petId)
        {
            //Delete in PetVaccine table
            var petVaccines = _context.PetVaccines.Where(pv => pv.PetId == petId);
            _context.PetVaccines.RemoveRange(petVaccines);

            //Delete in Pet table
            var pet = await _context.Pets.FindAsync(petId);
            if (pet != null)
            {
                _context.Pets.Remove(pet);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdatePetAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePetVaccinesAsync(Guid petId, List<Guid> newVaccineIds)
        {
            await RemoveAllPetVaccinesAsync(petId);
            await AddPetVaccinesAsync(petId, newVaccineIds);
        }

        public async Task<Pet> GetPetByUpdate(Guid petId)
        {
            return await _context.Pets
                .Include(p => p.User)
                .Include(p => p.PetVaccines)
                .ThenInclude(pv => pv.Vaccine)
                .FirstOrDefaultAsync(p => p.Id == petId);
        }

        public async Task RemoveAllPetVaccinesAsync(Guid petId)
        {

            // Tạo một DbContext mới để cô lập việc xóa
            using (var context = new PetFriendsContext())
            {

                // Truy xuất các vaccine liên quan đến petId
                var existingVaccines = await context.PetVaccines
                    .Where(pv => pv.PetId == petId)
                    .ToListAsync();

                if (existingVaccines.Any())
                {
                    // Xóa tất cả các vaccine này
                    context.PetVaccines.RemoveRange(existingVaccines);
                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task AddPetVaccinesAsync(Guid petId, List<Guid> newVaccineIds)
        {
            if (newVaccineIds != null && newVaccineIds.Any())
            {
                var newPetVaccines = newVaccineIds.Select(vaccineId => new PetVaccine
                {
                    Id = Guid.NewGuid(),
                    PetId = petId,
                    VaccineId = vaccineId,
                    DateGiven = DateTime.UtcNow,
                    Notes = "Added during update"
                }).ToList();

                foreach (var vaccine in newPetVaccines)
                {
                    _context.Entry(vaccine).State = EntityState.Added; // Đánh dấu trạng thái thêm mới
                }

                await _context.SaveChangesAsync(); // Lưu thay đổi
            }
        }

    }
}
