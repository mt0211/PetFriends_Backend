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
            .Include(p => p.User)
            .Include(p => p.UserPetVaccines) // Thay đổi sang UserPetVaccines
                .ThenInclude(upv => upv.Vaccine) // Include thông tin vaccine từ bảng Vaccine
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
                Vaccinated = p.UserPetVaccines.Any(),
                VaccineNames = p.UserPetVaccines != null && p.UserPetVaccines.Any()
                    ? string.Join(", ", 
                        p.UserPetVaccines.Select(upv => 
                            upv.VaccineId != null 
                                ? upv.Vaccine.Name  // Vaccine trong hệ thống
                                : upv.Name + "(outside of system)" // Vaccine ngoài hệ thống
                        ).Where(name => !string.IsNullOrEmpty(name))
                    )
                    : "N/A"
            })
            .FirstOrDefaultAsync();
        }

        public async Task DeletePetWithVaccinesAsync(Guid petId)
        {
            //Delete in PetVaccine table
            var petVaccines = _context.UserPetVaccines.Where(pv => pv.PetId == petId);
            _context.UserPetVaccines.RemoveRange(petVaccines);

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
             try
        {
            // Đảm bảo entity được theo dõi và cập nhật
            var existingPet = await _context.Pets.FindAsync(pet.Id);
            if (existingPet != null)
            {
                // Cập nhật từng thuộc tính một cách rõ ràng
                existingPet.Name = pet.Name;
                existingPet.Gender = pet.Gender;
                existingPet.Species = pet.Species;
                existingPet.Breed = pet.Breed;
                existingPet.DateOfBirth = pet.DateOfBirth;
                existingPet.UserId = pet.UserId;
                existingPet.UserPhoneNumber = pet.UserPhoneNumber;
                existingPet.Vaccinated = pet.Vaccinated;
                
                // Ghi log để kiểm tra
                Console.WriteLine($"Updating pet: {existingPet.Id}, Name: {existingPet.Name}, UserId: {existingPet.UserId}");
            }
            else
            {
                // Nếu không tìm thấy, attach và đánh dấu là modified
                _context.Pets.Attach(pet);
                _context.Entry(pet).State = EntityState.Modified;
                Console.WriteLine($"Attaching pet: {pet.Id}, Name: {pet.Name}, UserId: {pet.UserId}");
            }
            
            // Lưu thay đổi
            await _context.SaveChangesAsync();
            Console.WriteLine("SaveChanges completed successfully");
        }
        catch (Exception ex)
        {
            // Ghi log lỗi để debug
            Console.WriteLine($"Error updating pet: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw; // Re-throw để xử lý ở lớp service
        }
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
        .Include(p => p.UserPetVaccines)
            .ThenInclude(upv => upv.Vaccine)
        .Include(p => p.UserPetVaccines)
            .ThenInclude(upv => upv.UserPetVaccineDoses)
        .FirstOrDefaultAsync(p => p.Id == petId);
        }

        public async Task RemoveAllPetVaccinesAsync(Guid petId)
        {
           // 1) Lấy danh sách ID (hoặc load toàn bộ) nhưng AsNoTracking
            var existingVaccines = await _context.UserPetVaccines
                .AsNoTracking()
                .Where(upv => upv.PetId == petId)
                .Select(upv => upv.Id)  // chỉ cần ID
                .ToListAsync();

            if (existingVaccines.Any())
            {
                // 2) Tạo list entity stub
                var stubs = existingVaccines
                    .Select(id => new UserPetVaccine { Id = id })
                    .ToList();

                // 3) Attach + Remove
                _context.UserPetVaccines.AttachRange(stubs);
                _context.UserPetVaccines.RemoveRange(stubs);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddPetVaccinesAsync(Guid petId, List<Guid> newVaccineIds)
        {
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
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

    }
}
