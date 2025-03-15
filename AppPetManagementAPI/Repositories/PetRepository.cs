using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AppPetManagementAPI.Repositories
{
    public class PetRepository : Repository<Pet>, IPetRepository
    {
        private readonly PetfriendsContext _context;
        public PetRepository(PetfriendsContext context):base(context) 
        {
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetListPetByUserId(Guid userId)
        {
            var pets = await _context.Pets
                .Where(p => p.UserId == userId)
                .Include(p => p.PetVaccines)
                    .ThenInclude(pv => pv.Vaccine)
                .Include(p => p.UserPetVaccines)
                    .ThenInclude(uv => uv.UserPetVaccineDoses)
                .ToListAsync();
            var result = pets.Select(p => new
            {
                p.Id,
                p.Name,
                Age = p.DateOfBirth.HasValue
                    ? (DateTime.UtcNow - p.DateOfBirth.Value).Days / 365 + "y "
                      + ((DateTime.UtcNow - p.DateOfBirth.Value).Days % 365) / 30 + "m "
                      + ((DateTime.UtcNow - p.DateOfBirth.Value).Days % 30) + "d"
                    : "Unknown",
                    DateOfBirth = p.DateOfBirth,
                Weight = p.Weight ?? 0,
                p.Gender,
                p.Breed,
                p.Species,
                p.Description,
                Vaccinations = p.PetVaccines
                    .Select(v => new
                    {
                        VaccineId = v.Id,
                        VaccineName = v.Vaccine?.Name ?? "Unknown",
                        Doses = v.Vaccine?.NumberOfDoses ?? 0,
                        LastInjectionDate = v.DateGiven.HasValue
                            ? v.DateGiven.Value.ToString("dd/MM/yyyy")
                            : "Unknown"
                    })
                    .Concat(
                        p.UserPetVaccines.Select(uv => new
                        {
                            VaccineId = uv.Id,
                            VaccineName = uv.Name,
                            Doses = uv.NumberOfDoses ?? 0,
                            LastInjectionDate = uv.UserPetVaccineDoses
                                .OrderByDescending(d => d.DateGiven)
                                .Select(d => d.DateGiven.HasValue
                                             ? d.DateGiven.Value.ToString("dd/MM/yyyy")
                                             : "Unknown")
                                .FirstOrDefault()
                        })
                    )
                    .ToList()
            });
            return result;
        }
        public async Task UpdatePetInformation(Pet pet)
        {
            _context.Pets.Attach(pet);
            _context.Entry(pet).Property(c=>c.Name).IsModified = true;
            _context.Entry(pet).Property(c => c.Gender).IsModified = true;
            _context.Entry(pet).Property(c => c.Species).IsModified = true;
            _context.Entry(pet).Property(c => c.Breed).IsModified = true;
            _context.Entry(pet).Property(c => c.DateOfBirth).IsModified = true;
            _context.Entry(pet).Property(c => c.Weight).IsModified = true;
            _context.Entry(pet).Property(c => c.Description).IsModified = true;
            await _context.SaveChangesAsync();
        }

        //ADD PET VACCINE
        public async Task AddUserPetVaccine(UserPetVaccine petVaccine)
        {
            await _context.UserPetVaccines.AddAsync(petVaccine);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserPetVaccineDose(UserPetVaccineDose petVaccineDose)
        {
            await _context.UserPetVaccineDoses.AddAsync(petVaccineDose);
            await _context.SaveChangesAsync();
        }
        public async Task<Vaccine> GetVaccineByName(string name)
        {
            return await _context.Vaccines
                .Where(v => v.Name == name)
                .FirstOrDefaultAsync();
        }

        //UPDATE VACCINE
        public async Task<UserPetVaccine> GetUserPetVaccineById(Guid userPetVaccineId)
        {
            return await _context.UserPetVaccines
                .Include(uv => uv.UserPetVaccineDoses)
                .FirstOrDefaultAsync(uv => uv.Id == userPetVaccineId);
        }

        public async Task UpdateUserPetVaccine(UserPetVaccine userPetVaccine)
        {
            _context.UserPetVaccines.Attach(userPetVaccine);
            _context.Entry(userPetVaccine).Property(x => x.Name).IsModified = true;
            _context.Entry(userPetVaccine).Property(x => x.NumberOfDoses).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserPetVaccineDose(UserPetVaccineDose petVaccineDose)
        {
            _context.UserPetVaccineDoses.Remove(petVaccineDose);
            await _context.SaveChangesAsync();
        }
        public async Task<UserPetVaccine> GetVaccineDetailByID(Guid id)
        {
            return await _context.UserPetVaccines.Include(v=>v.UserPetVaccineDoses)
                .Where(uv => uv.Id == id).FirstOrDefaultAsync();
        }
        public async Task UpdateUserPetVaccineDose(UserPetVaccineDose dose)
        {
            _context.UserPetVaccineDoses.Update(dose);
            await _context.SaveChangesAsync();
        }
        //DELETE VACCINE
        public async Task RemoveUserPetVaccine(UserPetVaccine userPetVaccine)
        {
            //Delete in UserPetVaccineDose table
            var doses = _context.UserPetVaccineDoses
                .Where(d => d.UserPetVaccineId == userPetVaccine.Id);
            _context.UserPetVaccineDoses.RemoveRange(doses);

            //Delete in UserPetVaccine table
            _context.UserPetVaccines.Remove(userPetVaccine);
            await _context.SaveChangesAsync();
        }

        public async Task<PetVaccine> GetPetVaccineById(Guid id)
        {
            return await _context.PetVaccines
                .FirstOrDefaultAsync(pv => pv.Id == id);
        }

        public async Task RemovePetVaccine(PetVaccine petVaccine)
        {
            _context.PetVaccines.Remove(petVaccine);
            await _context.SaveChangesAsync();
        }

        

        //DELETE ONE PET
        public async Task RemoveUserPetVaccinesByPetId(Guid petId)
        {
            var userVaccines = await _context.UserPetVaccines
                .Where(uv => uv.PetId == petId)
                .ToListAsync();

            foreach (var uv in userVaccines)
            {
                var doses = await _context.UserPetVaccineDoses
                    .Where(d => d.UserPetVaccineId == uv.Id)
                    .ToListAsync();

                _context.UserPetVaccineDoses.RemoveRange(doses);
            }

            _context.UserPetVaccines.RemoveRange(userVaccines);

            await _context.SaveChangesAsync();
        }

        public async Task RemovePetVaccinesByPetId(Guid petId)
        {
            var petVaccines = _context.PetVaccines.Where(pv => pv.PetId == petId);
            _context.PetVaccines.RemoveRange(petVaccines);
            await _context.SaveChangesAsync();
        }

        //RECOMMEND VACCINE
        public async Task<List<Vaccine>> GetListVaccines()
        {
            return await _context.Vaccines
            .Where(v => v.Status == 0)
            .ToListAsync();
        }

        //CHECK VACCINE SYSTEM
        public async Task<UserPetVaccine> CheckVaccineSystem(Guid vaccineId)
        {
            return await _context.UserPetVaccines
            .Where(uv => uv.Id == vaccineId)
            .FirstOrDefaultAsync();
        }

        public async Task<UserPetVaccine> CheckVaccineName(Guid petId, string vaccineName)
        {
            return await _context.UserPetVaccines
        .Where(uv => uv.PetId == petId && uv.Name == vaccineName)
        .FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetListAdmin()
        {
            return await _context.Users.Where(u => u.Role == "ADMIN").ToListAsync();
        }
        
    }
}
