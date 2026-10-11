using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PatientService.Application.Errors;
using PatientService.Application.Persistence;

namespace PatientService.Infrastructure.Persistence;

public sealed class UnitOfWork(PatientDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // SQL Server remains the final protection against concurrent duplicate inserts.
            throw new ConflictException("The change conflicts with an existing record.");
        }
    }
}
