using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdminVaccineManagement.Repositories
{
    public class VaccineRepository : Repository<Vaccine>, IVaccineRepository
    {
        private readonly PetfriendsContext _context;
        public VaccineRepository(PetfriendsContext context) :base(context)
        { 
            _context = context;
        }
        public async Task<IEnumerable<dynamic>> GetListVaccines()
        {
            return await _context.Vaccines
                .Include(c => c.VaccineDoses)
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    NumberOfDoses = c.NumberOfDoses,
                    FirstInject = c.VaccineDoses.OrderBy(vd => vd.DoseNumber)
                                                .Select(vd => vd.DaysAfterPrevious)
                                                .FirstOrDefault(), // Lấy liều đầu tiên
                    Recommendation = c.Recommendation,
                    Status = c.Status,
                }).ToListAsync();
        }

        public async Task<dynamic> GetVaccineDetail(Guid id)
        {
             var vaccine = await _context.Vaccines
                .Include(v => v.VaccineDoses)
                .Where(v => v.Id == id)
                .Select(v => new
                {
                    Id = v.Id,
                    Name = v.Name,
                    NumberOfDoses = v.NumberOfDoses,
                    Injections = v.VaccineDoses
                        .OrderBy(vd => vd.DoseNumber)
                        .Select(vd => new
                        {
                            DoseNumber = vd.DoseNumber,
                            DaysAfterPrevious = vd.DaysAfterPrevious
                        }).ToList(),
                    Recommendation = v.Recommendation
                })
                .FirstOrDefaultAsync();
            return vaccine;
        }
        public async Task<User> GetUserByID(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task AddVaccineDoses(VaccineDose vaccineDose)
        {
             await _context.VaccineDoses.AddAsync(vaccineDose);
                await _context.SaveChangesAsync();
        }
        public async Task<List<VaccineDose>> GetVaccineDosesByVaccineId(Guid vaccineId)
        {
            return await _context.VaccineDoses
                .Where(d => d.VaccineId == vaccineId)
                .OrderBy(d => d.DoseNumber) // Sắp xếp theo thứ tự tiêm
                .ToListAsync();
        }
        public async Task DeleteVaccineDoses(List<VaccineDose> doses)
        {
            if (doses == null || doses.Count == 0)
                return; // Không có gì để xóa

            _context.VaccineDoses.RemoveRange(doses);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateVaccineDose(VaccineDose vaccineDose)
        {
            var existingDose = await _context.VaccineDoses.FindAsync(vaccineDose.Id);

            if (existingDose != null)
            {
                _context.VaccineDoses.Attach(vaccineDose);
                _context.Entry(vaccineDose).State = EntityState.Modified;
                await _context.SaveChangesAsync(); // Lưu vào database
            }
        }

    }
}
