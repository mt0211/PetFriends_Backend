using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PetManagementAPI.Repository.PetRepository
{
    public class PetRepository : Repository<Pet>, IPetRepository
    {
        private readonly PetfriendsContext _context;
        public PetRepository(PetfriendsContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetAllPets()
        {
            return await _context.Pets
            .GroupJoin(
                _context.Users,
                pet => pet.UserPhoneNumber,
                user => user.PhoneNumber,
                (pet, users) => new { Pet = pet, Users = users.DefaultIfEmpty() }
            )
            .Select(joinResult => new
            {
                Id = joinResult.Pet.Id,
                Name = joinResult.Pet.Name,
                Gender = joinResult.Pet.Gender,
                Species = joinResult.Pet.Species,
                Breed = joinResult.Pet.Breed,
                DateOfBirth = joinResult.Pet.DateOfBirth,
                OwnerName = joinResult.Users.FirstOrDefault() != null ? joinResult.Users.FirstOrDefault().FullName : null,
                OwnerPhoneNumber = joinResult.Pet.UserPhoneNumber,
                Vaccinated = joinResult.Pet.UserPetVaccines.Any(),
                VaccineNames = joinResult.Pet.UserPetVaccines.Any()
                    ? string.Join(", ",
                        joinResult.Pet.UserPetVaccines.Select(upv =>
                            upv.VaccineId != null
                                ? upv.Vaccine.Name
                                : upv.Name + "(outside of clinic system)"
                        ).Where(name => !string.IsNullOrEmpty(name))
                    )
                    : "N/A"  // Trả về "N/A" khi không có vaccine nào
            })
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
            return await _context.Vaccines
            .Where(v => v.Status == 0)
            .ToListAsync();
        }
        public async Task AddUserPetVaccineAsync(UserPetVaccine petVaccine)
        {
            await _context.UserPetVaccines.AddAsync(petVaccine);
            await _context.SaveChangesAsync();
        }

        public async Task<Vaccine> GetVaccineById(Guid vaccineId)
        {
            return await _context.Vaccines.FirstOrDefaultAsync(v => v.Id == vaccineId);
        }




        public async Task<dynamic> GetPetById(Guid petId)
        {
            return await _context.Pets
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
                    Vaccinated = p.Vaccinated,
                    VaccineNames = _context.UserPetVaccines
                        .Where(upv => upv.PetId == p.Id)
                        .Select(upv => upv.VaccineId != null ? upv.Vaccine.Name : upv.Name + "(outside of system)")
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList()
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task DeletePetWithVaccinesAsync(Guid petId)
        {
            // 1. Xóa các activity liên quan đến Pet này
            var activities = _context.Activities.Where(a => a.PetId == petId);
            _context.Activities.RemoveRange(activities);

            // 2. Xóa các UserPetVaccines liên quan
            var petVaccines = _context.UserPetVaccines.Where(pv => pv.PetId == petId);
            _context.UserPetVaccines.RemoveRange(petVaccines);

            // 3. Xóa Pet
            var pet = await _context.Pets.FindAsync(petId);
            if (pet != null)
            {
                _context.Pets.Remove(pet);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdatePetBasicInfo(
            Guid petId,
            string name,
            string gender,
            string species,
            string breed,
            DateTime? dateOfBirth,
            Guid? userId,
            string userPhoneNumber,
            byte vaccinated)
        {
            try {
                // Tạo một entity mới với ID đã cho
                var pet = new Pet
                {
                    Id = petId
                };
        
                // Attach entity vào context
                _context.Pets.Attach(pet);
        
                // Cập nhật các thuộc tính
                pet.Name = name;
                pet.Gender = gender;
                pet.Species = species;
                pet.Breed = breed;
                pet.DateOfBirth = dateOfBirth;
                pet.UserId = userId;
                pet.UserPhoneNumber = userPhoneNumber;
                pet.Vaccinated = vaccinated;
        
                // Đánh dấu các thuộc tính đã thay đổi
                _context.Entry(pet).Property(p => p.Name).IsModified = true;
                _context.Entry(pet).Property(p => p.Gender).IsModified = true;
                _context.Entry(pet).Property(p => p.Species).IsModified = true;
                _context.Entry(pet).Property(p => p.Breed).IsModified = true;
                _context.Entry(pet).Property(p => p.DateOfBirth).IsModified = true;
                _context.Entry(pet).Property(p => p.UserId).IsModified = true;
                _context.Entry(pet).Property(p => p.UserPhoneNumber).IsModified = true;
                _context.Entry(pet).Property(p => p.Vaccinated).IsModified = true;
        
                // Log trạng thái entity
                Console.WriteLine($"Entity state after attach and modify: {_context.Entry(pet).State}");
        
                // Lưu thay đổi
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync completed with {result} changes");
        
                if (result > 0)
                {
                    Console.WriteLine($"Basic pet info updated: {petId}, Name: {name}");
                }
                else
                {
                    Console.WriteLine($"No changes were saved to the database for pet: {petId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePetBasicInfo: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task UpdatePetVaccinesAsync(Guid petId, List<Guid> newVaccineIds)
        {
            try
            {
                // Xóa tất cả vaccine cũ
                await RemoveAllPetVaccinesAsync(petId);

                // Thêm vaccine mới
                if (newVaccineIds != null && newVaccineIds.Any())
                {
                    foreach (var vaccineId in newVaccineIds)
                    {
                        var vaccine = await GetVaccineById(vaccineId);
                        if (vaccine != null)
                        {
                            var newUserPetVaccine = new UserPetVaccine
                            {
                                Id = Guid.NewGuid(),
                                PetId = petId,
                                VaccineId = vaccineId,
                                Name = vaccine.Name,
                                NumberOfDoses = vaccine.NumberOfDoses,
                                Recommendation = vaccine.Recommendation
                            };

                            await _context.UserPetVaccines.AddAsync(newUserPetVaccine);
                            Console.WriteLine($"Added vaccine {vaccine.Name} for pet: {petId}");
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating pet vaccines: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveAllPetVaccinesAsync(Guid petId)
        {
            try
            {
                // Lấy danh sách vaccine doses trước
                var vaccineDoses = await _context.UserPetVaccineDoses
                    .Where(upvd => upvd.UserPetVaccine.PetId == petId)
                    .ToListAsync();

                if (vaccineDoses.Any())
                {
                    _context.UserPetVaccineDoses.RemoveRange(vaccineDoses);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Removed {vaccineDoses.Count} vaccine doses for pet: {petId}");
                }

                // Sau đó xóa vaccines
                var vaccines = await _context.UserPetVaccines
                    .Where(upv => upv.PetId == petId)
                    .ToListAsync();

                if (vaccines.Any())
                {
                    _context.UserPetVaccines.RemoveRange(vaccines);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Removed {vaccines.Count} vaccines for pet: {petId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing pet vaccines: {ex.Message}");
                throw;
            }
        }
    }
}
